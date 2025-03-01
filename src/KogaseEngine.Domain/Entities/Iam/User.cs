namespace KogaseEngine.Domain.Entities.Iam;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public UserType Type { get; set; }
    public UserStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }

    public virtual ICollection<Project> OwnedProjects { get; set; } = new List<Project>();
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}

public enum UserType
{
    Admin,
    Developer,
    Player
}

public enum UserStatus
{
    Active,
    Inactive,
    Suspended
} 