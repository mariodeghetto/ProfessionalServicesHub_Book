using Microsoft.EntityFrameworkCore;
using ProfessionalServicesHub.Domain.Clients;

namespace ProfessionalServicesHub.Infrastructure.Data;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options)
{
    public DbSet<Client> Clients => Set<Client>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var client = modelBuilder.Entity<Client>();

        client.Property(x => x.Code)
            .HasMaxLength(20)
            .IsRequired();

        client.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        client.Property(x => x.City)
            .HasMaxLength(100);

        client.Property(x => x.Email)
            .HasMaxLength(254);

        client.HasIndex(x => x.Code)
            .IsUnique();

        client.HasIndex(x => x.Name);
    }
}
