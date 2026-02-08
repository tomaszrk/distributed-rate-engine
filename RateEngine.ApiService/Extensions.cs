using Microsoft.EntityFrameworkCore;
using RateEngine.Infrastructure.Persistence;

namespace RateEngine.ApiService
{
    public static class Extensions
    {
        public static void ApplyMigrations(this WebApplication app)
        {
            // Create a scope to retrieve services
            using var scope = app.Services.CreateScope();

            // Get the DbContext
            var db = scope.ServiceProvider.GetRequiredService<RateEngineDbContext>();

            // If the database doesn't exist, create it. 
            // If tables are missing, create them.
            db.Database.Migrate();
        }
    }
}
