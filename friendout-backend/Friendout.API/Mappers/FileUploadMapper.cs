using Friendout.Domain.Models;
using Microsoft.AspNetCore.Http;

namespace friendout_backend.Mappers;

/// <summary>
/// Mapper pour convertir les types spécifiques à ASP.NET Core vers les modèles de domaine.
/// Cette classe permet d'isoler les dépendances web de la couche Domain.
/// </summary>
public static class FileUploadMapper
{
    /// <summary>
    /// Convertit un IFormFile en FileUpload (modèle de domaine).
    /// </summary>
    /// <param name="formFile">Le fichier uploadé depuis ASP.NET Core</param>
    /// <returns>Un FileUpload ou null si formFile est null</returns>
    public static FileUpload? ToFileUpload(IFormFile? formFile)
    {
        if (formFile == null || formFile.Length == 0)
            return null;

        return new FileUpload
        {
            FileName = formFile.FileName,
            ContentType = formFile.ContentType,
            Length = formFile.Length,
            Stream = formFile.OpenReadStream()
        };
    }
}


