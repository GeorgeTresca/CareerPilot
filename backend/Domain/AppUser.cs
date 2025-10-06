using Microsoft.AspNetCore.Identity;

namespace backend.Domain
{
    
public class AppUser : IdentityUser<Guid>
{
    public string FullName { get; set; } = "";
    public string? Location { get; set; }
    public string? AvatarUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
}
