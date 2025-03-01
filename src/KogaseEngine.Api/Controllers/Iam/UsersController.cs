using Microsoft.AspNetCore.Mvc;
using KogaseEngine.Core.Dtos.Iam;
using KogaseEngine.Core.Services.Iam;
using KogaseEngine.Domain.Entities.Iam;
using System.Security.Cryptography;
using System.Text;

namespace KogaseEngine.Api.Controllers.Iam;

[ApiController]
[Route("api/v1/iam/users")]
public class UsersController : ControllerBase
{
    readonly UserService _userService;
    readonly ProjectService _projectService;

    public UsersController(UserService userService, ProjectService projectService)
    {
        _userService = userService;
        _projectService = projectService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers([FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var users = await _userService.GetAllUsersAsync(page, pageSize);
        return Ok(users.Select(MapToDto));
    }

    [HttpGet("{userId:guid}")]
    public async Task<ActionResult<UserDto>> GetUserById(Guid userId)
    {
        var user = await _userService.GetUserByIdAsync(userId);
        if (user == null)
            return NotFound();

        return Ok(MapToDto(user));
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> CreateUser(CreateUserDto createDto)
    {
        // Check if email already exists
        var existingUser = await _userService.GetUserByEmailAsync(createDto.Email);
        if (existingUser != null)
            return Conflict(new { message = "A user with this email already exists" });

        var user = new User
        {
            Email = createDto.Email,
            PasswordHash = HashPassword(createDto.Password),
            FirstName = createDto.FirstName,
            LastName = createDto.LastName,
            Type = createDto.Type,
            Status = UserStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userService.CreateUserAsync(user);
        return CreatedAtAction(nameof(GetUserById), new { userId = result.Id }, MapToDto(result));
    }

    [HttpPut("{userId:guid}")]
    public async Task<ActionResult> UpdateUser(Guid userId, UpdateUserDto updateDto)
    {
        var user = await _userService.GetUserByIdAsync(userId);
        if (user == null)
            return NotFound();

        user.FirstName = updateDto.FirstName;
        user.LastName = updateDto.LastName;
        user.Status = updateDto.Status;

        await _userService.UpdateUserAsync(user);
        return NoContent();
    }

    [HttpDelete("{userId:guid}")]
    public async Task<ActionResult> DeleteUser(Guid userId)
    {
        var user = await _userService.GetUserByIdAsync(userId);
        if (user == null)
            return NotFound();

        await _userService.DeleteUserAsync(userId);
        return NoContent();
    }

    [HttpGet("{userId:guid}/projects")]
    public async Task<ActionResult<IEnumerable<ProjectDto>>> GetUserProjects(Guid userId)
    {
        var user = await _userService.GetUserByIdAsync(userId);
        if (user == null)
            return NotFound();

        var projects = await _userService.GetUserProjectsAsync(userId);
        return Ok(projects.Select(p => new ProjectDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            OwnerId = p.OwnerId,
            Status = p.Status,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt
        }));
    }

    UserDto MapToDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Type = user.Type,
            Status = user.Status,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt
        };
    }

    string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }
}