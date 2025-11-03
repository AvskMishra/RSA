using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RiskApp.Application.Earnings;

namespace RiskApp.Api.Controllers;


[ApiController]
[Route("api/[controller]")]
public class EarningsController : ControllerBase
{
    private readonly IEarningService _service;
    public EarningsController(IEarningService service) => _service = service;

    /// <summary>Create an earning snapshot for a profile.</summary>
    /// <response code="201">Created</response>
    /// <response code="400">Validation error</response>
    
    [Authorize(Policy = "CanWrite")]
    [HttpPost]
    [ProducesResponseType(typeof(EarningReadDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] EarningCreateDto dto, CancellationToken ct)
    {
        var created = await _service.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }


    /// <summary>Get a single earning record by Id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(EarningReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct)
    {
        var item = await _service.GetAsync(id, ct);
        return item is null ? NotFound() : Ok(item);
    }

    /// <summary>List earnings for a profile (newest first).</summary>
    [Authorize(Policy = "CanRead")]
    [HttpGet("by-profile/{profileId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<EarningReadDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListByProfile([FromRoute] Guid profileId, [FromQuery] int skip = 0, [FromQuery] int take = 50, CancellationToken ct = default)
    {
        var items = await _service.ListByProfileAsync(profileId, skip, take, ct);
        return Ok(items);
    }

    /// <summary>Update an earning snapshot.</summary>
    [Authorize(Policy = "CanWrite")]
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] EarningUpdateDto dto, CancellationToken ct)
    {
        var ok = await _service.UpdateAsync(id, dto, ct);
        return ok ? NoContent() : NotFound();
    }

    /// <summary>Delete an earning snapshot.</summary>
    [Authorize(Policy = "CanWrite")]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken ct)
    {
        var ok = await _service.DeleteAsync(id, ct);
        return ok ? NoContent() : NotFound();
    }
}
