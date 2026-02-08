using RateEngine.ApiService;
using RateEngine.ApiService.Endpoints;
using RateEngine.Infrastructure.Persistence;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Add Service Defaults
// This is the Aspire "glue" that sets up OpenTelemetry, Health Checks, and Service Discovery.
builder.AddServiceDefaults();

// 2. Add API Services
builder.Services.AddProblemDetails(); // Standard error handling
builder.Services.AddOpenApi();        // Swagger/OpenAPI documentation
builder.Services.AddScoped<RateEngine.Application.Common.Interfaces.IHotelRepository, RateEngine.Infrastructure.Repositories.HotelRepository>();
// 3. Register the Database Context
// This looks for a connection string named "rateenginedb" (which Aspire provides)
// and registers RateEngineDbContext so you can inject it into Controllers.
builder.AddSqlServerDbContext<RateEngineDbContext>("rateenginedb");
builder.AddRabbitMQClient("messaging");
builder.AddRedisDistributedCache("cache");

var app = builder.Build();
app.ApplyMigrations();
// 4. Configure the HTTP Request Pipeline
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    // Generates the JSON file at /openapi/v1.json
    app.MapOpenApi();

    // Generates the Visual UI at /scalar/v1
    app.MapScalarApiReference();
}

// Maps the health check endpoints defined in ServiceDefaults
app.MapDefaultEndpoints();
app.MapHotelEndpoints();

app.Run();