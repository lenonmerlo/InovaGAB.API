using InovaGAB.API.Models;
using Microsoft.EntityFrameworkCore;

namespace InovaGAB.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> User => Set<User>();
        public DbSet<Idea> Idea => Set<Idea>();
        public DbSet<Project> Project => Set<Project>();
        public DbSet<StrategicGuideline> StrategicGuideline => Set<StrategicGuideline>();
        public DbSet<Challenge> Challenge => Set<Challenge>();

    }
}
