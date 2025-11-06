using MassTransit;
using Microsoft.AspNetCore.Mvc;
using RiskApp.Application.Messaging;

namespace RiskApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MessagingTestController : ControllerBase
{
    private readonly IRequestClient<PerformExternalChecks> _client;

    public MessagingTestController(IRequestClient<PerformExternalChecks> client)
    {
        _client = client;
    }

    [HttpPost("run")]
    public async Task<IActionResult> Run([FromBody] Guid profileId, CancellationToken ct)
    {
        var correlationId = Guid.NewGuid();

        var response = await _client.GetResponse<ExternalChecksCompleted>(new
        {
            CorrelationId = correlationId,
            ProfileId = profileId,
            NationalId = "PANX1234Z",
            Email = "probe@example.com",
            Phone = "+919876543210"
        }, ct);

        return Ok(new
        {
            Message = "External checks succeeded!",
            response.Message.CreditScore,
            response.Message.FraudRiskLevel,
            response.Message.IsHighRiskIdentity
        });
    }
}
