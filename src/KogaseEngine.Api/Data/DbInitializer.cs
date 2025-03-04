using KogaseEngine.Domain.Entities.Iam;
using KogaseEngine.Infra.Persistence;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace KogaseEngine.Api.Data;

public static class DbInitializer
{
    public static async Task Initialize(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Apply pending migrations if they exist
        await context.Database.MigrateAsync();

        // Check if data already exists
        if (context.Users.Any()) return; // DB has been seeded

        // Create admin user
        var adminUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "admin@kogase.io",
            PasswordHash = HashPassword("Admin123!"),
            FirstName = "Admin",
            LastName = "User",
            Type = UserType.Admin,
            Status = UserStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        // Create basic roles
        var adminRole = new Role
        {
            Id = Guid.NewGuid(),
            Name = "Administrator",
            Description = "Full system access",
            Permissions = JsonSerializer.Serialize(new[] { "all" })
        };

        var developerRole = new Role
        {
            Id = Guid.NewGuid(),
            Name = "Developer",
            Description = "Project management access",
            Permissions = JsonSerializer.Serialize(new[] { "project.read", "project.write", "telemetry.read" })
        };

        var viewerRole = new Role
        {
            Id = Guid.NewGuid(),
            Name = "Viewer",
            Description = "Read-only access",
            Permissions = JsonSerializer.Serialize(new[] { "project.read", "telemetry.read" })
        };

        // Create a sample project
        var demoProject = new Project
        {
            Id = Guid.NewGuid(),
            Name = "Demo Project",
            Description = "A demo project for testing",
            ApiKey = GenerateApiKey(),
            OwnerId = adminUser.Id,
            Status = ProjectStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Settings = "{\"allowAnonymousTelemetry\": true}"
        };

        // Assign admin role to admin user
        var userRole = new UserRole
        {
            UserId = adminUser.Id,
            RoleId = adminRole.Id,
            ProjectId = null, // System-wide role (null ProjectId is valid since it's not part of the primary key)
            AssignedAt = DateTime.UtcNow,
            AssignedBy = adminUser.Id
        };

        // Add data to context
        context.Users.Add(adminUser);
        context.Roles.AddRange(adminRole, developerRole, viewerRole);
        context.Projects.Add(demoProject);
        context.UserRoles.Add(userRole);

        await context.SaveChangesAsync();
    }

    static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    static string GenerateApiKey()
    {
        var key = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(key);
        }

        return Convert.ToBase64String(key).Replace("/", "_").Replace("+", "-").Replace("=", "");
    }
}