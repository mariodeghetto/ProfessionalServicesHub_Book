using Microsoft.EntityFrameworkCore;
using ProfessionalServicesHub.Domain.Calendar;
using ProfessionalServicesHub.Domain.Clients;
using ProfessionalServicesHub.Domain.Documents;
using ProfessionalServicesHub.Domain.Work;

namespace ProfessionalServicesHub.Infrastructure.Data;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options)
{
    public DbSet<Client> Clients => Set<Client>();

    public DbSet<Engagement> Engagements => Set<Engagement>();

    public DbSet<WorkActivity> WorkActivities => Set<WorkActivity>();

    public DbSet<CalendarEntry> CalendarEntries => Set<CalendarEntry>();

    public DbSet<BusinessDocument> Documents => Set<BusinessDocument>();

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

        var engagement = modelBuilder.Entity<Engagement>();

        engagement.Property(x => x.Code)
            .HasMaxLength(30)
            .IsRequired();

        engagement.Property(x => x.Title)
            .HasMaxLength(200)
            .IsRequired();

        engagement.HasIndex(x => x.Code)
            .IsUnique();

        engagement.HasOne(x => x.Client)
            .WithMany()
            .HasForeignKey(x => x.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        var workActivity = modelBuilder.Entity<WorkActivity>();

        workActivity.Property(x => x.Title)
            .HasMaxLength(200)
            .IsRequired();

        workActivity.Property(x => x.Description)
            .HasMaxLength(2000);

        workActivity.Property(x => x.Assignee)
            .HasMaxLength(120);

        workActivity.HasIndex(x => new
        {
            x.EngagementId,
            x.Status
        });

        workActivity.HasOne(x => x.Engagement)
            .WithMany(x => x.Activities)
            .HasForeignKey(x => x.EngagementId)
            .OnDelete(DeleteBehavior.Cascade);

        var calendarEntry = modelBuilder.Entity<CalendarEntry>();

        calendarEntry.Property(x => x.Subject)
            .HasMaxLength(200)
            .IsRequired();

        calendarEntry.Property(x => x.Location)
            .HasMaxLength(200);

        calendarEntry.Property(x => x.Assignee)
            .HasMaxLength(120);

        calendarEntry.Property(x => x.Description)
            .HasMaxLength(2000);

        calendarEntry.HasIndex(x => x.StartTime);
        calendarEntry.HasIndex(x => x.EndTime);
        calendarEntry.HasIndex(x => x.EngagementId);
        calendarEntry.HasIndex(x => x.Assignee);

        calendarEntry.HasOne(x => x.Client)
            .WithMany()
            .HasForeignKey(x => x.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        calendarEntry.HasOne(x => x.Engagement)
            .WithMany()
            .HasForeignKey(x => x.EngagementId)
            .OnDelete(DeleteBehavior.Restrict);

        calendarEntry.HasOne(x => x.WorkActivity)
            .WithMany()
            .HasForeignKey(x => x.WorkActivityId)
            .OnDelete(DeleteBehavior.SetNull);
        var document = modelBuilder.Entity<BusinessDocument>();

        document.Property(x => x.OriginalFileName)
            .HasMaxLength(260)
            .IsRequired();

        document.Property(x => x.StorageKey)
            .HasMaxLength(160)
            .IsRequired();

        document.Property(x => x.ContentType)
            .HasMaxLength(120)
            .IsRequired();

        document.Property(x => x.Sha256)
            .HasMaxLength(64)
            .IsRequired();

        document.Property(x => x.Description)
            .HasMaxLength(1000);

        document.Property(x => x.UploadedBy)
            .HasMaxLength(120);

        document.HasIndex(x => x.StorageKey)
            .IsUnique();

        document.HasIndex(x => x.Sha256);
        document.HasIndex(x => x.EngagementId);
        document.HasIndex(x => x.UploadedAtUtc);

        document.HasOne(x => x.Client)
            .WithMany()
            .HasForeignKey(x => x.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        document.HasOne(x => x.Engagement)
            .WithMany()
            .HasForeignKey(x => x.EngagementId)
            .OnDelete(DeleteBehavior.Restrict);

        document.HasOne(x => x.WorkActivity)
            .WithMany()
            .HasForeignKey(x => x.WorkActivityId)
            .OnDelete(DeleteBehavior.SetNull);

    }
}
