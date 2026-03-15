using Friendout.Domain.Enums;

namespace Friendout.Domain.DTOs.Localisation;

public class LocalisationDto
{
    public LocalisationType Type { get; set; }
    
    public string? Address { get; set; }
    
    public string? MapLink { get; set; }
    
    public string? VirtualUrl { get; set; }
    
    public string? DisplayName { get; set; }
}