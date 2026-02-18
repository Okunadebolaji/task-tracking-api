using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskTrackingApi.Dtos;
using TaskTrackingApi.Models;

namespace TaskTrackingApi.Controllers
{
   [ApiController]
[Route("api/role-menu-permissions")]
public class RoleMenuPermissionsController : ControllerBase
{
    private readonly TaskTrackingDbContext _db;

    public RoleMenuPermissionsController(TaskTrackingDbContext db)
    {
        _db = db;
    }

    [HttpGet("{roleId}")]
    public async Task<IActionResult> Get(int roleId)
    {
        var perms = await _db.RoleMenuPermissions
            .Include(r => r.Menu)
            .Where(r => r.RoleId == roleId)
            .Select(r => new
            {
                r.MenuId,
                MenuName = r.Menu.Name,
                r.CanView,
                r.CanCreate,
                r.CanEdit,
                r.CanDelete,
                r.CanApprove,
                r.CanReject
            })
            .ToListAsync();

        return Ok(perms);
    }

    [HttpPut("{roleId}")]
    public async Task<IActionResult> Update(
        int roleId,
        [FromBody] List<RoleMenuPermission> perms
    )
    {
        var existing = await _db.RoleMenuPermissions
            .Where(r => r.RoleId == roleId)
            .ToListAsync();

        _db.RoleMenuPermissions.RemoveRange(existing);

        foreach (var p in perms)
        {
            p.RoleId = roleId;
            _db.RoleMenuPermissions.Add(p);
        }

        await _db.SaveChangesAsync();
        return Ok();
    }
}


}