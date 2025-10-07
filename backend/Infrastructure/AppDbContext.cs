namespace backend.Infrastructure
{
    using backend.Domain;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;

    public class AppDbContext : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Skill> Skills => Set<Skill>();
        public DbSet<Project> Projects => Set<Project>();
        public DbSet<Assignment> Assignments => Set<Assignment>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            base.OnModelCreating(b);

            b.Entity<Skill>(e =>
            {
                e.HasIndex(x => new { x.UserId, x.Name });
                e.Property(x => x.Name).HasMaxLength(100);
                e.Property(x => x.Level).HasDefaultValue(1);
            });

            b.Entity<Project>(e =>
            {
                e.HasIndex(x => new { x.ManagerId, x.IsActive, x.CreatedAt });
                e.Property(x => x.Title).HasMaxLength(150);
            });

            b.Entity<Assignment>(e =>
            {
                e.HasIndex(x => new { x.ProjectId, x.UserId }).IsUnique();
                e.Property(x => x.Status).HasMaxLength(20);
            });
        }
    }
}
