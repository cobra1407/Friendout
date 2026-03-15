namespace Friendout.Infrastructure.Enums;

/// <summary>
/// Représente les différentes catégories fonctionnelles de fichiers.
/// 
/// Cette énumération permet de définir le contexte métier d’un fichier
/// sans exposer les détails techniques de stockage (chemins, dossiers, etc.).
/// 
/// Chaque catégorie peut correspondre à des règles spécifiques :
/// - type de fichier autorisé
/// - taille maximale
/// - niveau de sécurité
/// - emplacement de stockage
/// </summary>
public enum FileCategory
{
    /// <summary>
    /// Image utilisée comme avatar pour un utilisateur.
    /// 
    /// Contraintes typiques :
    /// - Formats image uniquement (JPEG, PNG, etc.)
    /// - Taille limitée
    /// - Accès public ou semi-public
    /// </summary>
    UserAvatar,

    /// <summary>
    /// Image associée à une activité (illustration, photo, bannière, etc.).
    /// 
    /// Contraintes typiques :
    /// - Formats image
    /// - Taille modérée
    /// - Accès public
    /// </summary>
    ActivityImage,

    /// <summary>
    /// Fichier joint à une activité (document, image, archive, etc.).
    /// 
    /// Contraintes typiques :
    /// - Plusieurs types de fichiers autorisés
    /// - Accès restreint aux utilisateurs autorisés
    /// </summary>
    ActivityAttachment,

    /// <summary>
    /// Document générique (PDF, DOCX, etc.) stocké par l’application.
    /// 
    /// Contraintes typiques :
    /// - Accès restreint
    /// - Conservation longue durée
    /// </summary>
    Document
}