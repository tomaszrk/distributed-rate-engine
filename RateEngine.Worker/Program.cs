using Polly;
using Polly.Retry;
using RateEngine.Infrastructure.Persistence;
using RateEngine.Worker;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

builder.Services.AddResiliencePipeline("db-retry", pipelineBuilder =>
{
    pipelineBuilder.AddRetry(new RetryStrategyOptions
    {
        MaxRetryAttempts = 3,
        BackoffType = DelayBackoffType.Exponential,
        Delay = TimeSpan.FromSeconds(1),
        ShouldHandle = new PredicateBuilder().Handle<Exception>()
    });
});

builder.AddServiceDefaults();
builder.Services.AddScoped<RateEngine.Application.Common.Interfaces.IHotelRepository, RateEngine.Infrastructure.Repositories.HotelRepository>();
builder.AddSqlServerDbContext<RateEngineDbContext>("rateenginedb");
builder.AddRabbitMQClient("messaging");

var host = builder.Build();
host.Run(); 
