using System.IO;
using System.Threading.Tasks;
using Friendout.Domain.Models;
using Friendout.Infrastructure.Enums;
using Friendout.Infrastructure.Interfaces;
using Friendout.Infrastructure.Options;
using Friendout.Infrastructure.Services;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using FluentAssertions;

namespace Friendout.Test;

public class FileServiceTests
{
    private string _baseFolder = null!;
    private IFileValidationService _validationService = null!;
    private IOptions<AppOptions> _appOptions = null!;

    [SetUp]
    public void Setup()
    {
        _baseFolder = Path.Combine(Path.GetTempPath(), "Friendout_FileServiceTests");
        if (Directory.Exists(_baseFolder))
        {
            Directory.Delete(_baseFolder, recursive: true);
        }

        _validationService = new AlwaysValidFileValidationService();
        _appOptions = Microsoft.Extensions.Options.Options.Create(new AppOptions { Url = "https://localhost" });
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_baseFolder))
        {
            Directory.Delete(_baseFolder, recursive: true);
        }
    }

    [Test]
    public async Task SaveFileAsync_WithValidFile_CreatesFileOnDisk()
    {
        // Arrange
        var service = new FileService(_baseFolder, _validationService, _appOptions);
        var content = new byte[] { 1, 2, 3, 4 };
        var file = new FileUpload
        {
            FileName = "photo.png",
            ContentType = "image/png",
            Length = content.Length,
            Stream = new MemoryStream(content)
        };

        // Act
        var fileName = await service.SaveFileAsync(file, FileCategory.ActivityImage);

        // Assert
        fileName.Should().NotBeNullOrWhiteSpace();

        var physicalPath = service.GetFilePath(fileName, FileCategory.ActivityImage);
        File.Exists(physicalPath).Should().BeTrue();
    }

    [Test]
    public async Task DeleteFileAsync_RemovesExistingFile()
    {
        // Arrange
        var service = new FileService(_baseFolder, _validationService, _appOptions);
        var content = new byte[] { 1, 2, 3, 4 };
        var file = new FileUpload
        {
            FileName = "photo.png",
            ContentType = "image/png",
            Length = content.Length,
            Stream = new MemoryStream(content)
        };

        var fileName = await service.SaveFileAsync(file, FileCategory.ActivityImage);
        var physicalPath = service.GetFilePath(fileName, FileCategory.ActivityImage);
        File.Exists(physicalPath).Should().BeTrue();

        // Act
        await service.DeleteFileAsync(fileName, FileCategory.ActivityImage);

        // Assert
        File.Exists(physicalPath).Should().BeFalse();
    }

    private sealed class AlwaysValidFileValidationService : IFileValidationService
    {
        public ValidationResult ValidateFile(FileUpload file, FileCategory category)
        {
            return ValidationResult.Success();
        }
    }
}

