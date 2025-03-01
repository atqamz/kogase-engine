using KogaseEngine.Domain.Entities.Iam;
using KogaseEngine.Domain.Interfaces;
using KogaseEngine.Domain.Interfaces.Iam;

namespace KogaseEngine.Core.Services.Iam;

public class RoleService
{
    readonly IRoleRepository _roleRepository;
    readonly IUnitOfWork _unitOfWork;

    public RoleService(IRoleRepository roleRepository, IUnitOfWork unitOfWork)
    {
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Role?> GetRoleByIdAsync(Guid id)
    {
        return await _roleRepository.GetByIdAsync(id);
    }

    public async Task<Role?> GetRoleByNameAsync(string name)
    {
        return await _roleRepository.GetByNameAsync(name);
    }

    public async Task<IEnumerable<Role>> GetAllRolesAsync()
    {
        return await _roleRepository.GetAllAsync();
    }

    public async Task<Role> CreateRoleAsync(Role role)
    {
        // Check if role name already exists
        var existingRole = await _roleRepository.GetByNameAsync(role.Name);
        if (existingRole != null)
            throw new InvalidOperationException($"A role with name {role.Name} already exists.");

        var newRole = await _roleRepository.CreateAsync(role);
        await _unitOfWork.SaveChangesAsync();
        return newRole;
    }

    public async Task UpdateRoleAsync(Role role)
    {
        await _roleRepository.UpdateAsync(role);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteRoleAsync(Guid id)
    {
        await _roleRepository.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();
    }
}