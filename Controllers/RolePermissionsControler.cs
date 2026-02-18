using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskTrackingApi.Dtos;
using TaskTrackingApi.Models;

namespace TaskTrackingApi.Controllers
{
    [ApiController]
[Route("api/role-permissions")]
public class RolePermissionsController : ControllerBase
{
    private readonly TaskTrackingDbContext _db;
private readonly IPermissionService _permissionService;

public RolePermissionsController(TaskTrackingDbContext db, IPermissionService permissionService)
{
    _db = db;
    _permissionService = permissionService;
}


    // 🔹 Get permissions for a role
   [HttpGet("{roleId}")]
public async Task<IActionResult> GetByRole(
    int roleId,
    [FromHeader(Name = "x-user-id")] string userIdHeader)
{
    if (!int.TryParse(userIdHeader, out var userId))
        return BadRequest("Invalid x-user-id");

    var user = await _db.Users.FindAsync(userId);
    if (user == null) return Unauthorized();

    if (!await _permissionService.HasPermissionAsync(userId, "ROLE_PERMISSIONS_MANAGE"))
        return StatusCode(403, "Permission denied");

    var permissions = await _db.RolePermissions
        .Where(rp => rp.RoleId == roleId)
        .Select(rp => new
        {
            rp.PermissionId,
            rp.IsAllowed
        })
        .ToListAsync();

    return Ok(permissions);
}

    // 🔹 Save permissions for a role (bulk)
   [HttpPost]
public async Task<IActionResult> Save(
    List<RolePermission> items,
    [FromHeader(Name = "x-user-id")] string userIdHeader)
{
    if (!int.TryParse(userIdHeader, out var userId))
        return BadRequest("Invalid x-user-id");

    var user = await _db.Users.FindAsync(userId);
    if (user == null) return Unauthorized();

    if (!await _permissionService.HasPermissionAsync(userId, "ROLE_PERMISSIONS_MANAGE"))
        return StatusCode(403, "Permission denied");

    foreach (var item in items)
    {
        var existing = await _db.RolePermissions
            .FirstOrDefaultAsync(x =>
                x.RoleId == item.RoleId &&
                x.PermissionId == item.PermissionId);

        if (existing == null)
        {
            _db.RolePermissions.Add(item);
        }
        else
        {
            existing.IsAllowed = item.IsAllowed;
        }
    }

    await _db.SaveChangesAsync();
    return Ok();
}

}




}