using Microsoft.AspNetCore.Mvc;
using KogaseEngine.Core.Dtos.Iam;
using KogaseEngine.Core.Services.Iam;
using KogaseEngine.Domain.Entities.Iam;
using System.Text.Json;

namespace KogaseEngine.Api.Controllers.Iam;

[ApiController]
[Route("api/v1/iam/roles")]
public class RolesController : ControllerBase
{
    private readonly RoleService _roleService;

    public RolesController(RoleService roleService)
    {
        _roleService = roleService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<RoleDto>>> GetRoles()
    {
        var roles = await _roleService.GetAllRolesAsync();
        return Ok(roles.Select(MapToDto));
    }

    [HttpGet("{roleId:guid}")]
    public async Task<ActionResult<RoleDto>> GetRoleById(Guid roleId)
    {
        var role = await _roleService.GetRoleByIdAsync(roleId);
        if (role == null)
            return NotFound();

        return Ok(MapToDto(role));
    }

    [HttpPost]
    public async Task<ActionResult<RoleDto>> CreateRole(CreateRoleDto createDto)
    {
        // Check if role name already exists
        var existingRole = await _roleService.GetRoleByNameAsync(createDto.Name);
        if (existingRole != null)
            return Conflict(new { message = "A role with this name already exists" });

        var role = new Role
        {
            Name = createDto.Name,
            Description = createDto.Description,
            Permissions = JsonSerializer.Serialize(createDto.Permissions)
        };

        var result = await _roleService.CreateRoleAsync(role);
        return CreatedAtAction(nameof(GetRoleById), new { roleId = result.Id }, MapToDto(result));
    }

    [HttpPut("{roleId:guid}")]
    public async Task<ActionResult> UpdateRole(Guid roleId, UpdateRoleDto updateDto)
    {
        var role = await _roleService.GetRoleByIdAsync(roleId);
        if (role == null)
            return NotFound();

        role.Description = updateDto.Description;
        role.Permissions = JsonSerializer.Serialize(updateDto.Permissions);

        await _roleService.UpdateRoleAsync(role);
        return NoContent();
    }

    [HttpDelete("{roleId:guid}")]
    public async Task<ActionResult> DeleteRole(Guid roleId)
    {
        var role = await _roleService.GetRoleByIdAsync(roleId);
        if (role == null)
            return NotFound();

        await _roleService.DeleteRoleAsync(roleId);
        return NoContent();
    }

    private RoleDto MapToDto(Role role)
    {
        string[] permissions;
        try
        {
            permissions = JsonSerializer.Deserialize<string[]>(role.Permissions) ?? Array.Empty<string>();
        }
        catch
        {
            permissions = Array.Empty<string>();
        }

        return new RoleDto
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            Permissions = permissions
        };
    }
}