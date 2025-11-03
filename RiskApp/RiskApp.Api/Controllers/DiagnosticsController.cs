using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Diagnostics;

namespace RiskApp.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DiagnosticsController : ControllerBase
    {
        private static readonly ActivitySource ActivitySource = new("RiskApp");

        [HttpGet("ping")]
        public IActionResult Ping()
        {
            using var act = ActivitySource.StartActivity("Diagnostics.Ping");
            var traceId = Activity.Current?.TraceId.ToString() ?? HttpContext.TraceIdentifier;

            Log.Information("Diagnostics ping received. TraceId={TraceId}", traceId);
            return Ok(new { ok = true, traceId });
        }
    }
}
