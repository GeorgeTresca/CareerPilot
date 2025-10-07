using backend.Application.Matching;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/projects/{projectId:guid}/candidates")]
[Authorize] 
public class MatchingController : ControllerBase
{
    private readonly MatchService _svc;
    public MatchingController(MatchService svc) => _svc = svc;

    [HttpGet]
    public async Task<IActionResult> Get(Guid projectId)
    {
        var ranked = await _svc.RankCandidates(projectId);
        return Ok(ranked);
    }
}
