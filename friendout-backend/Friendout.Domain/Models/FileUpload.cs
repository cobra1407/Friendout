namespace Friendout.Domain.Models;

/// <summary>
/// Abstraction d'un fichier uploadé, indépendante du framework web
/// </summary>
public class FileUpload
{
    public required string FileName { get; init; }
    public required string ContentType { get; init; }
    public required long Length { get; init; }
    public required Stream Stream { get; init; }
}