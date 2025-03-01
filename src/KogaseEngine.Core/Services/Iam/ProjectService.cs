using KogaseEngine.Domain.Entities.Iam;
using KogaseEngine.Domain.Interfaces;
using KogaseEngine.Domain.Interfaces.Iam;

namespace KogaseEngine.Core.Services.Iam;

public class ProjectService
{
    readonly IProjectRepository _projectRepository;
    readonly IUnitOfWork _unitOfWork;

    public ProjectService(IProjectRepository projectRepository, IUnitOfWork unitOfWork)
    {
        _projectRepository = projectRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Project?> GetProjectByIdAsync(Guid id)
    {
        return await _projectRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Project>> GetAllProjectsAsync(int page = 1, int pageSize = 10)
    {
        return await _projectRepository.GetAllAsync(page, pageSize);
    }

    public async Task<IEnumerable<Project>> GetProjectsByOwnerIdAsync(Guid ownerId)
    {
        return await _projectRepository.GetByOwnerIdAsync(ownerId);
    }

    public async Task<Project> CreateProjectAsync(Project project)
    {
        var newProject = await _projectRepository.CreateAsync(project);
        await _unitOfWork.SaveChangesAsync();
        return newProject;
    }

    public async Task UpdateProjectAsync(Project project)
    {
        await _projectRepository.UpdateAsync(project);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteProjectAsync(Guid id)
    {
        await _projectRepository.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<Project?> GetProjectByApiKeyAsync(string apiKey)
    {
        return await _projectRepository.GetByApiKeyAsync(apiKey);
    }

    public async Task<string> RegenerateApiKeyAsync(Guid projectId)
    {
        var newApiKey = await _projectRepository.GenerateNewApiKeyAsync(projectId);
        await _unitOfWork.SaveChangesAsync();
        return newApiKey;
    }
}