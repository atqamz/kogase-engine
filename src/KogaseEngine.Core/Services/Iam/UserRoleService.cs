using KogaseEngine.Domain.Entities.Iam;
using KogaseEngine.Domain.Interfaces;
using KogaseEngine.Domain.Interfaces.Iam;

namespace KogaseEngine.Core.Services.Iam;

public class UserRoleService
{
    readonly IUserRoleRepository _userRoleRepository;
    readonly IUserRepository _userRepository;
    readonly IRoleRepository _roleRepository;
    readonly IProjectRepository _projectRepository;
    readonly IUnitOfWork _unitOfWork;

    public UserRoleService(
        IUserRoleRepository userRoleRepository,
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IProjectRepository projectRepository,
        IUnitOfWork unitOfWork
    )
    {
        _userRoleRepository = userRoleRepository;
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _projectRepository = projectRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<UserRole?> GetUserRoleAsync(Guid userId, Guid roleId, Guid? projectId)
    {
        return await _userRoleRepository.GetAsync(userId, roleId, projectId);
    }

    public async Task<IEnumerable<UserRole>> GetUserRolesByUserIdAsync(Guid userId)
    {
        return await _userRoleRepository.GetByUserIdAsync(userId);
    }

    public async Task<IEnumerable<UserRole>> GetUserRolesByProjectIdAsync(Guid projectId)
    {
        return await _userRoleRepository.GetByProjectIdAsync(projectId);
    }

    public async Task<UserRole> AssignRoleToUserAsync(Guid userId, Guid roleId, Guid? projectId, Guid assignedBy)
    {
        // Validate that user, role, and project (if provided) exist
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new InvalidOperationException($"User with ID {userId} not found.");

        var role = await _roleRepository.GetByIdAsync(roleId);
        if (role == null)
            throw new InvalidOperationException($"Role with ID {roleId} not found.");

        if (projectId.HasValue)
        {
            var project = await _projectRepository.GetByIdAsync(projectId.Value);
            if (project == null)
                throw new InvalidOperationException($"Project with ID {projectId} not found.");
        }

        // Assign the role
        var userRole = new UserRole
        {
            UserId = userId,
            RoleId = roleId,
            ProjectId = projectId,
            AssignedBy = assignedBy
        };

        var result = await _userRoleRepository.AssignRoleAsync(userRole);
        await _unitOfWork.SaveChangesAsync();
        return result;
    }

    public async Task RemoveRoleFromUserAsync(Guid userId, Guid roleId, Guid? projectId)
    {
        await _userRoleRepository.RemoveRoleAsync(userId, roleId, projectId);
        await _unitOfWork.SaveChangesAsync();
    }
}