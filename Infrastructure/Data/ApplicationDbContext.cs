// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.EntityFrameworkCore;
using YouTubeShortsAutomator.Domain.Models;

namespace YouTubeShortsAutomator.Infrastructure.Repositories;

/// <summary>
/// Entity Framework Core database context for the application
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    // Entity DbSets
    public DbSet<User> Users => Set<User>();
    public DbSet<Video> Videos => Set<Video>();
    public DbSet<ProcessingJob> ProcessingJobs => Set<ProcessingJob>();
    public DbSet<ProcessingStep> ProcessingSteps => Set<ProcessingStep>();
    public DbSet<ProcessingError> ProcessingErrors => Set<ProcessingError>();
    public DbSet<UploadResult> UploadResults => Set<UploadResult>();
    public DbSet<UploadMetadata> UploadMetadata => Set<UploadMetadata>();
    public DbSet<AnalyticsMetric> AnalyticsMetrics => Set<AnalyticsMetric>();
    public DbSet<DemographicMetric> DemographicMetrics => Set<DemographicMetric>();
    public DbSet<ApiCredential> ApiCredentials => Set<ApiCredential>();
    public DbSet<UploadSchedule> UploadSchedules => Set<UploadSchedule>();
    public DbSet<ScheduledUpload> ScheduledUploads => Set<ScheduledUpload>();

    /// <summary>
    /// Configures the database model
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(254);
            entity.Property(e => e.DisplayName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ChannelId).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.ChannelId);
        });

        // Video entity
        modelBuilder.Entity<Video>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(5000);
            entity.Property(e => e.FilePath).IsRequired();
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasOne(e => e.User)
                .WithMany(u => u.Videos)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.UploadResult)
                .WithOne(ur => ur.Video)
                .HasForeignKey<UploadResult>(ur => ur.VideoId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ProcessingJob entity
        modelBuilder.Entity<ProcessingJob>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.VideoId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasOne(e => e.Video)
                .WithMany(v => v.ProcessingJobs)
                .HasForeignKey(e => e.VideoId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ProcessingStep entity
        modelBuilder.Entity<ProcessingStep>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.JobId);
            entity.HasOne<ProcessingJob>()
                .WithMany(j => j.Steps)
                .HasForeignKey(e => e.JobId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ProcessingError entity
        modelBuilder.Entity<ProcessingError>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ErrorMessage).IsRequired();
            entity.HasIndex(e => e.JobId);
            entity.HasIndex(e => e.ErrorType);
            entity.HasOne(e => e.Job)
                .WithMany(j => j.Errors)
                .HasForeignKey(e => e.JobId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // UploadResult entity
        modelBuilder.Entity<UploadResult>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.YouTubeVideoId).IsRequired();
            entity.Property(e => e.YouTubeUrl).IsRequired();
            entity.HasIndex(e => e.VideoId);
            entity.HasIndex(e => e.YouTubeVideoId);
            entity.HasIndex(e => e.Status);
        });

        // UploadMetadata entity
        modelBuilder.Entity<UploadMetadata>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UploadResultId);
            entity.HasOne<UploadResult>()
                .WithMany(ur => ur.UploadMetadata)
                .HasForeignKey(e => e.UploadResultId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // AnalyticsMetric entity
        modelBuilder.Entity<AnalyticsMetric>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.VideoId);
            entity.HasIndex(e => e.Period);
            entity.HasIndex(e => e.CollectedAt);
            entity.HasOne(e => e.Video)
                .WithMany(v => v.Metrics)
                .HasForeignKey(e => e.VideoId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // DemographicMetric entity
        modelBuilder.Entity<DemographicMetric>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.MetricId);
            entity.HasOne<AnalyticsMetric>()
                .WithMany(m => m.Demographics)
                .HasForeignKey(e => e.MetricId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ApiCredential entity
        modelBuilder.Entity<ApiCredential>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ClientId).IsRequired();
            entity.Property(e => e.ClientSecret).IsRequired();
            entity.Property(e => e.AccessToken).IsRequired();
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Status);
            entity.HasOne(e => e.User)
                .WithOne(u => u.ApiCredential)
                .HasForeignKey<ApiCredential>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // UploadSchedule entity
        modelBuilder.Entity<UploadSchedule>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ScheduleName).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.UserId);
            entity.HasOne(e => e.User)
                .WithOne(u => u.UploadSchedule)
                .HasForeignKey<UploadSchedule>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ScheduledUpload entity
        modelBuilder.Entity<ScheduledUpload>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ScheduleId);
            entity.HasIndex(e => e.Status);
            entity.HasOne<UploadSchedule>()
                .WithMany(s => s.ScheduledUploads)
                .HasForeignKey(e => e.ScheduleId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
