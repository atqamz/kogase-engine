using System;

namespace KogaseEngine.Core.Dtos.Iam;

public class RoleDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string[] Permissions { get; set; } = [];
}

public class CreateRoleDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string[] Permissions { get; set; } = [];
}

public class UpdateRoleDto
{
    public string Description { get; set; } = string.Empty;
    public string[] Permissions { get; set; } = [];
}