using RateEngine.Domain.Entities;


namespace RateEngine.Application.Common.Interfaces
{
    public interface IHotelRepository
    {
        Task<Hotel?> GetByIdAsync(Guid id);
        Task AddAsync(Hotel hotel);
        Task SaveChangesAsync();
    }
}
