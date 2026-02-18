using Microsoft.EntityFrameworkCore;
using TaskTrackingApi.Models;
namespace TaskTrackingApi.Models;



public class PermissionService : IPermissionService
{
    private readonly TaskTrackingDbContext _context;

    public PermissionService(TaskTrackingDbContext context)
    {
        _context = context;
    }

    public async Task<bool> HasPermissionAsync(int userId, string permissionKey)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);

        if (user?.Role == null)
            return false;

        // Find the permission by key
        var permission = await _context.Permissions
            .FirstOrDefaultAsync(p => p.KeyName == permissionKey && p.IsActive);

        if (permission == null)
            return false;

        // Check RolePermissions
        var rolePermission = await _context.RolePermissions
            .FirstOrDefaultAsync(rp => rp.RoleId == user.Role.Id && rp.PermissionId == permission.Id);

        return rolePermission?.IsAllowed ?? false;
    }
}

