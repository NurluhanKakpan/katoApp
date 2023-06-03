namespace Domain.Models;

public class KatoInfo
{
    public Guid Id { get; set; }
    public bool? IsDeleted { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? CreatedUserId { get; set; }
    public string? UpdatedUserId { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
    public Guid? ParentId { get; set; }
    public string? ShortName { get; set; }
    public DateTime? BeginDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool? IsMain { get; set; }
    public bool? IsLocality { get; set; }
}
