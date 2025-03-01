using KogaseEngine.Domain.Entities.Iam;
using KogaseEngine.Domain.Interfaces;
using KogaseEngine.Domain.Interfaces.Iam;

namespace KogaseEngine.Core.Services.Iam;

public class UserService
{
    readonly IUserRepository _userRepository;
    readonly IUnitOfWork _unitOfWork;

    public UserService(IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<User?> GetUserByIdAsync(Guid id)
    {
        return await _userRepository.GetByIdAsync(id);
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _userRepository.GetByEmailAsync(email);
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync(int page = 1, int pageSize = 10)
    {
        return await _userRepository.GetAllAsync(page, pageSize);
    }

    public async Task<User> CreateUserAsync(User user)
    {
        // Check if email already exists
        var existingUser = await _userRepository.GetByEmailAsync(user.Email);
        if (existingUser != null)
            throw new InvalidOperationException($"A user with email {user.Email} already exists.");

        var newUser = await _userRepository.CreateAsync(user);
        await _unitOfWork.SaveChangesAsync();
        return newUser;
    }

    public async Task UpdateUserAsync(User user)
    {
        await _userRepository.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteUserAsync(Guid id)
    {
        await _userRepository.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<IEnumerable<Project?>> GetUserProjectsAsync(Guid userId)
    {
        return await _userRepository.GetUserProjectsAsync(userId);
    }
}