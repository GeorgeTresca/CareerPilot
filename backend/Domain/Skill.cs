namespace backend.Domain
{
    public class Skill
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Name { get; set; } = "";
        public int Level { get; set; } // 1..5
        public int Years { get; set; }
        public string? TagsJson { get; set; }
        public AppUser? User { get; set; }
    }
}
