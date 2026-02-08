using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RateEngine.Domain.Entities;
using RateEngine.Domain.Events;
using RateEngine.Infrastructure.Persistence;
using System.Text;
using System.Text.Json;
using Polly;
using Polly.Registry;

namespace RateEngine.Worker;

public class Worker : BackgroundService
{
    private readonly IConnection _connection;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<Worker> _logger;
    private IChannel? _channel;
    private readonly ResiliencePipeline _pipeline;

    public Worker(IConnection connection, IServiceProvider serviceProvider, ILogger<Worker> logger, ResiliencePipelineProvider<string> pipelineProvider)
    {
        _connection = connection;
        _serviceProvider = serviceProvider;
        _logger = logger;
        _pipeline = pipelineProvider.GetPipeline("db-retry");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // 1. Setup the Channel
        _channel = await _connection.CreateChannelAsync();

        // Always declare queue again (idempotency) - ensures it exists
        await _channel.QueueDeclareAsync(queue: "hotel-updates", durable: false, exclusive: false, autoDelete: false, arguments: null);

        // 2. Define the Consumer
        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            _logger.LogInformation("Received Message: {Message}", message);

            try
            {
                var eventData = JsonSerializer.Deserialize<HotelCreatedEvent>(message);

                if (eventData is not null)
                {
                    await _pipeline.ExecuteAsync(async token =>
                    {
                        using var scope = _serviceProvider.CreateScope();
                        var dbContext = scope.ServiceProvider.GetRequiredService<RateEngineDbContext>();

                        // Reconstruct the entity (Simplified for demo)
                        var address = new Address("Unknown", "00000", "Unknown");
                        var hotel = Hotel.CreateWithId(
                            eventData.Id,
                            eventData.Name,
                            eventData.City,
                            eventData.Stars,
                            address
                        );

                        if (await dbContext.Hotels.FindAsync([hotel.Id], token) == null)
                        {
                            dbContext.Hotels.Add(hotel);
                            await dbContext.SaveChangesAsync(token);
                            _logger.LogInformation("Saved Hotel to SQL (Resiliently!)");
                        }
                        else
                        {
                            _logger.LogInformation("Hotel already exists. Skipping.");
                        }
                    });
                    
                }

                await _channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message");
                // Negative Acknowledge (Retries or sends to Dead Letter Queue)
                await _channel.BasicNackAsync(deliveryTag: ea.DeliveryTag, multiple: false, requeue: false);
            }
        };

        // 5. Start Consuming
        await _channel.BasicConsumeAsync(queue: "hotel-updates", autoAck: false, consumer: consumer);

        // Keep the task alive
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}
