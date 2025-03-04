using KogaseEngine.Domain.Entities.Auth;
using KogaseEngine.Domain.Interfaces;
using KogaseEngine.Domain.Interfaces.Auth;
using KogaseEngine.Domain.Interfaces.Iam;

namespace KogaseEngine.Core.Services.Auth;

public class DeviceService
{
    readonly IDeviceRepository _deviceRepository;
    readonly IProjectRepository _projectRepository;
    readonly IUnitOfWork _unitOfWork;

    public DeviceService(
        IDeviceRepository deviceRepository,
        IProjectRepository projectRepository,
        IUnitOfWork unitOfWork
    )
    {
        _deviceRepository = deviceRepository;
        _projectRepository = projectRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Device?> GetDeviceByIdAsync(Guid id)
    {
        return await _deviceRepository.GetByIdAsync(id);
    }

    public async Task<Device?> GetDeviceByInstallIdAsync(Guid projectId, string installId)
    {
        return await _deviceRepository.GetByInstallIdAsync(projectId, installId);
    }

    public async Task<IEnumerable<Device>> GetDevicesByProjectIdAsync(Guid projectId, int page = 1, int pageSize = 10)
    {
        // Verify project exists
        var project = await _projectRepository.GetByIdAsync(projectId);
        if (project == null)
            throw new InvalidOperationException($"Project with ID {projectId} not found.");

        return await _deviceRepository.GetByProjectIdAsync(projectId, page, pageSize);
    }

    public async Task<IEnumerable<Device>> GetDevicesByUserIdAsync(Guid userId)
    {
        return await _deviceRepository.GetByUserIdAsync(userId);
    }

    public async Task<Device> RegisterDeviceAsync(Device device)
    {
        // Check if project exists
        var project = await _projectRepository.GetByIdAsync(device.ProjectId);
        if (project == null)
            throw new InvalidOperationException($"Project with ID {device.ProjectId} not found.");

        // Check if device already exists for this install ID and project
        var existingDevice = await _deviceRepository.GetByInstallIdAsync(device.ProjectId, device.InstallId);
        if (existingDevice != null)
        {
            // Update existing device
            existingDevice.Platform = device.Platform;
            existingDevice.DeviceModel = device.DeviceModel;
            existingDevice.OsVersion = device.OsVersion;
            existingDevice.AppVersion = device.AppVersion;
            existingDevice.LastActiveAt = DateTime.UtcNow;
            existingDevice.Status = DeviceStatus.Active;
            existingDevice.Metadata = device.Metadata;

            await _deviceRepository.UpdateAsync(existingDevice);
            await _unitOfWork.SaveChangesAsync();
            return existingDevice;
        }

        // Create new device
        var newDevice = await _deviceRepository.CreateAsync(device);
        await _unitOfWork.SaveChangesAsync();
        return newDevice;
    }

    public async Task UpdateDeviceAsync(Device device)
    {
        var existingDevice = await _deviceRepository.GetByIdAsync(device.Id);
        if (existingDevice == null)
            throw new InvalidOperationException($"Device with ID {device.Id} not found.");

        // Update properties
        existingDevice.Platform = device.Platform;
        existingDevice.DeviceModel = device.DeviceModel;
        existingDevice.OsVersion = device.OsVersion;
        existingDevice.AppVersion = device.AppVersion;
        existingDevice.LastActiveAt = DateTime.UtcNow;
        existingDevice.Status = device.Status;
        existingDevice.Metadata = device.Metadata;

        await _deviceRepository.UpdateAsync(existingDevice);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task UnregisterDeviceAsync(Guid id)
    {
        var device = await _deviceRepository.GetByIdAsync(id);
        if (device == null)
            throw new InvalidOperationException($"Device with ID {id} not found.");

        await _deviceRepository.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();
    }
}