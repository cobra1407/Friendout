using Friendout.Domain.Models;
using Friendout.Infrastructure.Enums;

namespace Friendout.Infrastructure.Interfaces;

/// <summary>
/// Service de validation des fichiers uploadés pour la sécurité.
/// </summary>
public interface IFileValidationService
{
    /// <summary>
    /// Valide un fichier selon sa catégorie.
    /// </summary>
    /// <param name="file">Le fichier à valider</param>
    /// <param name="category">La catégorie du fichier</param>
    /// <returns>Résultat de la validation avec message d'erreur si invalide</returns>
    ValidationResult ValidateFile(FileUpload file, FileCategory category);
}

/// <summary>
/// Résultat d'une validation de fichier.
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; init; }
    public string? ErrorMessage { get; init; }

    public static ValidationResult Success() => new() { IsValid = true };
    public static ValidationResult Failure(string errorMessage) => new() { IsValid = false, ErrorMessage = errorMessage };
}


