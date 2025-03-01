namespace KogaseEngine.Domain.Entities.Iam;

public class Project
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid OwnerId { get; set; }
    public ProjectStatus Status { get; set; }
    public string Settings { get; set; } = string.Empty;

    public virtual User Owner { get; set; } = null!;
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}

public enum ProjectStatus
{
    Active,
    Archived,
    Suspended
} 