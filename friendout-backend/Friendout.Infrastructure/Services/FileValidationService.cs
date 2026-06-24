using System.Collections.Generic;
using System.IO;
using System.Linq;
using Friendout.Domain.Models;
using Friendout.Infrastructure.Enums;
using Friendout.Infrastructure.Interfaces;

namespace Friendout.Infrastructure.Services;

/// <summary>
/// Service de validation des fichiers uploadés pour la sécurité.
/// Valide le type, la taille, l'extension et les magic numbers.
/// </summary>
public class FileValidationService : IFileValidationService
{
    // Taille maximale par catégorie (en bytes)
    private static readonly Dictionary<FileCategory, long> MaxSizes = new()
    {
        // Avatars are resized + re-encoded to ~512x512 JPEG after upload (see FileService),
        // so the input limit can comfortably allow full-size phone photos.
        { FileCategory.UserAvatar, 20 * 1024 * 1024 },     // 20 MB
        { FileCategory.ActivityImage, 10 * 1024 * 1024 },  // 10 MB
        { FileCategory.ActivityAttachment, 20 * 1024 * 1024 }, // 20 MB
        { FileCategory.Document, 50 * 1024 * 1024 }        // 50 MB
    };

    // Extensions autorisées par catégorie
    private static readonly Dictionary<FileCategory, HashSet<string>> AllowedExtensions = new()
    {
        { FileCategory.UserAvatar, new HashSet<string> { ".jpg", ".jpeg", ".png", ".gif", ".webp" } },
        { FileCategory.ActivityImage, new HashSet<string> { ".jpg", ".jpeg", ".png", ".gif", ".webp" } },
        { FileCategory.ActivityAttachment, new HashSet<string> { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".pdf", ".doc", ".docx", ".zip" } },
        { FileCategory.Document, new HashSet<string> { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".txt" } }
    };

    // Content-Types autorisés par catégorie
    private static readonly Dictionary<FileCategory, HashSet<string>> AllowedContentTypes = new()
    {
        { FileCategory.UserAvatar, new HashSet<string> { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" } },
        { FileCategory.ActivityImage, new HashSet<string> { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" } },
        { FileCategory.ActivityAttachment, new HashSet<string> { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp", "application/pdf", "application/msword", "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "application/zip" } },
        { FileCategory.Document, new HashSet<string> { "application/pdf", "application/msword", "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "application/vnd.ms-excel", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "text/plain" } }
    };

    // Magic numbers pour les images (premiers bytes du fichier)
    private static readonly Dictionary<string, byte[]> ImageMagicNumbers = new()
    {
        { ".jpg", new byte[] { 0xFF, 0xD8, 0xFF } },
        { ".jpeg", new byte[] { 0xFF, 0xD8, 0xFF } },
        { ".png", new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } },
        { ".gif", new byte[] { 0x47, 0x49, 0x46, 0x38 } }, // GIF87a ou GIF89a
        { ".webp", new byte[] { 0x52, 0x49, 0x46, 0x46 } } // RIFF (WebP commence par RIFF)
    };

    public ValidationResult ValidateFile(FileUpload file, FileCategory category)
    {
        if (file == null || file.Length == 0)
            return ValidationResult.Failure("Le fichier est vide ou invalide.");

        // 1. Validation de la taille
        if (!MaxSizes.TryGetValue(category, out var maxSize))
            return ValidationResult.Failure($"Catégorie de fichier non supportée: {category}");

        if (file.Length > maxSize)
        {
            var maxSizeMB = maxSize / (1024.0 * 1024.0);
            return ValidationResult.Failure($"Le fichier est trop volumineux. Taille maximale autorisée: {maxSizeMB:F1} MB");
        }

        // 2. Validation de l'extension
        var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
        if (string.IsNullOrEmpty(extension))
            return ValidationResult.Failure("Le fichier doit avoir une extension.");

        if (!AllowedExtensions.TryGetValue(category, out var allowedExts) || !allowedExts.Contains(extension))
        {
            var allowedList = string.Join(", ", allowedExts);
            return ValidationResult.Failure($"Extension non autorisée. Extensions autorisées: {allowedList}");
        }

        // 3. Validation du Content-Type
        if (string.IsNullOrWhiteSpace(file.ContentType))
            return ValidationResult.Failure("Le type de contenu (Content-Type) est requis.");

        if (!AllowedContentTypes.TryGetValue(category, out var allowedTypes) || !allowedTypes.Contains(file.ContentType.ToLowerInvariant()))
        {
            return ValidationResult.Failure($"Type de contenu non autorisé: {file.ContentType}");
        }

        // 4. Validation des magic numbers pour les images (sécurité renforcée)
        if (category == FileCategory.UserAvatar || category == FileCategory.ActivityImage)
        {
            var magicNumberValidation = ValidateImageMagicNumber(file, extension);
            if (!magicNumberValidation.IsValid)
                return magicNumberValidation;
        }

        // 5. Validation du nom de fichier (protection contre path traversal)
        if (file.FileName.Contains("..") || file.FileName.Contains("/") || file.FileName.Contains("\\"))
            return ValidationResult.Failure("Le nom de fichier contient des caractères non autorisés.");

        return ValidationResult.Success();
    }

    /// <summary>
    /// Valide les magic numbers d'une image pour s'assurer que le fichier est vraiment une image.
    /// </summary>
    private ValidationResult ValidateImageMagicNumber(FileUpload file, string extension)
    {
        if (!ImageMagicNumbers.TryGetValue(extension, out var expectedMagicNumber))
            return ValidationResult.Success(); // Si on ne connaît pas le magic number, on accepte (pour les nouveaux formats)

        try
        {
            // Lire les premiers bytes du fichier
            var buffer = new byte[expectedMagicNumber.Length];
            var originalPosition = file.Stream.Position;
            file.Stream.Position = 0;
            var bytesRead = file.Stream.Read(buffer, 0, expectedMagicNumber.Length);
            file.Stream.Position = originalPosition; // Restaurer la position

            if (bytesRead < expectedMagicNumber.Length)
                return ValidationResult.Failure("Impossible de lire les en-têtes du fichier.");

            // Vérifier les magic numbers
            if (extension == ".gif")
            {
                // GIF peut être GIF87a ou GIF89a
                if (!buffer.Take(3).SequenceEqual(new byte[] { 0x47, 0x49, 0x46 }) ||
                    (buffer[3] != 0x38 && buffer[3] != 0x39))
                {
                    return ValidationResult.Failure("Le fichier n'est pas une image GIF valide.");
                }
            }
            else if (extension == ".webp")
            {
                // WebP commence par RIFF, puis WEBP
                if (!buffer.Take(4).SequenceEqual(new byte[] { 0x52, 0x49, 0x46, 0x46 }))
                {
                    return ValidationResult.Failure("Le fichier n'est pas une image WebP valide.");
                }
                // Note: Pour WebP, on devrait vérifier plus loin, mais c'est suffisant pour la sécurité de base
            }
            else
            {
                // Pour JPEG et PNG, vérification complète
                if (!buffer.SequenceEqual(expectedMagicNumber))
                {
                    return ValidationResult.Failure($"Le fichier n'est pas une image {extension.ToUpperInvariant()} valide.");
                }
            }
        }
        catch
        {
            return ValidationResult.Failure("Erreur lors de la validation du fichier.");
        }

        return ValidationResult.Success();
    }
}


