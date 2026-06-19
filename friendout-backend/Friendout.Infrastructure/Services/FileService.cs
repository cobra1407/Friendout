using System;
using System.IO;
using System.Threading.Tasks;
using Friendout.Domain.Models;
using Friendout.Infrastructure.Enums;
using Friendout.Infrastructure.Interfaces;
using Friendout.Infrastructure.Options;
using Friendout.Infrastructure.Utils;
using Microsoft.Extensions.Options;

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

        public async Task<string> SaveFileAsync(FileUpload file, FileCategory category)
        {
            if (file is null || file.Length == 0)
                throw new ArgumentException("Invalid file.");

            // Security validation
            var validationResult = _validationService.ValidateFile(file, category);
            if (!validationResult.IsValid)
                throw new ArgumentException(validationResult.ErrorMessage ?? "Invalid file.");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var folder = FileCategoryMapper.ResolveFolder(category);

            var targetFolder = Path.Combine(_baseFolder, folder);
            Directory.CreateDirectory(targetFolder);

            // Use Guid to avoid conflicts and security attacks
            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(targetFolder, fileName);

            // Protection against path traversal
            if (!filePath.StartsWith(_baseFolder, StringComparison.OrdinalIgnoreCase))
                throw new UnauthorizedAccessException("Unauthorized file path.");

            await using var output = new FileStream(filePath, FileMode.Create);
            await file.Stream.CopyToAsync(output);

            return fileName;
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
