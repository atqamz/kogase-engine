using Microsoft.EntityFrameworkCore;
using KogaseEngine.Domain.Entities.Iam;
using KogaseEngine.Domain.Entities.Auth;
using KogaseEngine.Domain.Entities.Telemetry;

namespace KogaseEngine.Infra.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // IAM Module
    public DbSet<Project> Projects { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Role> Roles { get; set; } = null!;
    public DbSet<UserRole> UserRoles { get; set; } = null!;

    // Auth Module
    public DbSet<Device> Devices { get; set; } = null!;
    public DbSet<Session> Sessions { get; set; } = null!;
    public DbSet<AuthToken> AuthTokens { get; set; } = null!;

    // Telemetry Module
    public DbSet<TelemetryEvent> TelemetryEvents { get; set; } = null!;
    public DbSet<PlaySession> PlaySessions { get; set; } = null!;
    public DbSet<MetricAggregate> MetricAggregates { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure IAM entities
        modelBuilder.Entity<UserRole>()
            .HasKey(ur => new { ur.UserId, ur.RoleId });

        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(ur => ur.UserId);
        
        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.Project)
            .WithMany(p => p.UserRoles)
            .HasForeignKey(ur => ur.ProjectId)
            .IsRequired(false); // This makes the ProjectId foreign key optional

        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.RoleId);

        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.Assigner)
            .WithMany()
            .HasForeignKey(ur => ur.AssignedBy)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Project>()
            .HasOne(p => p.Owner)
            .WithMany(u => u.OwnedProjects)
            .HasForeignKey(p => p.OwnerId);

        modelBuilder.Entity<Project>()
            .HasIndex(p => p.ApiKey)
            .IsUnique();

        // Auth Module configurations
        modelBuilder.Entity<Device>()
            .HasIndex(d => new { d.ProjectId, d.InstallId })
            .IsUnique();

        // Telemetry Module configurations
        modelBuilder.Entity<TelemetryEvent>()
            .HasIndex(e => new { e.ProjectId, e.Timestamp })
            .IsDescending(false, true);

        modelBuilder.Entity<MetricAggregate>()
            .HasIndex(m => new { m.ProjectId, m.MetricName, m.Date, m.Dimension, m.DimensionValue })
            .IsUnique();
    }
}