namespace Nutrihabitocc.Domain.Entities;

public class Auditory
{
    public bool IsActive { get; set; } = true;
    public string CreatedBy { get; set; } = "System";
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? DeletedBy { get; set; }
    public DateTime? DeletedOn { get; set; }
}