using Microsoft.AspNetCore.Mvc;
using TaskTrackingApi.Models;
using TaskTrackingApi.Dtos;
using Microsoft.EntityFrameworkCore;




[ApiController]
[Route("api/[controller]")]
public class TaskRequirementsController : ControllerBase
{
    private readonly TaskTrackingDbContext _db;
private readonly IPermissionService _permissionService;

public TaskRequirementsController(TaskTrackingDbContext db, IPermissionService permissionService)
{
    _db = db;
    _permissionService = permissionService;
}


   [HttpPost]
public async Task<IActionResult> Link(
    TaskRequirementDto dto,
    [FromHeader(Name = "x-user-id")] string userIdHeader)
{
    if (!int.TryParse(userIdHeader, out var userId))
        return BadRequest("Invalid x-user-id");

    var user = await _db.Users.FindAsync(userId);
    if (user == null) return Unauthorized();

    if (!await _permissionService.HasPermissionAsync(userId, "TASK_REQUIREMENTS_ADD"))
        return StatusCode(403, "Permission denied");

    if (_db.TaskRequirements.Any(tr =>
        tr.TaskId == dto.TaskId && tr.RequirementId == dto.RequirementId))
    {
        return BadRequest("Already linked");
    }

    var link = new TaskRequirement
    {
        TaskId = dto.TaskId,
        RequirementId = dto.RequirementId
    };

    _db.TaskRequirements.Add(link);
    await _db.SaveChangesAsync();

    return Ok();
}

    [HttpGet("by-task/{taskId}")]
public async Task<IActionResult> GetByTask(
    int taskId,
    [FromHeader(Name = "x-user-id")] string userIdHeader)
{
    if (!int.TryParse(userIdHeader, out var userId))
        return BadRequest("Invalid x-user-id");

    var user = await _db.Users.FindAsync(userId);
    if (user == null) return Unauthorized();

    if (!await _permissionService.HasPermissionAsync(userId, "TASK_REQUIREMENTS_VIEW"))
        return StatusCode(403, "Permission denied");

    var reqs = await _db.TaskRequirements
        .Where(tr => tr.TaskId == taskId)
        .Select(tr => tr.Requirement)
        .Select(r => new
        {
            r.Id,
            r.Module,
            r.RequirementText,
            r.Status
        })
        .ToListAsync();

    return Ok(reqs);
}

}
