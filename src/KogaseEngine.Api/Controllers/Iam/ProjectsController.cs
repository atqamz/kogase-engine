using Microsoft.AspNetCore.Mvc;
using KogaseEngine.Core.Dtos.Iam;
using KogaseEngine.Core.Services.Iam;
using KogaseEngine.Domain.Entities.Iam;

namespace KogaseEngine.Api.Controllers.Iam;

[ApiController]
[Route("api/v1/iam/projects")]
public class ProjectsController : ControllerBase
{
    readonly ProjectService _projectService;
    readonly UserService _userService;

    public ProjectsController(ProjectService projectService, UserService userService)
    {
        _projectService = projectService;
        _userService = userService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProjectDto>>> GetProjects([FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var projects = await _projectService.GetAllProjectsAsync(page, pageSize);
        return Ok(projects.Select(MapToDto));
    }

    [HttpGet("{projectId:guid}")]
    public async Task<ActionResult<ProjectDto>> GetProjectById(Guid projectId)
    {
        var project = await _projectService.GetProjectByIdAsync(projectId);
        if (project == null)
            return NotFound();

        return Ok(MapToDto(project));
    }

    [HttpPost]
    public async Task<ActionResult<ProjectDto>> CreateProject(CreateProjectDto createDto)
    {
        var project = new Project
        {
            Name = createDto.Name,
            Description = createDto.Description,
            OwnerId = createDto.OwnerId,
            Status = ProjectStatus.Active,
            Settings = createDto.Settings
        };

        var result = await _projectService.CreateProjectAsync(project);
        return CreatedAtAction(nameof(GetProjectById), new { projectId = result.Id }, MapToDto(result));
    }

    [HttpPut("{projectId:guid}")]
    public async Task<ActionResult> UpdateProject(Guid projectId, UpdateProjectDto updateDto)
    {
        var project = await _projectService.GetProjectByIdAsync(projectId);
        if (project == null)
            return NotFound();

        project.Name = updateDto.Name;
        project.Description = updateDto.Description;
        project.Status = updateDto.Status;
        project.Settings = updateDto.Settings;

        await _projectService.UpdateProjectAsync(project);
        return NoContent();
    }

    [HttpDelete("{projectId:guid}")]
    public async Task<ActionResult> DeleteProject(Guid projectId)
    {
        var project = await _projectService.GetProjectByIdAsync(projectId);
        if (project == null)
            return NotFound();

        await _projectService.DeleteProjectAsync(projectId);
        return NoContent();
    }

    [HttpPost("{projectId:guid}/regenerate-key")]
    public async Task<ActionResult<string>> RegenerateApiKey(Guid projectId)
    {
        var project = await _projectService.GetProjectByIdAsync(projectId);
        if (project == null)
            return NotFound();

        var newApiKey = await _projectService.RegenerateApiKeyAsync(projectId);
        return Ok(new { apiKey = newApiKey });
    }

    ProjectDto MapToDto(Project project)
    {
        return new ProjectDto
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            OwnerId = project.OwnerId,
            OwnerName = project.Owner?.FirstName + " " + project.Owner?.LastName,
            ApiKey = project.ApiKey,
            Status = project.Status,
            CreatedAt = project.CreatedAt,
            UpdatedAt = project.UpdatedAt,
            Settings = project.Settings
        };
    }
}