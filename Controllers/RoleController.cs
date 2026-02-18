using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskTrackingApi.Models;

namespace TaskTrackingApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RolesController : ControllerBase
    {
        private readonly TaskTrackingDbContext _db;
private readonly IPermissionService _permissionService;

public RolesController(TaskTrackingDbContext db, IPermissionService permissionService)
{
    _db = db;
    _permissionService = permissionService;
}


        // GET: api/roles
      [HttpGet]
public async Task<IActionResult> GetAll([FromHeader(Name = "x-user-id")] string userIdHeader)
{
    if (!int.TryParse(userIdHeader, out var userId))
        return BadRequest("Invalid x-user-id");

    var user = await _db.Users.FindAsync(userId);
    if (user == null) return Unauthorized();

    if (!await _permissionService.HasPermissionAsync(userId, "ROLE_MANAGE"))
        return StatusCode(403, "Permission denied");

    return Ok(await _db.Roles.ToListAsync());
}


        // GET: api/roles/{roleId}/permissions
        [HttpGet("{roleId}/permissions")]
public async Task<IActionResult> GetRolePermissions(
    int roleId,
    [FromHeader(Name = "x-user-id")] string userIdHeader)
{
    if (!int.TryParse(userIdHeader, out var userId))
        return BadRequest("Invalid x-user-id");

    var user = await _db.Users.FindAsync(userId);
    if (user == null) return Unauthorized();

    if (!await _permissionService.HasPermissionAsync(userId, "ROLE_PERMISSIONS_MANAGE"))
        return StatusCode(403, "Permission denied");

    var permissions = await _db.Permissions
        .Select(p => new
        {
            p.Id,
            p.Name,
            p.KeyName,
            IsAllowed = _db.RolePermissions
                .Any(rp => rp.RoleId == roleId && rp.PermissionId == p.Id && rp.IsAllowed)
        })
        .OrderBy(p => p.Name)
        .ToListAsync();

    return Ok(permissions);
}

        // POST: api/roles/{roleId}/permissions
       [HttpPost("{roleId}/permissions")]
public async Task<IActionResult> SaveRolePermissions(
    int roleId,
    [FromBody] List<RolePermissionDto> permissions,
    [FromHeader(Name = "x-user-id")] string userIdHeader)
{
    if (!int.TryParse(userIdHeader, out var userId))
        return BadRequest("Invalid x-user-id");

    var user = await _db.Users.FindAsync(userId);
    if (user == null) return Unauthorized();

    if (!await _permissionService.HasPermissionAsync(userId, "ROLE_PERMISSIONS_MANAGE"))
        return StatusCode(403, "Permission denied");

    var existing = await _db.RolePermissions
        .Where(rp => rp.RoleId == roleId)
        .ToListAsync();

    _db.RolePermissions.RemoveRange(existing);

    foreach (var p in permissions)
    {
        _db.RolePermissions.Add(new RolePermission
        {
            RoleId = roleId,
            PermissionId = p.PermissionId,
            IsAllowed = p.IsAllowed
        });
    }

    await _db.SaveChangesAsync();
    return Ok();
}
    public class RolePermissionDto
    {
        public int PermissionId { get; set; }
        public bool IsAllowed { get; set; }
    }
}

}
