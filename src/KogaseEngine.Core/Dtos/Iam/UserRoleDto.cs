using System;

namespace KogaseEngine.Core.Dtos.Iam;

public class UserRoleDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public Guid RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public Guid? ProjectId { get; set; }
    public string? ProjectName { get; set; }
    public DateTime AssignedAt { get; set; }
    public Guid AssignedBy { get; set; }
    public string AssignerName { get; set; } = string.Empty;
}

public class AssignRoleDto
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public Guid? ProjectId { get; set; }
}