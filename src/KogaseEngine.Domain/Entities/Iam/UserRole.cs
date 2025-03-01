namespace KogaseEngine.Domain.Entities.Iam;

public class UserRole
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public Guid? ProjectId { get; set; }
    public DateTime AssignedAt { get; set; }
    public Guid AssignedBy { get; set; }

    public virtual User User { get; set; } = null!;
    public virtual Role Role { get; set; } = null!;
    public virtual Project? Project { get; set; }
    public virtual User Assigner { get; set; } = null!;
} 