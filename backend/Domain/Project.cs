namespace backend.Domain;

public class Project
{
    public Guid Id { get; set; }
    public Guid ManagerId { get; set; }       // user who created it
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public string RequiredSkillsJson { get; set; } = "[]";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
