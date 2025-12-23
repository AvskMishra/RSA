using Microsoft.AspNetCore.Mvc;
using RiskApp.Application.Profiles;

namespace RiskApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProfilesController : ControllerBase
{
    private readonly IProfileService _service;

    public ProfilesController(IProfileService service)
    {
        _service = service;
    }

    /// <summary>Create a new profile.</summary>
    /// <remarks>Registers a customer before running risk assessment. NationalId must be unique.</remarks>
    /// <param name="dto">Profile data.</param>
    /// <response code="201">Profile created successfully.</response>
    /// <response code="400">Validation error.</response>

    [HttpPost]
    [ProducesResponseType(typeof(ProfileReadDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] ProfileCreateDto dto, CancellationToken ct)
    {
        var created = await _service.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Get a profile by Id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProfileReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct)
    {
        var item = await _service.GetAsync(id, ct);
        return item is null ? NotFound() : Ok(item);
    }

    /// <summary>List profiles (paged).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ProfileReadDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] int skip = 0, [FromQuery] int take = 50, CancellationToken ct = default)
    {
        var items = await _service.ListAsync(skip, take, ct);
        return Ok(items);
    }

    /// <summary>Update contact details of a profile.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] ProfileUpdateDto dto, CancellationToken ct)
    {
        var ok = await _service.UpdateAsync(id, dto, ct);
        return ok ? NoContent() : NotFound();
    }

    /// <summary>Delete a profile.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken ct)
    {
        var ok = await _service.DeleteAsync(id, ct);
        return ok ? NoContent() : NotFound();
    }
}
