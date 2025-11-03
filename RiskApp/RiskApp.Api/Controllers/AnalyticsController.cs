using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RiskApp.Application.Analytics;

namespace RiskApp.Api.Controllers;


[Authorize(Policy = "CanRead")]
[ApiController]
[Route("api/[controller]")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _svc;
    public AnalyticsController(IAnalyticsService svc)
    {
        _svc = svc;
    }

    /// <summary>Counts of decisions (Approve/Review/Decline). Optional filter by profileId.</summary>
    [HttpGet("decisions")]
    [ProducesResponseType(typeof(IReadOnlyList<DecisionBucketDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> DecisionDistribution([FromQuery] Guid? profileId, CancellationToken ct)
    {
        return Ok(await _svc.GetDecisionDistributionAsync(profileId, ct));
    }

    /// <summary>Average score overall and per decision. Optional filter by profileId.</summary>
    [HttpGet("average-score")]
    [ProducesResponseType(typeof(AverageScoreDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> AverageScore([FromQuery] Guid? profileId, CancellationToken ct)
        => Ok(await _svc.GetAverageScoreAsync(profileId, ct));



    /// <summary>Monthly trend (count & avg score). Params: monthsBack=1..60, optional profileId.</summary>
    [HttpGet("trend/monthly")]
    [ProducesResponseType(typeof(IReadOnlyList<MonthlyTrendDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> MonthlyTrend([FromQuery] Guid? profileId, [FromQuery] int monthsBack = 12, CancellationToken ct = default)
        => Ok(await _svc.GetMonthlyTrendAsync(profileId, monthsBack, ct));

    /// <summary>
    /// Combined analytics: decision distribution, average score, monthly trend.
    /// Optional filters: profileId (GUID), monthsBack (1..60, default 12).
    /// </summary>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(AnalyticsSummaryDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Summary([FromQuery] Guid? profileId, [FromQuery] int monthsBack = 12, CancellationToken ct = default)
        => Ok(await _svc.GetSummaryAsync(profileId, monthsBack, ct));
}
