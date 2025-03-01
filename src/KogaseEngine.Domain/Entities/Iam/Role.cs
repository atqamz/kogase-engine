namespace KogaseEngine.Domain.Entities.Iam;

public class Role
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Permissions { get; set; } = string.Empty; // JSON array of permission codes

    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
} 