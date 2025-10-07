namespace backend.Domain;

public class Assignment
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Guid UserId { get; set; }
    public string Status { get; set; } = "Proposed";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
