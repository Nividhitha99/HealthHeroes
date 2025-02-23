using Microsoft.EntityFrameworkCore;

namespace Health_Heroes.Data
{
    public static class Dependencies
    {
        public static void ConfigureServices(IConfiguration configuration, IServiceCollection services)
        {
            // Use only one DbContext for the Health_Hero database
            services.AddDbContext<HealthHeroDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
        }
    }
}
