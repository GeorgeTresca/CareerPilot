using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using backend.Domain;
using backend.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly AppDbContext _db;
    public ProjectsController(AppDbContext db) { _db = db; }

    private Guid? TryGetUserId()
    {
        var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        return Guid.TryParse(idStr, out var gid) ? gid : null;
    }

    public record ProjectReq(string Title, string? Description, object[] RequiredSkills);

    [HttpGet]
    [AllowAnonymous] 
    public async Task<IActionResult> List([FromQuery] bool? active, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        page = page < 1 ? 1 : page;
        pageSize = (pageSize < 1 || pageSize > 100) ? 20 : pageSize;

        var q = _db.Projects.AsNoTracking().AsQueryable();
        if (active.HasValue) q = q.Where(p => p.IsActive == active.Value);

        var total = await q.CountAsync();
        var items = await q.OrderByDescending(p => p.CreatedAt)
                           .Skip((page - 1) * pageSize)
                           .Take(pageSize)
                           .ToListAsync();

        return Ok(new { total, page, pageSize, items });
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> Get(Guid id)
    {
        var p = await _db.Projects.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        return p is null ? NotFound() : Ok(p);
    }

    // Create/update/delete require authentication 
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(ProjectReq req)
    {
        var uid = TryGetUserId();
        if (uid is null) return Unauthorized();

        var p = new Project
        {
            Id = Guid.NewGuid(),
            ManagerId = uid.Value,
            Title = req.Title.Trim(),
            Description = req.Description,
            RequiredSkillsJson = JsonSerializer.Serialize(req.RequiredSkills),
            IsActive = true
        };
        _db.Projects.Add(p);
        await _db.SaveChangesAsync();
        return Ok(p);
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, ProjectReq req)
    {
        var uid = TryGetUserId();
        if (uid is null) return Unauthorized();

        var p = await _db.Projects.FirstOrDefaultAsync(x => x.Id == id && x.ManagerId == uid.Value);
        if (p == null) return NotFound();

        p.Title = req.Title.Trim();
        p.Description = req.Description;
        p.RequiredSkillsJson = JsonSerializer.Serialize(req.RequiredSkills);
        await _db.SaveChangesAsync();

        return Ok(p);
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id)
    {
        var uid = TryGetUserId();
        if (uid is null) return Unauthorized();

        var p = await _db.Projects.FirstOrDefaultAsync(x => x.Id == id && x.ManagerId == uid.Value);
        if (p == null) return NotFound();

        _db.Projects.Remove(p);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
