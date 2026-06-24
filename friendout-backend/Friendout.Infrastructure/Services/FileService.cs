using System;
using System.IO;
using System.Threading.Tasks;
using Friendout.Domain.Models;
using Friendout.Infrastructure.Enums;
using Friendout.Infrastructure.Interfaces;
using Friendout.Infrastructure.Options;
using Friendout.Infrastructure.Utils;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

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

        // Avatars are always resized down to a fixed square and re-encoded as JPEG, regardless
        // of the original format or size. This keeps storage small and predictable rather than
        // rejecting large uploads outright — a 20MB phone photo becomes a ~20-60KB avatar.
        private const int AvatarMaxDimension = 512;
        private const int AvatarJpegQuality = 82;

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
            var extension = isAvatar ? ".jpg" : Path.GetExtension(file.FileName).ToLowerInvariant();

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
            else
            {
                await using var output = new FileStream(filePath, FileMode.Create);
                await file.Stream.CopyToAsync(output);
            }

            return fileName;
        }

        /// <summary>
        /// Resizes the image to fit within a fixed square (cropping to fill, not letterboxing)
        /// and re-encodes it as JPEG at a controlled quality, regardless of the input format.
        /// </summary>
        private static async Task SaveResizedAvatarAsync(Stream sourceStream, string destinationPath)
        {
            using var image = await SixLabors.ImageSharp.Image.LoadAsync(sourceStream);

            image.Mutate(ctx => ctx.Resize(new ResizeOptions
            {
                Mode = ResizeMode.Crop,
                Size = new Size(AvatarMaxDimension, AvatarMaxDimension)
            }));

            var encoder = new JpegEncoder { Quality = AvatarJpegQuality };
            await image.SaveAsync(destinationPath, encoder);
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
