using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskTrackingApi.Dtos;
using TaskTrackingApi.Models;

namespace TaskTrackingApi.Controllers
{

    [ApiController]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;
    private readonly TaskTrackingDbContext _db;

    public DashboardController(
        IDashboardService dashboardService,
        TaskTrackingDbContext db)
    {
        _dashboardService = dashboardService;
        _db = db;
    }

    [HttpGet("counters")]
    public async Task<IActionResult> GetCounters(
        [FromHeader(Name = "x-user-id")] int userId)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user == null)
            return Unauthorized();

        var data = await _dashboardService.GetCountersAsync(user.CompanyId);

        return Ok(data);
    }


    [HttpGet("tasks/pending")]
public async Task<IActionResult> GetPendingTasks(
    [FromHeader(Name = "x-user-id")] int userId)
{
    return Ok(await GetTasksByStatusName(userId, "Pending"));
}

[HttpGet("tasks/wip")]
public async Task<IActionResult> GetWipTasks(
    [FromHeader(Name = "x-user-id")] int userId)
{
    return Ok(await GetTasksByStatusName(userId, "In Progress"));
}

[HttpGet("tasks/completed")]
public async Task<IActionResult> GetCompletedTasks(
    [FromHeader(Name = "x-user-id")] int userId)
{
    return Ok(await GetTasksByStatusName(userId, "Completed"));
}

[HttpGet("tasks/overdue")]
public async Task<IActionResult> GetOverdueTasks(
    [FromHeader(Name = "x-user-id")] int userId)
{
    var user = await _db.Users.FindAsync(userId);
    if (user == null) return Unauthorized();

    var tasks = await _db.Tasks
        .Include(t => t.Status)
        .Include(t => t.Project)
        .Include(t => t.Team)
        .Where(t =>
            t.CompanyId == user.CompanyId &&
            t.Status.Name != "Completed" &&
            t.Status.Name != "Rejected" &&
            t.EndDate < DateTime.UtcNow)
        .Select(t => new
        {
            t.Id,
            Title = t.Module,
            ProjectName = t.Project != null ? t.Project.Name : null,
            StatusName = t.Status.Name,
            TeamName = t.Team != null ? t.Team.Name : null,
            DueDate = t.EndDate
        })
        .ToListAsync();

    return Ok(tasks);
}



private async Task<List<object>> GetTasksByStatusName(int userId, string statusName)
{
    var user = await _db.Users.FindAsync(userId);
    if (user == null) throw new Exception("Unauthorized");

    return await _db.Tasks
        .Include(t => t.Status)
        .Include(t => t.Project)
        .Include(t => t.Team)
        .Where(t =>
            t.CompanyId == user.CompanyId &&
            t.Status.Name == statusName)
        .Select(t => new
        {
            t.Id,
            Title = t.Module,
            ProjectName = t.Project != null ? t.Project.Name : null,
            StatusName = t.Status.Name,
            TeamName = t.Team != null ? t.Team.Name : null,
            DueDate = t.EndDate
        })
        .ToListAsync<object>();
}


}

}