using System.Threading.Tasks;
using Friendout.Domain.Models;
using Friendout.Infrastructure.Enums;

namespace Friendout.Infrastructure.Interfaces;

/// <summary>
/// File management service - WITHOUT ASP.NET Core dependency
/// </summary>
public interface IFileService
{
    
    /// <summary>
    /// Saves a file to disk
    /// </summary>
    /// <param name="fileUpload">File to save</param>
    /// <param name="fileCategory">File category (ex: "activities") each category has a different localisation</param>
    /// <returns>Saved file unique name</returns>
    Task<string> SaveFileAsync(FileUpload fileUpload, FileCategory fileCategory);
    
    /// <summary>
    /// Deletes a file from disk
    /// </summary>
    Task DeleteFileAsync(string fileName, FileCategory fileCategory);
    
    /// <summary>
    /// Gets the full path of a file
    /// </summary>
    string GetFilePath(string fileName, FileCategory category);
    
    /// <summary>
    /// Gets the public URL of a file
    /// </summary>
    string GetFileUrl(string fileName, FileCategory category);
}