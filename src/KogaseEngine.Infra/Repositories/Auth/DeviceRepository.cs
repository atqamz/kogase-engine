using KogaseEngine.Domain.Entities.Auth;
using KogaseEngine.Domain.Interfaces.Auth;
using KogaseEngine.Infra.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KogaseEngine.Infra.Repositories.Auth;

public class DeviceRepository : Repository<Device>, IDeviceRepository
{
    public DeviceRepository(ApplicationDbContext context) : base(context)
    {
    }

    public override async Task<Device?> GetByIdAsync(Guid id)
    {
        return await _context.Devices
            .Include(d => d.Project)
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public override async Task<IEnumerable<Device>> GetAllAsync(int page = 1, int pageSize = 10)
    {
        return await _context.Devices
            .Include(d => d.Project)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public override async Task<Device> CreateAsync(Device device)
    {
        device.FirstSeenAt = DateTime.UtcNow;
        device.LastActiveAt = DateTime.UtcNow;
        return await base.CreateAsync(device);
    }

    public override Task UpdateAsync(Device device)
    {
        device.LastActiveAt = DateTime.UtcNow;
        return base.UpdateAsync(device);
    }

    public async Task<Device?> GetByInstallIdAsync(Guid projectId, string installId)
    {
        return await _context.Devices
            .FirstOrDefaultAsync(d => d.ProjectId == projectId && d.InstallId == installId);
    }

    public async Task<IEnumerable<Device>> GetByProjectIdAsync(Guid projectId, int page = 1, int pageSize = 10)
    {
        return await _context.Devices
            .Where(d => d.ProjectId == projectId)
            .OrderByDescending(d => d.LastActiveAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<Device>> GetByUserIdAsync(Guid userId)
    {
        // Get devices from sessions
        var deviceIds = await _context.Sessions
            .Where(s => s.UserId == userId)
            .Select(s => s.DeviceId)
            .Distinct()
            .ToListAsync();

        // Get devices from auth tokens
        var tokenDeviceIds = await _context.AuthTokens
            .Where(t => t.UserId == userId && t.DeviceId != null)
            .Select(t => t.DeviceId!.Value)
            .Distinct()
            .ToListAsync();

        // Combine the two lists
        var allDeviceIds = deviceIds.Union(tokenDeviceIds).Distinct().ToList();

        // Fetch the actual devices
        return await _context.Devices
            .Where(d => allDeviceIds.Contains(d.Id))
            .ToListAsync();
    }
}