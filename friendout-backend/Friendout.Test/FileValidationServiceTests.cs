using System.IO;
using System.Text;
using Friendout.Domain.Models;
using Friendout.Infrastructure.Enums;
using Friendout.Infrastructure.Services;
using NUnit.Framework;
using FluentAssertions;

namespace Friendout.Test;

public class FileValidationServiceTests
{
    private FileValidationService _service = null!;

    [SetUp]
    public void Setup()
    {
        _service = new FileValidationService();
    }

    private static FileUpload CreateFile(
        string fileName,
        string contentType,
        byte[] contentBytes)
    {
        var stream = new MemoryStream(contentBytes);
        return new FileUpload
        {
            FileName = fileName,
            ContentType = contentType,
            Length = contentBytes.Length,
            Stream = stream
        };
    }

    [Test]
    public void ValidateFile_WithValidPngAvatar_ReturnsSuccess()
    {
        // Arrange - valid PNG magic numbers
        var pngHeader = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
        var file = CreateFile("avatar.png", "image/png", pngHeader.Concat(Encoding.UTF8.GetBytes("data")).ToArray());

        // Act
        var result = _service.ValidateFile(file, FileCategory.UserAvatar);

        // Assert
        result.IsValid.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
    }

    [Test]
    public void ValidateFile_WithTooLargeFile_ReturnsFailure()
    {
        // Arrange - 100 MB fake file (only length matters, not stream size)
        var header = new byte[] { 0xFF, 0xD8, 0xFF };
        var file = new FileUpload
        {
            FileName = "big.jpg",
            ContentType = "image/jpeg",
            Length = 100L * 1024 * 1024,
            Stream = new MemoryStream(header)
        };

        // Act
        var result = _service.ValidateFile(file, FileCategory.UserAvatar);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrWhiteSpace();
    }

    [Test]
    public void ValidateFile_WithDisallowedExtension_ReturnsFailure()
    {
        // Arrange
        var pngHeader = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
        var file = CreateFile("avatar.exe", "image/png", pngHeader);

        // Act
        var result = _service.ValidateFile(file, FileCategory.UserAvatar);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Extensions autorisées");
    }

    [Test]
    public void ValidateFile_WithPathTraversalInName_ReturnsFailure()
    {
        // Arrange
        var pngHeader = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
        var file = CreateFile("../avatar.png", "image/png", pngHeader);

        // Act
        var result = _service.ValidateFile(file, FileCategory.UserAvatar);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("nom de fichier");
    }
}

