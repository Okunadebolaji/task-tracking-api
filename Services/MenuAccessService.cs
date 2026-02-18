using Microsoft.EntityFrameworkCore;
using TaskTrackingApi.Models;
namespace TaskTrackingApi.Models;

public class MenuAccessService : IMenuAccessService
{
    private readonly TaskTrackingDbContext _context;

    public MenuAccessService(TaskTrackingDbContext context)
    {
        _context = context;
    }

    public async Task<bool> CanViewMenuAsync(int userId, string menuKey)
    {
        return await _context.Users
            .Where(u => u.Id == userId && u.IsActive)
            .AnyAsync(u =>
                u.Role!.RoleMenuPermissions.Any(rmp =>
                    rmp.Menu.UniqueKey == menuKey &&
                    rmp.CanView
                )
            );
    }
}
