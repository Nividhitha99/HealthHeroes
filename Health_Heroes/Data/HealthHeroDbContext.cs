using Microsoft.EntityFrameworkCore;
using Health_Heroes.Models;  // Replace with your actual namespace

namespace Health_Heroes.Data
{
    public class HealthHeroDbContext : DbContext
    {
        public HealthHeroDbContext(DbContextOptions<HealthHeroDbContext> options)
            : base(options)
        {
        }

        public DbSet<Donor_Details> Donor_Details { get; set; }  // Your Donor_Details table
        public DbSet<User> User { get; set; }                  // Your User table

       
    }
}

