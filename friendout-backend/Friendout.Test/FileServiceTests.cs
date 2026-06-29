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
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Image = SixLabors.ImageSharp.Image;

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

    /// <summary>Builds a real (decodable) in-memory PNG of the given size, for tests that exercise image resizing.</summary>
    private static byte[] CreateTestPng(int width, int height)
    {
        using var image = new Image<Rgba32>(width, height);
        using var ms = new MemoryStream();
        image.SaveAsPng(ms);
        return ms.ToArray();
    }

    [Test]
    public async Task SaveFileAsync_WithValidFile_CreatesFileOnDisk()
    {
        // Arrange
        var service = new FileService(_baseFolder, _validationService, _appOptions);
        var content = CreateTestPng(10, 10);
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
        var content = CreateTestPng(10, 10);
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

    [Test]
    public async Task SaveFileAsync_ActivityImage_IsDownscaledAndReencodedAsJpeg()
    {
        // Arrange: an oversized image (wider than the 1280px cap) saved as PNG.
        var service = new FileService(_baseFolder, _validationService, _appOptions);
        var content = CreateTestPng(2000, 1000);
        var file = new FileUpload
        {
            FileName = "big-photo.png",
            ContentType = "image/png",
            Length = content.Length,
            Stream = new MemoryStream(content)
        };

        // Act
        var fileName = await service.SaveFileAsync(file, FileCategory.ActivityImage);

        // Assert
        fileName.Should().EndWith(".webp");

        var physicalPath = service.GetFilePath(fileName, FileCategory.ActivityImage);
        using var savedImage = await Image.LoadAsync(physicalPath);
        savedImage.Width.Should().Be(1280);
        savedImage.Height.Should().Be(640);
    }

    [Test]
    public async Task SaveFileAsync_ActivityImage_DoesNotUpscaleSmallImages()
    {
        // Arrange: an image already smaller than the 1280px cap.
        var service = new FileService(_baseFolder, _validationService, _appOptions);
        var content = CreateTestPng(200, 100);
        var file = new FileUpload
        {
            FileName = "small-photo.png",
            ContentType = "image/png",
            Length = content.Length,
            Stream = new MemoryStream(content)
        };

        // Act
        var fileName = await service.SaveFileAsync(file, FileCategory.ActivityImage);

        // Assert
        var physicalPath = service.GetFilePath(fileName, FileCategory.ActivityImage);
        using var savedImage = await Image.LoadAsync(physicalPath);
        savedImage.Width.Should().Be(200);
        savedImage.Height.Should().Be(100);
    }

    [Test]
    public async Task SaveFileAsync_Avatar_IsResizedToFixedSquareAndReencodedAsJpeg()
    {
        // Arrange
        var service = new FileService(_baseFolder, _validationService, _appOptions);
        var content = CreateTestPng(2000, 1000);
        var file = new FileUpload
        {
            FileName = "avatar.png",
            ContentType = "image/png",
            Length = content.Length,
            Stream = new MemoryStream(content)
        };

        // Act
        var fileName = await service.SaveFileAsync(file, FileCategory.UserAvatar);

        // Assert
        fileName.Should().EndWith(".webp");

        var physicalPath = service.GetFilePath(fileName, FileCategory.UserAvatar);
        using var savedImage = await Image.LoadAsync(physicalPath);
        savedImage.Width.Should().Be(512);
        savedImage.Height.Should().Be(512);
    }

    [Test]
    public async Task SaveFileAsync_ActivityAttachment_IsStoredAsIs_NotResized()
    {
        // Attachments keep their original bytes/extension — only ActivityImage and
        // UserAvatar are resized/re-encoded.
        var service = new FileService(_baseFolder, _validationService, _appOptions);
        var content = CreateTestPng(2000, 1000);
        var file = new FileUpload
        {
            FileName = "document.png",
            ContentType = "image/png",
            Length = content.Length,
            Stream = new MemoryStream(content)
        };

        var fileName = await service.SaveFileAsync(file, FileCategory.ActivityAttachment);

        fileName.Should().EndWith(".png");
        var physicalPath = service.GetFilePath(fileName, FileCategory.ActivityAttachment);
        var savedBytes = await File.ReadAllBytesAsync(physicalPath);
        savedBytes.Should().BeEquivalentTo(content);
    }

    private sealed class AlwaysValidFileValidationService : IFileValidationService
    {
        public ValidationResult ValidateFile(FileUpload file, FileCategory category)
        {
            return ValidationResult.Success();
        }
    }
}

