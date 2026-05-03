using InovaGAB.API.Models;
using Microsoft.EntityFrameworkCore;

namespace InovaGAB.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Idea> Ideas => Set<Idea>();
        public DbSet<Project> Projects => Set<Project>();
        public DbSet<StrategicGuideline> StrategicGuidelines => Set<StrategicGuideline>();
        public DbSet<Challenge> Challenges => Set<Challenge>();

    }
}
