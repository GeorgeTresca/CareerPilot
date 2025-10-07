using backend.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<AppUser> _users;
    private readonly IConfiguration _cfg;

    public AuthController(UserManager<AppUser> users, IConfiguration cfg)
    {
        _users = users;
        _cfg = cfg;
    }

    public record RegisterReq(string Email, string Password, string FullName);
    public record LoginReq(string Email, string Password);
    public record MeDto(Guid Id, string Email, string FullName, string? Location, string? AvatarUrl);

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterReq req)
    {
        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            Email = req.Email,
            UserName = req.Email,
            FullName = req.FullName
        };

        var result = await _users.CreateAsync(user, req.Password);
        if (!result.Succeeded) return BadRequest(result.Errors);
        return Ok();
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginReq req)
    {
        var user = await _users.FindByEmailAsync(req.Email);
        if (user == null || !await _users.CheckPasswordAsync(user, req.Password))
            return Unauthorized();

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_cfg["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!)
        };

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds
        );

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);
        return Ok(new { token = jwt });
    }

    
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<MeDto>> Me()
    {
        var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (sub == null) return Unauthorized();
        var user = await _users.FindByIdAsync(sub);
        if (user == null) return Unauthorized();
        return new MeDto(user.Id, user.Email!, user.FullName, user.Location, user.AvatarUrl);
    }
}
