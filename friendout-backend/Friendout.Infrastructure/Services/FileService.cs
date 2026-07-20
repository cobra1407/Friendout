using System;
using System.IO;
using System.Threading.Tasks;
using Friendout.Domain.Models;
using Friendout.Infrastructure.Enums;
using Friendout.Infrastructure.Interfaces;
using Friendout.Infrastructure.Options;
using Friendout.Infrastructure.Utils;
using Microsoft.Extensions.Options;
using SkiaSharp;

namespace Friendout.Infrastructure.Services
{
    public class FileService : IFileService
    {
        private readonly string _baseFolder;
        private readonly string _appUrl;
        private readonly IFileValidationService _validationService;

        /// <summary>
        /// Initializes the FileService with the base path for file storage.
        /// </summary>
        /// <param name="baseFolder">Base path where files will be stored (ex: "C:\app\uploads")</param>
        /// <param name="validationService">File validation service</param>
        /// <param name="appOptions">Application options containing the public URL</param>
        public FileService(string baseFolder, IFileValidationService validationService, IOptions<AppOptions> appOptions)
        {
            if (string.IsNullOrWhiteSpace(baseFolder))
                throw new ArgumentException("Base folder cannot be empty.", nameof(baseFolder));

            _baseFolder = Path.Combine(baseFolder, "uploads");
            _appUrl = appOptions.Value.Url.TrimEnd('/');
            _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
            Directory.CreateDirectory(_baseFolder);
        }

        // Avatars are always resized down to a fixed square and re-encoded as WebP, regardless
        // of the original format or size. This keeps storage small and predictable rather than
        // rejecting large uploads outright — a 20MB phone photo becomes a ~15-45KB avatar.
        private const int AvatarMaxDimension = 512;
        private const int AvatarWebpQuality = 82;

        // Activity images are only ever displayed as a small card thumbnail or within the
        // details page, never at full resolution — resizing down avoids shipping multi-MB
        // phone photos to every visitor (and decoding them client-side), which matters a lot
        // when the app is self-hosted on modest hardware like a Raspberry Pi.
        private const int ActivityImageMaxWidth = 1280;
        private const int ActivityImageWebpQuality = 80;

        public async Task<string> SaveFileAsync(FileUpload file, FileCategory category)
        {
            if (file is null || file.Length == 0)
                throw new ArgumentException("Invalid file.");

            // Security validation
            var validationResult = _validationService.ValidateFile(file, category);
            if (!validationResult.IsValid)
                throw new ArgumentException(validationResult.ErrorMessage ?? "Invalid file.");

            var folder = FileCategoryMapper.ResolveFolder(category);
            var targetFolder = Path.Combine(_baseFolder, folder);
            Directory.CreateDirectory(targetFolder);

            var isAvatar = category == FileCategory.UserAvatar;
            var isActivityImage = category == FileCategory.ActivityImage;
            // Both avatars and activity images are re-encoded as WebP after resizing, so the
            // stored extension reflects that — the original format/extension is discarded.
            var extension = (isAvatar || isActivityImage) ? ".webp" : Path.GetExtension(file.FileName).ToLowerInvariant();

            // Use Guid to avoid conflicts and security attacks
            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(targetFolder, fileName);

            // Protection against path traversal
            if (!filePath.StartsWith(_baseFolder, StringComparison.OrdinalIgnoreCase))
                throw new UnauthorizedAccessException("Unauthorized file path.");

            if (isAvatar)
            {
                await SaveResizedAvatarAsync(file.Stream, filePath);
            }
            else if (isActivityImage)
            {
                await SaveResizedActivityImageAsync(file.Stream, filePath);
            }
            else
            {
                await using var output = new FileStream(filePath, FileMode.Create);
                await file.Stream.CopyToAsync(output);
            }

            return fileName;
        }

        /// <summary>
        /// Center-crops the image to a square and resizes it to a fixed size, then re-encodes
        /// it as lossy WebP. SkiaSharp has no built-in "crop to fill" resize mode, so the crop
        /// rectangle is computed manually before resizing.
        /// Decoding/resizing/encoding is CPU-bound and fully synchronous in SkiaSharp, so it
        /// runs on a thread pool thread via Task.Run rather than blocking the request thread.
        /// </summary>
        private static Task SaveResizedAvatarAsync(Stream sourceStream, string destinationPath)
        {
            return Task.Run(() =>
            {
                using var original = SKBitmap.Decode(sourceStream)
                    ?? throw new InvalidOperationException("Unsupported or corrupt image.");
                using var originalImage = SKImage.FromBitmap(original);

                var cropSize = Math.Min(original.Width, original.Height);
                var cropX = (original.Width - cropSize) / 2;
                var cropY = (original.Height - cropSize) / 2;
                var sourceRect = new SKRect(cropX, cropY, cropX + cropSize, cropY + cropSize);
                var destRect = new SKRect(0, 0, AvatarMaxDimension, AvatarMaxDimension);

                // SKFilterQuality/SKPaint.FilterQuality are obsolete as of SkiaSharp 4 — sampling
                // quality is now passed explicitly to DrawImage via SKSamplingOptions instead.
                var sampling = new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear);

                using var surface = SKSurface.Create(new SKImageInfo(AvatarMaxDimension, AvatarMaxDimension));
                var canvas = surface.Canvas;
                canvas.Clear(SKColors.Transparent);
                canvas.DrawImage(originalImage, sourceRect, destRect, sampling);
                using var resizedImage = surface.Snapshot();

                EncodeAndSave(resizedImage, destinationPath, AvatarWebpQuality);
            });
        }

        /// <summary>
        /// Downscales the image to fit within a max width/height (keeping aspect ratio, never
        /// upscaling smaller images) and re-encodes it as lossy WebP.
        /// </summary>
        private static Task SaveResizedActivityImageAsync(Stream sourceStream, string destinationPath)
        {
            return Task.Run(() =>
            {
                using var original = SKBitmap.Decode(sourceStream)
                    ?? throw new InvalidOperationException("Unsupported or corrupt image.");

                var scale = Math.Min(1.0, (double)ActivityImageMaxWidth / Math.Max(original.Width, original.Height));

                if (scale >= 1.0)
                {
                    // Already within bounds — save as-is (still re-encoded to WebP below).
                    using var originalImage = SKImage.FromBitmap(original);
                    EncodeAndSave(originalImage, destinationPath, ActivityImageWebpQuality);
                    return;
                }

                var targetWidth = Math.Max(1, (int)Math.Round(original.Width * scale));
                var targetHeight = Math.Max(1, (int)Math.Round(original.Height * scale));
                var targetInfo = new SKImageInfo(targetWidth, targetHeight, original.ColorType, original.AlphaType);

                // SKFilterQuality is obsolete as of SkiaSharp 4 — SKSamplingOptions replaces it.
                var sampling = new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear);
                using var resizedBitmap = original.Resize(targetInfo, sampling)
                    ?? throw new InvalidOperationException("Failed to resize activity image.");
                using var resizedImage = SKImage.FromBitmap(resizedBitmap);

                EncodeAndSave(resizedImage, destinationPath, ActivityImageWebpQuality);
            });
        }

        private static void EncodeAndSave(SKImage image, string destinationPath, int quality)
        {
            using var data = image.Encode(SKEncodedImageFormat.Webp, quality);
            using var output = File.OpenWrite(destinationPath);
            data.SaveTo(output);
        }

        public string GetFileUrl(string fileName, FileCategory category)
        {
            var folder = FileCategoryMapper.ResolveFolder(category);
            return $"{_appUrl}/uploads/{folder}/{fileName}".Replace("\\", "/");
        }

        public string GetFilePath(string fileName, FileCategory category)
        {
            var folder = FileCategoryMapper.ResolveFolder(category);
            return Path.Combine(_baseFolder, folder, fileName);
        }

        // New version that exploits FileCategory instead of subFolder
        public async Task DeleteFileAsync(string fileName, FileCategory category)
        {
            // Uses the folder defined by FileCategory to build the path
            var folder = FileCategoryMapper.ResolveFolder(category);
            var filePath = Path.Combine(_baseFolder, folder, fileName);

            // Secures access to the path (additional protection)
            if (!filePath.StartsWith(_baseFolder, StringComparison.OrdinalIgnoreCase))
                throw new UnauthorizedAccessException("Unauthorized file path.");

            if (File.Exists(filePath))
            {
                await Task.Run(() => File.Delete(filePath));
            }
        }
    }
}
