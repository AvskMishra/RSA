using Microsoft.OpenApi.Models;
using RiskApp.Application.Profiles;
using RiskApp.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Infrastructure (SQLite + services)
var conn = builder.Configuration.GetConnectionString("Default")!;
builder.Services.AddInfrastructure(conn);


// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.Services.AddOpenApi();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "RiskApp API", Version = "v1" });

    // Include XML docs from Api + Application + Domain
    var xmlFiles = new[] { "RiskApp.Api.xml", "RiskApp.Application.xml", "RiskApp.Domain.xml" };
    foreach (var xml in xmlFiles)
    {
        var path = Path.Combine(AppContext.BaseDirectory, xml);
        if (File.Exists(path))
            c.IncludeXmlComments(path, includeControllerXmlComments: true);
    }
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
