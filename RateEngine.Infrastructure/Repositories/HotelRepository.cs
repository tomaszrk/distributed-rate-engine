using Microsoft.EntityFrameworkCore;
using RateEngine.Application.Common.Interfaces;
using RateEngine.Domain.Entities;
using RateEngine.Infrastructure.Persistence;

namespace RateEngine.Infrastructure.Repositories
{
    public class HotelRepository : IHotelRepository
    {
        private readonly RateEngineDbContext _context;

        public HotelRepository(RateEngineDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Hotel hotel)
        {
            await _context.Hotels.AddAsync(hotel);
        }

        public async Task<Hotel?> GetByIdAsync(Guid id)
        {
            return await _context.Hotels
            .Include(h => h.Address) // Eager Load the Value Object
            .FirstOrDefaultAsync(h => h.Id == id);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
