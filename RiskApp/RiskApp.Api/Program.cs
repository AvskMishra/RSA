using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.OpenApi.Models;
using RiskApp.Api.Middleware;
using RiskApp.Api.Observability;
using RiskApp.Application.Profiles;
using RiskApp.Application.Risk;
using RiskApp.Infrastructure;
using RiskApp.Infrastructure.Auth;
using RiskApp.Infrastructure.Persistence;
using Serilog;

var builder = WebApplication.CreateBuilder(args);


// Configure Serilog using our extension (external JSON)
builder.AddSerilog();

// enable OTel from appsetting.json
//builder.AddOpenTelemetry();

// Infrastructure (SQLite + services)
var conn = builder.Configuration.GetConnectionString("Default")!;
builder.Services.AddInfrastructure(conn);

//Adding FluentValidation (auto-validation + discover validators from Application assembly)
builder.Services
    .AddFluentValidationAutoValidation()
    .AddFluentValidationClientsideAdapters();

builder.Services.AddValidatorsFromAssemblyContaining<RiskApp.Application.Validation.ProfileCreateValidator>();


builder.Services.AddControllers().ConfigureApiBehaviorOptions(options =>
{
    // Keep default [ApiController] behavior (automatic 400) but you can override the response here if you want
    //options.InvalidModelStateResponseFactory = context =>
    //{
    //    var problem = new ValidationProblemDetails(context.ModelState)
    //    {
    //        Status = StatusCodes.Status400BadRequest,
    //        Title = "One or more validation errors occurred."
    //    };
    //    return new BadRequestObjectResult(problem);
    //};
});


// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.Services.AddOpenApi();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "RiskApp API", Version = "v1" });

    // JWT bearer
    var jwtScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: Bearer {your JWT token}"
    };
    c.AddSecurityDefinition("Bearer", jwtScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        [jwtScheme] = Array.Empty<string>()
    });

    // Include XML docs from Api + Application + Domain
    var xmlFiles = new[] { "RiskApp.Api.xml", "RiskApp.Application.xml", "RiskApp.Domain.xml" };
    foreach (var xml in xmlFiles)
    {
        var path = Path.Combine(AppContext.BaseDirectory, xml);
        if (File.Exists(path))
            c.IncludeXmlComments(path, includeControllerXmlComments: true);
    }
});


// Health checks
builder.Services.AddHealthChecks();

// Custom middleware
builder.Services.AddTransient<ErrorHandlingMiddleware>();
builder.Services.AddTransient<CorrelationIdMiddleware>();


builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

var app = builder.Build();


// 🔹 4️⃣ Enable structured Serilog request logging
app.UseSerilogRequestLogging(opts =>
{
    opts.EnrichDiagnosticContext = (diag, http) =>
    {
        diag.Set("RequestHost", http.Request.Host.Value);
        diag.Set("RemoteIpAddress", http.Connection.RemoteIpAddress?.ToString());
        diag.Set("UserAgent", http.Request.Headers.UserAgent.ToString());
    };
});

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ErrorHandlingMiddleware>();

// Health endpoints
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready");

app.UseSwagger();
app.UseSwaggerUI();


// SEED on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<RiskAppDbContext>();
    await SeedData.EnsureSeededAsync(db); // <-- db is passed here

    var riskSvc = scope.ServiceProvider.GetRequiredService<IRiskAssessmentService>();
    await SeedData.EnsureRiskAssessmentsSeededAsync(db, riskSvc);

    // for Identity seeding
    await IdentitySeed.EnsureSeededAsync(scope.ServiceProvider);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
