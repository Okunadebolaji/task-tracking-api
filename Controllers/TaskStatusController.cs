using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskTrackingApi.Models;
using TaskTrackingApi.Dtos;

[ApiController]
[Route("api/[controller]")]
public class TaskStatusesController : ControllerBase
{
    private readonly TaskTrackingDbContext _context;

    public TaskStatusesController(TaskTrackingDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetStatuses()
    {
        var statuses = await _context.TaskStatuses
            .Where(s => s.CompanyId == null) // GLOBAL
            .OrderBy(s => s.SortOrder)
            .Select(s => new
            {
                s.Id,
                s.Name,
                s.IsFinal,
                s.IsDefault
            })
            .ToListAsync();

        return Ok(statuses);
    }

    
}
