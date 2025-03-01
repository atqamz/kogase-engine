using Microsoft.AspNetCore.Mvc;
using KogaseEngine.Core.Dtos.Iam;
using KogaseEngine.Core.Services.Iam;

namespace KogaseEngine.Api.Controllers.Iam;

[ApiController]
[Route("api/v1/iam/userroles")]
public class UserRolesController : ControllerBase
{
    private readonly UserRoleService _userRoleService;
    private readonly UserService _userService;
    private readonly RoleService _roleService;
    private readonly ProjectService _projectService;

    public UserRolesController(
        UserRoleService userRoleService,
        UserService userService,
        RoleService roleService,
        ProjectService projectService)
    {
        _userRoleService = userRoleService;
        _userService = userService;
        _roleService = roleService;
        _projectService = projectService;
    }

    [HttpGet("user/{userId:guid}")]
    public async Task<ActionResult<IEnumerable<UserRoleDto>>> GetUserRoles(Guid userId)
    {
        var user = await _userService.GetUserByIdAsync(userId);
        if (user == null)
            return NotFound(new { message = "User not found" });

        var userRoles = await _userRoleService.GetUserRolesByUserIdAsync(userId);
        return Ok(await MapToDtosAsync(userRoles));
    }

    [HttpGet("project/{projectId:guid}")]
    public async Task<ActionResult<IEnumerable<UserRoleDto>>> GetProjectRoles(Guid projectId)
    {
        var project = await _projectService.GetProjectByIdAsync(projectId);
        if (project == null)
            return NotFound(new { message = "Project not found" });

        var projectRoles = await _userRoleService.GetUserRolesByProjectIdAsync(projectId);
        return Ok(await MapToDtosAsync(projectRoles));
    }

    [HttpPost("assign")]
    public async Task<ActionResult<UserRoleDto>> AssignRole(AssignRoleDto assignDto, [FromQuery] Guid assignedBy)
    {
        try
        {
            var userRole = await _userRoleService.AssignRoleToUserAsync(
                assignDto.UserId,
                assignDto.RoleId,
                assignDto.ProjectId,
                assignedBy);

            // Load related entities to build the DTO
            var user = await _userService.GetUserByIdAsync(userRole.UserId);
            var role = await _roleService.GetRoleByIdAsync(userRole.RoleId);
            var project = assignDto.ProjectId.HasValue 
                ? await _projectService.GetProjectByIdAsync(assignDto.ProjectId.Value) 
                : null;
            var assigner = await _userService.GetUserByIdAsync(assignedBy);

            if (user == null || role == null || (assignDto.ProjectId.HasValue && project == null) || assigner == null)
                return BadRequest(new { message = "One or more related entities not found" });

            return Ok(new UserRoleDto
            {
                UserId = userRole.UserId,
                UserName = $"{user.FirstName} {user.LastName}",
                RoleId = userRole.RoleId,
                RoleName = role.Name,
                ProjectId = userRole.ProjectId,
                ProjectName = project?.Name,
                AssignedAt = userRole.AssignedAt,
                AssignedBy = userRole.AssignedBy,
                AssignerName = $"{assigner.FirstName} {assigner.LastName}"
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("remove")]
    public async Task<ActionResult> RemoveRole([FromQuery] Guid userId, [FromQuery] Guid roleId, [FromQuery] Guid? projectId)
    {
        var userRole = await _userRoleService.GetUserRoleAsync(userId, roleId, projectId);
        if (userRole == null)
            return NotFound(new { message = "User role assignment not found" });

        await _userRoleService.RemoveRoleFromUserAsync(userId, roleId, projectId);
        return NoContent();
    }

    private async Task<IEnumerable<UserRoleDto>> MapToDtosAsync(IEnumerable<Domain.Entities.Iam.UserRole> userRoles)
    {
        var dtos = new List<UserRoleDto>();
        
        foreach (var userRole in userRoles)
        {
            var user = await _userService.GetUserByIdAsync(userRole.UserId);
            var role = await _roleService.GetRoleByIdAsync(userRole.RoleId);
            var project = userRole.ProjectId.HasValue 
                ? await _projectService.GetProjectByIdAsync(userRole.ProjectId.Value) 
                : null;
            var assigner = await _userService.GetUserByIdAsync(userRole.AssignedBy);

            if (user == null || role == null || assigner == null)
                continue;

            dtos.Add(new UserRoleDto
            {
                UserId = userRole.UserId,
                UserName = $"{user.FirstName} {user.LastName}",
                RoleId = userRole.RoleId,
                RoleName = role.Name,
                ProjectId = userRole.ProjectId,
                ProjectName = project?.Name,
                AssignedAt = userRole.AssignedAt,
                AssignedBy = userRole.AssignedBy,
                AssignerName = $"{assigner.FirstName} {assigner.LastName}"
            });
        }

        return dtos;
    }
}