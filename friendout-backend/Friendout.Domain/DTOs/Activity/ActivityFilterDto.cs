using Friendout.Domain.Enums.FilterEnums;

namespace Friendout.Domain.DTOs.Activity;

public class ActivityFilterDto
{
    public ActivityTimeFilter TimeFilter { get; set; } = ActivityTimeFilter.All;

    // Permet de filtrer seulement les activités de l'utilisateur
    public bool OnlyOwnActivity { get; set; } = false;

    // Recherche par mot-clé dans le titre ou la description
    public string? Search { get; set; }

    // Scroll infini
    public int Skip { get; set; } = 0;
    public int Take { get; set; } = 20;
}