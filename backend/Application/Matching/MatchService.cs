using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using backend.Infrastructure;

namespace backend.Application.Matching;

public class MatchService
{
    private readonly AppDbContext _db;
    public MatchService(AppDbContext db) => _db = db;

    private record ReqSkill(string name, int minLevel);

    public async Task<IReadOnlyList<object>> RankCandidates(Guid projectId)
    {
        var project = await _db.Projects.AsNoTracking()
            .FirstAsync(p => p.Id == projectId && p.IsActive);

        var req = JsonSerializer.Deserialize<List<ReqSkill>>(project.RequiredSkillsJson)
                  ?? new List<ReqSkill>();

        
        var userSkills = await _db.Skills.AsNoTracking().ToListAsync();
        var byUser = userSkills.GroupBy(s => s.UserId)
                               .ToDictionary(g => g.Key, g => g.ToList());

        var users = await _db.Users.AsNoTracking().ToListAsync();

        var ranked = users.Select(u =>
        {
            var skillList = byUser.GetValueOrDefault(u.Id) ?? new List<backend.Domain.Skill>();
            double score = 0;

            foreach (var r in req)
            {
                var have = skillList.FirstOrDefault(s => s.Name.Equals(r.name, StringComparison.OrdinalIgnoreCase));
                if (have != null)
                {
                    
                    var lvl = Math.Clamp((have.Level - r.minLevel + 5) / 5.0, 0, 1);
                    score += lvl * 10;
                    
                    score += Math.Min(have.Years, 5);
                }
            }

            return new
            {
                userId = u.Id,
                u.FullName,
                u.Email,
                score
            };
        })
        .Where(x => x.score > 0)
        .OrderByDescending(x => x.score)
        .Take(50)
        .ToList();

        return ranked;
    }
}
