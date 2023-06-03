namespace Application.DTOs;

public class KatoResulDto
{
    public string? Code { get; set; }
    public string? Locality { get; set; }
    public string? District { get; set; }
    public string? Region { get; set; }
    public bool? IsLocality { get; set; }
    public bool? IsDeleted { get; set; }
    public LevelDto? HighLevel { get; set; }
    public List<LevelDto>? LowLevel { get; set; }
}