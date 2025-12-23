using Microsoft.AspNetCore.Mvc;
using RiskApp.Application.Work;

namespace RiskApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmploymentRecordsController : ControllerBase
{
    private readonly IEmploymentService _service;
    public EmploymentRecordsController(IEmploymentService service) => _service = service;

    /// <summary>Create an employment record for a profile.</summary>
    /// <response code="201">Created</response>
    /// <response code="400">Validation error</response>
    [HttpPost]
    [ProducesResponseType(typeof(EmploymentReadDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] EmploymentCreateDto dto, CancellationToken ct)
    {
        var created = await _service.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Get a single employment record by Id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(EmploymentReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct)
    {
        var item = await _service.GetAsync(id, ct);
        return item is null ? NotFound() : Ok(item);
    }

    /// <summary>List employment records for a profile.</summary>
    [HttpGet("by-profile/{profileId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<EmploymentReadDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListByProfile([FromRoute] Guid profileId, [FromQuery] int skip = 0, [FromQuery] int take = 50, CancellationToken ct = default)
    {
        var items = await _service.ListByProfileAsync(profileId, skip, take, ct);
        return Ok(items);
    }

    /// <summary>Update employer/type/income.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] EmploymentUpdateDto dto, CancellationToken ct)
    {
        var ok = await _service.UpdateAsync(id, dto, ct);
        return ok ? NoContent() : NotFound();
    }

    /// <summary>Close employment (set end date and mark not current).</summary>
    [HttpPost("{id:guid}/close")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Close([FromRoute] Guid id, [FromBody] EmploymentCloseDto dto, CancellationToken ct)
    {
        var ok = await _service.CloseAsync(id, dto, ct);
        return ok ? NoContent() : NotFound();
    }

    /// <summary>Delete an employment record.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken ct)
    {
        var ok = await _service.DeleteAsync(id, ct);
        return ok ? NoContent() : NotFound();
    }
}
