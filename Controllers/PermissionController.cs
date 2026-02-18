using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskTrackingApi.Models;

namespace TaskTrackingApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PermissionsController : ControllerBase
    {
        private readonly TaskTrackingDbContext _db;
private readonly IPermissionService _permissionService;

public PermissionsController(TaskTrackingDbContext db, IPermissionService permissionService)
{
    _db = db;
    _permissionService = permissionService;
}


        // GET: api/permissions
       [HttpGet]
public async Task<IActionResult> GetAll([FromHeader(Name = "x-user-id")] string userIdHeader)
{
    if (!int.TryParse(userIdHeader, out var userId))
        return BadRequest("Invalid x-user-id");

    var user = await _db.Users.FindAsync(userId);
    if (user == null) return Unauthorized();

  

    var permissions = await _db.Permissions
        .Where(p => p.IsActive)
        .OrderBy(p => p.Name)
        .ToListAsync();

    return Ok(permissions);
}

    }
}
