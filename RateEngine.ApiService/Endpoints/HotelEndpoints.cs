using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using RabbitMQ.Client;
using RateEngine.Application.Common.Interfaces;
using RateEngine.Domain.Entities;
using RateEngine.Domain.Events;
using System.Text;
using System.Text.Json;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace RateEngine.ApiService.Endpoints;

public static class HotelEndpoints
{
    public static void MapHotelEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/hotels");

        // GET /hotels/{id}
        group.MapGet("/{id}", async (Guid id, IHotelRepository repo, IDistributedCache cache) =>
        {
            string cacheKey = $"hotel:{id}";
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var cachedJson = await cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrWhiteSpace(cachedJson))
            {
                var cachedHotel = JsonSerializer.Deserialize<Hotel>(cachedJson, jsonOptions);
                return Results.Ok(cachedHotel);
            }


            var hotel = await repo.GetByIdAsync(id);

            if (hotel is not null)
            {
                var json = JsonSerializer.Serialize(hotel, jsonOptions);

                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(60)
                };
                await cache.SetStringAsync(cacheKey, json, options);

                return Results.Ok(hotel);
            }
            return Results.NotFound();
        })
        .WithName("GetHotel");

        // POST /hotels
        group.MapPost("/", async ([FromBody] CreateHotelRequest request, IConnection connection) =>
        {
            // Create Domain Entity using the Factory Method
            var address = new Address(request.Street, request.ZipCode, request.Country);
            var hotel = Hotel.Create(request.Name, request.City, request.Stars, address);

            //Prepare the Message
            var eventMessage = new HotelCreatedEvent(hotel.Id, hotel.Name, hotel.City, hotel.Stars);
            var json = JsonSerializer.Serialize(eventMessage);
            var body = Encoding.UTF8.GetBytes(json);

            // Publish to RabbitMQ
            // We create a "channel" (lightweight connection)
            using var channel = await connection.CreateChannelAsync();

            // Declare the queue to make sure it exists
            await channel.QueueDeclareAsync(queue: "hotel-updates", durable: false, exclusive: false, autoDelete: false, arguments: null);


            // Send the message
            await channel.BasicPublishAsync(exchange: string.Empty, routingKey: "hotel-updates", body: body);

            // Return "Accepted" (202)
            // This tells the client: "We got it, but it's not in the DB yet."
            return Results.Accepted($"/hotels/{hotel.Id}", hotel);
        });
    }

    // Simple DTO record for the incoming JSON
    public record CreateHotelRequest(string Name, string City, int Stars, string Street, string ZipCode, string Country);
}

