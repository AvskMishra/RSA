using Microsoft.AspNetCore.Mvc;
using RiskApp.Application.Risk;

namespace RiskApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RiskAssessmentsController : ControllerBase
{
    private readonly IRiskAssessmentService _service;
    public RiskAssessmentsController(IRiskAssessmentService service) => _service = service;

    /// <summary>Run the scorecard for a profile and create a new assessment.</summary>
    /// <response code="201">Assessment created</response>
    /// <response code="400">Validation error</response>
    [HttpPost("assess")]
    [ProducesResponseType(typeof(RiskAssessmentReadDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Assess([FromBody] RiskAssessRequestDto request, CancellationToken ct)
    {
        var created = await _service.AssessAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Get a risk assessment by Id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(RiskAssessmentReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct)
    {
        var item = await _service.GetAsync(id, ct);
        return item is null ? NotFound() : Ok(item);
    }

    /// <summary>List assessments for a profile (newest first).</summary>
    [HttpGet("by-profile/{profileId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<RiskAssessmentReadDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListByProfile([FromRoute] Guid profileId, [FromQuery] int skip = 0, [FromQuery] int take = 50, CancellationToken ct = default)
    {
        var items = await _service.ListByProfileAsync(profileId, skip, take, ct);
        return Ok(items);
    }
}
