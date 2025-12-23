using Microsoft.Extensions.Configuration;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.AspNetCore;
using Serilog.Settings.Configuration;
using System.Diagnostics;
using OpenTelemetry.Instrumentation.Runtime;

namespace RiskApp.Api.Observability;

public static class ObservabilityExtensions
{
    /// <summary>
    /// Configure Serilog from external "serilog.config.json".
    /// </summary>
    public static void AddSerilog(this WebApplicationBuilder builder)
    {
        #region Serilog implementation:

        //Load the external configuration file
        builder.Configuration.AddJsonFile("serilog.config.json", optional: false, reloadOnChange: true);

        //Build Serilog logger from configuration
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .CreateLogger();

        //Replace the default logger
        builder.Host.UseSerilog();
        Log.Information("Serilog configured from serilog.config.json");

        #endregion

    }


    /// <summary>
    /// Adds OpenTelemetry tracing & metrics based on appsettings "OpenTelemetry" section.
    /// Keeps Serilog as-is.
    /// </summary>
    public static void AddOpenTelemetry(this WebApplicationBuilder builder)
    {
        var otel = builder.Configuration.GetSection("OpenTelemetry");
        if (!otel.GetValue<bool>("Enable"))
        {
            Serilog.Log.Information("OpenTelemetry disabled via configuration.");
            return;
        }
        // Stable W3C Ids
        Activity.DefaultIdFormat = ActivityIdFormat.W3C;
        Activity.ForceDefaultIdFormat = true;

        builder.Services.AddOpenTelemetry().ConfigureResource(rb => rb.AddService("RiskApp.Api"))
               .WithTracing(t =>
               {
                   t.AddAspNetCoreInstrumentation(o =>
                   {
                       o.RecordException = true;
                       o.Filter = ctx => ctx.Request.Path.StartsWithSegments("/api");
                   });
                   t.AddHttpClientInstrumentation();

                   if (otel.GetSection("Exporters").GetValue<bool>("Console"))
                       t.AddConsoleExporter();

                   if (otel.GetSection("Exporters").GetValue<bool>("Otlp"))
                   {
                       var ep = otel.GetSection("Exporters").GetValue<string>("OtlpEndpoint");
                       t.AddOtlpExporter(o => o.Endpoint = new Uri(ep));
                   }
               })
               .WithMetrics(m =>
               {
                   m.AddAspNetCoreInstrumentation();
                   m.AddRuntimeInstrumentation();

                   if (otel.GetSection("Exporters").GetValue<bool>("Console"))
                       m.AddConsoleExporter();

                   if (otel.GetSection("Exporters").GetValue<bool>("Otlp"))
                   {
                       var ep = otel.GetSection("Exporters").GetValue<string>("OtlpEndpoint");
                       m.AddOtlpExporter(o => o.Endpoint = new Uri(ep));
                   }
               });

        Serilog.Log.Information("OpenTelemetry configured (Enable=true).");
    }

}
