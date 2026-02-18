using Microsoft.AspNetCore.Mvc;
using TaskTrackingApi.Models;
using TaskTrackingApi.Dtos;
using Microsoft.EntityFrameworkCore;



[ApiController]
[Route("api/[controller]")]
public class ProjectUsersController : ControllerBase
{
    private readonly TaskTrackingDbContext _db;

private readonly IPermissionService _permissionService;

public ProjectUsersController(TaskTrackingDbContext db, IPermissionService permissionService)
{
    _db = db;
    _permissionService = permissionService;
}


   [HttpPost]
public async Task<IActionResult> Assign(
    ProjectUserDto dto,
    [FromHeader(Name = "x-user-id")] string userIdHeader)
{
    if (!int.TryParse(userIdHeader, out var userId))
        return BadRequest("Invalid x-user-id");

    var user = await _db.Users.FindAsync(userId);
    if (user == null) return Unauthorized();

    if (!await _permissionService.HasPermissionAsync(userId, "PROJECT_USERS_ADD"))
        return StatusCode(403, "Permission denied");

    if (_db.ProjectUsers.Any(pu => pu.ProjectId == dto.ProjectId && pu.UserId == dto.UserId))
        return BadRequest("User already assigned to project");

    var pu = new ProjectUser
    {
        ProjectId = dto.ProjectId,
        UserId = dto.UserId
    };

    _db.ProjectUsers.Add(pu);
    await _db.SaveChangesAsync();

    return Ok();
}


   [HttpGet("{projectId}")]
public async Task<IActionResult> GetUsers(
    int projectId,
    [FromHeader(Name = "x-user-id")] string userIdHeader)
{
    if (!int.TryParse(userIdHeader, out var userId))
        return BadRequest("Invalid x-user-id");

    var user = await _db.Users.FindAsync(userId);
    if (user == null) return Unauthorized();

    if (!await _permissionService.HasPermissionAsync(userId, "PROJECT_USERS_VIEW"))
        return StatusCode(403, "Permission denied");

    var users = await _db.ProjectUsers
        .Where(pu => pu.ProjectId == projectId)
        .Select(pu => pu.User)
        .Select(u => new
        {
            u.Id,
            u.FullName,
            u.Email
        })
        .ToListAsync();

    return Ok(users);
}

}




