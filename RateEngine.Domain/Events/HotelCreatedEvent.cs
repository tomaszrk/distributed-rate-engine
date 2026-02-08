namespace RateEngine.Domain.Events;
public record HotelCreatedEvent(Guid Id, string Name, string City, int Stars);