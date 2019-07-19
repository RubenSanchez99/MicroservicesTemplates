using CRUDService.Model;
using Microsoft.EntityFrameworkCore;
using CRUDService.Infrastructure.EntityConfigurations;

namespace CRUDService.Infrastructure
{
    // dotnet ef migrations add XXXX -o Infrastructure/Migrations -c RepositoryContext
    // dotnet ef database update
    internal class RepositoryContext : DbContext
    {
        public RepositoryContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Entity> Entities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new EntityConfig());
        }
    }
}
