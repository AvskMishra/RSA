# 🛠 RiskApp – Local Development & Infrastructure Setup Guide

## Database Migrations

### Using Package Manager Console

```
Add-Migration InitialCreate -Project RiskApp.Infrastructure -StartupProject RiskApp.Api -OutputDir Persistence\Migrations

Update-Database -Project RiskApp.Infrastructure -StartupProject RiskApp.Api
```

## Serilog Logging

### Core Packages

```
Install-Package Serilog
Install-Package Serilog.Sinks.Console
Install-Package Serilog.Sinks.File
```

### Additional Enrichers

```
Install-Package Serilog.Enrichers.Environment
Install-Package Serilog.Enrichers.Process
Install-Package Serilog.Enrichers.Thread
Install-Package Serilog.Enrichers.ClientInfo
```

## Open Telemetry

### Required Packages

```
Install-Package OpenTelemetry.Extensions.Hosting
Install-Package OpenTelemetry.Instrumentation.AspNetCore
Install-Package OpenTelemetry.Instrumentation.Http
Install-Package OpenTelemetry.Instrumentation.Runtime
Install-Package OpenTelemetry.Exporter.Console
Install-Package OpenTelemetry.Exporter.Open
```

### Run Jaeger Locally

```
nerdctl run -d --name jaeger -p 16686:16686 -p 6831:6831/udp jaegertracing/all-in-one:latest
```

### Jaeger UI

> http://localhost:16686/search

## Authentication and Identity

### API Project

```
Install-Package Microsoft.AspNetCore.Authentication.JwtBearer

```

### Infrastructure Project

```
Install-Package Microsoft.AspNetCore.Identity.EntityFrameworkCore
Install-Package Microsoft.AspNetCore.Identity
Install-Package Microsoft.IdentityModel.Tokens
Install-Package System.IdentityModel.Tokens.Jwt

```

### Identity Seeding(Environment Variables)

```
dotnet user-secrets set "AuthSeeding:DefaultPassword" "S0mething-Str0ng!"
dotnet user-secrets set "AuthSeeding:Users:0:Password" "Reader#456"
dotnet user-secrets set "AuthSeeding:Users:1:Password" "Writer#789"
dotnet user-secrets set "AuthSeeding:Enabled" "true"
```

### Identity Migrations

#### Using .Net CLI

```
dotnet ef migrations add AddIdentity --project ./src/RiskApp.Infrastructure --startup-project ./src/RiskApp.Api
dotnet ef database update --project ./src/RiskApp.Infrastructure --startup-project ./src/RiskApp.Api
```

#### Using Package Manager Console

```
Add-Migration AddIdentity -Context RiskAppDbContext -Project RiskApp.Infrastructure -StartupProject RiskApp.Api -OutputDir Persistence\Migrations
Update-Database -Context RiskAppDbContext -Project RiskApp.Infrastructure -StartupProject RiskApp.Api
```

## MMessaging (Service Bus/MassTransit)

### RiskApp.Api

```
Install-Package MassTransit
Install-Package MassTransit.AspNetCore
Install-Package MassTransit.RabbitMQ
Install-Package MassTransit.Azure.ServiceBus.Core
```

### RiskApp.Infrastructure

```
Install-Package MassTransit
Install-Package MassTransit.RabbitMQ
Install-Package MassTransit.Azure.ServiceBus.Core
```