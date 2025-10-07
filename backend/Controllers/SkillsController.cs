using backend.Domain;
using backend.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace backend.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SkillsController : ControllerBase
{
    private readonly AppDbContext _db;
    public SkillsController(AppDbContext db) { _db = db; }

    private Guid? TryGetUserId()
    {
        var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        return Guid.TryParse(idStr, out var gid) ? gid : null;
    }

    // A DTO that returns only what the client needs
    public record SkillDto(Guid Id, string Name, int Level, int Years, string? TagsJson);
    public record SkillReq(string Name, int Level, int Years, string? TagsJson);

    [HttpGet("me")]
    public async Task<IActionResult> MySkills()
    {
        var uid = TryGetUserId();
        if (uid is null) return Unauthorized();

        var skills = await _db.Skills
            .Where(s => s.UserId == uid.Value)
            .Select(s => new SkillDto(s.Id, s.Name, s.Level, s.Years, s.TagsJson))
            .ToListAsync();

        return Ok(skills);
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] SkillReq req)
    {
        var uid = TryGetUserId();
        if (uid is null) return Unauthorized();

        var s = new Skill
        {
            Id = Guid.NewGuid(),
            UserId = uid.Value,
            Name = req.Name,
            Level = req.Level,
            Years = req.Years,
            TagsJson = req.TagsJson
        };

        _db.Skills.Add(s);
        await _db.SaveChangesAsync();

        return Ok(new SkillDto(s.Id, s.Name, s.Level, s.Years, s.TagsJson));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] SkillReq req)
    {
        var uid = TryGetUserId();
        if (uid is null) return Unauthorized();

        var s = await _db.Skills.FirstOrDefaultAsync(x => x.Id == id && x.UserId == uid.Value);
        if (s == null) return NotFound();

        s.Name = req.Name;
        s.Level = req.Level;
        s.Years = req.Years;
        s.TagsJson = req.TagsJson;

        await _db.SaveChangesAsync();
        return Ok(new SkillDto(s.Id, s.Name, s.Level, s.Years, s.TagsJson));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var uid = TryGetUserId();
        if (uid is null) return Unauthorized();

        var s = await _db.Skills.FirstOrDefaultAsync(x => x.Id == id && x.UserId == uid.Value);
        if (s == null) return NotFound();

        _db.Skills.Remove(s);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
