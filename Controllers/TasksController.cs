using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskTrackingApi.Dtos;
using TaskTrackingApi.Models;
using TaskEntity = TaskTrackingApi.Models.Task;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly TaskTrackingDbContext _db;
    private readonly IPermissionService _permissionService;

    public TasksController(TaskTrackingDbContext db, IPermissionService permissionService)
    {
        _db = db;
        _permissionService = permissionService;
    }

    // ================= GET ALL TASKS =================
    [HttpGet]
    public async Task<IActionResult> GetTasks(
        [FromHeader(Name = "x-user-id")] string userIdHeader,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] int? projectId = null,
        [FromQuery] int? statusId = null,
        [FromQuery] int? teamId = null)
    {
        if (!int.TryParse(userIdHeader, out var userId))
            return BadRequest("Invalid x-user-id");

        var user = await _db.Users.FindAsync(userId);
        if (user == null) return Unauthorized();

       

        var query = _db.Tasks
            .Include(t => t.Project)
            .Include(t => t.Team)
            .Include(t => t.Status)
            .Include(t => t.CreatedByUser)
            .Include(t => t.TaskAssignments).ThenInclude(ta => ta.User)
            .Include(t => t.TaskRequirements).ThenInclude(tr => tr.Requirement)
            .Where(t => t.CompanyId == user.CompanyId)
            .AsQueryable();

        if (projectId.HasValue) query = query.Where(t => t.ProjectId == projectId);
        if (statusId.HasValue) query = query.Where(t => t.StatusId == statusId);
        if (teamId.HasValue) query = query.Where(t => t.TeamId == teamId);

        var totalCount = await query.CountAsync();

        var tasks = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new
            {
                t.Id,
                t.Module,
                t.Description,
                t.References,
                t.UserStory,   // ✅ include here
                t.StartDate,
                t.EndDate,
                t.Comment,
                t.Source,
                t.CreatedAt,
                t.IsApproved,
                t.IsRejected,
                Status = new { t.Status.Id, t.Status.Name },
                Project = t.Project == null ? null : new { t.Project.Id, t.Project.Name },
                Team = t.Team == null ? null : new { t.Team.Id, t.Team.Name },
                AssignedUsers = t.TaskAssignments.Select(ta => new { ta.User.Id, ta.User.FullName }),
                Requirements = t.TaskRequirements.Select(tr => new { tr.RequirementId, tr.Requirement.RequirementText })
            })
            .ToListAsync();

        return Ok(new
        {
            data = tasks,
            page,
            pageSize,
            totalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            totalCount
        });
    }

    // ================= GET TASK BY ID =================
  [HttpGet("{id}")]
public async Task<IActionResult> GetTaskById(
    int id,
    [FromHeader(Name = "x-user-id")] int userId)
{
    var user = await _db.Users.FindAsync(userId);
    if (user == null) return Unauthorized();

    var task = await _db.Tasks
        .Include(t => t.Status)
        .Include(t => t.Project)
        .Include(t => t.Team)
        .Include(t => t.CreatedByUser)
        .Include(t => t.TaskAssignments)
            .ThenInclude(ta => ta.User)
        .Include(t => t.TaskRequirements)
            .ThenInclude(tr => tr.Requirement)
        .FirstOrDefaultAsync(t =>
            t.Id == id &&
            t.CompanyId == user.CompanyId
        );

    if (task == null) return NotFound();

    return Ok(new
    {
        task.Id,
        task.Module,
        task.Description,
        task.References,
        task.UserStory,
        task.StartDate,
        task.EndDate,
        task.Comment,
        task.Source,

        Status = new {
            task.Status.Id,
            task.Status.Name
        },

        Project = new {
            task.Project.Id,
            task.Project.Name
        },

        Team = task.Team == null ? null : new {
            task.Team.Id,
            task.Team.Name
        },

        CreatedBy = new {
            task.CreatedByUser.Id,
            task.CreatedByUser.FullName
        },

        AssignedUsers = task.TaskAssignments.Select(ta => new {
            ta.User.Id,
            ta.User.FullName
        }),

        Requirements = task.TaskRequirements.Select(tr => new {
            tr.RequirementId,
            tr.Requirement.RequirementText
        })
    });
}


    // ================= CREATE TASK =================
  [HttpPost]
public async Task<IActionResult> CreateTask(
    [FromBody] CreateTaskDto dto,
    [FromHeader(Name = "x-user-id")] string userIdHeader)
{
    if (!int.TryParse(userIdHeader, out var userId))
        return BadRequest("Invalid x-user-id");

    var user = await _db.Users.FindAsync(userId);
    if (user == null) return Unauthorized();

    var statusId = dto.StatusId;
    if (statusId == null || statusId == 0)
    {
        statusId = await _db.TaskStatuses
            .Where(s => s.IsDefault)
            .Select(s => s.Id)
            .FirstOrDefaultAsync();

        if (statusId == 0)
            return BadRequest("No default task status configured");
    }

    // 1. Create the Task object
    var task = new TaskEntity
    {
        Module = dto.Module!,
        Description = dto.Description!,
        References = dto.References,
        UserStory = dto.UserStory,
        StartDate = DateTime.SpecifyKind(dto.StartDate, DateTimeKind.Utc),
        EndDate = DateTime.SpecifyKind(dto.EndDate, DateTimeKind.Utc),
        Comment = dto.Comment,
        Source = dto.Source ?? "Manual",
        ProjectId = dto.ProjectId,
        TeamId = dto.TeamId,
        CreatedByUserId = userId,
        CompanyId = user.CompanyId,
        StatusId = statusId.Value,
        CreatedAt = DateTime.UtcNow,
        IsApproved = false,
        IsRejected = false
    };

    // 2. Add the Assignments BEFORE calling SaveChanges
    if (dto.AssignedUserIds != null && dto.AssignedUserIds.Any())
    {
        var validUserIds = await _db.Users
            .Where(u => dto.AssignedUserIds.Distinct().Contains(u.Id) && u.CompanyId == user.CompanyId)
            .Select(u => u.Id)
            .ToListAsync();

        foreach (var assignedUserId in validUserIds)
        {
            // By adding to the task's Navigation Property, 
            // EF will automatically link the TaskId during save.
            task.TaskAssignments.Add(new TaskAssignment 
            { 
                UserId = assignedUserId,
                AssignedAt = DateTime.UtcNow // Ensure this date is also set
            });
        }
    }

    // 3. Save everything once
    _db.Tasks.Add(task);
    await _db.SaveChangesAsync();

    return Ok(new { taskId = task.Id });
}
    // ================= UPDATE TASK =================
   [HttpPut("{id}")]
public async Task<IActionResult> UpdateTask(
    int id,
    [FromBody] UpdateTaskDto dto,
    [FromHeader(Name = "x-company-id")] int companyId,
    [FromHeader(Name = "x-user-id")] int userId)
{
    var task = await _db.Tasks
        .Include(t => t.CreatedByUser)
        .Include(t => t.TaskRequirements)
        .Include(t => t.TaskAssignments)
        .FirstOrDefaultAsync(t => t.Id == id && t.CompanyId == companyId);

    if (task == null)
        return NotFound("Task not found");

    // ===== Update scalar fields =====
    task.Module = dto.Module!;
    task.Description = dto.Description!;
    task.References = dto.References;
    task.UserStory = dto.UserStory;
    task.Comment = dto.Comment;
    task.StartDate = dto.StartDate;
    task.EndDate = dto.EndDate;
    task.Source = dto.Source!;
    task.StatusId = dto.StatusId;
    task.ProjectId = dto.ProjectId;
    task.TeamId = dto.TeamId;

    // ===== Update requirements safely =====
    task.TaskRequirements.Clear();
    if (dto.RequirementIds != null && dto.RequirementIds.Any())
    {
        foreach (var reqId in dto.RequirementIds)
        {
            task.TaskRequirements.Add(new TaskRequirement
            {
                TaskId = id,
                RequirementId = reqId
            });
        }
    }

    // ===== Update assigned users safely =====
    task.TaskAssignments.Clear();

    if (dto.AssignedUserIds != null && dto.AssignedUserIds.Any())
    {
        var validUserIds = await _db.Users
            .Where(u =>
                dto.AssignedUserIds.Contains(u.Id) &&
                u.CompanyId == companyId
            )
            .Select(u => u.Id)
            .ToListAsync();

        foreach (var validUserId in validUserIds)
        {
            task.TaskAssignments.Add(new TaskAssignment
            {
                TaskId = id,
                UserId = validUserId
            });
        }
    }

    await _db.SaveChangesAsync();

    return Ok(new
    {
        task.Id,
        task.Module,
        task.Description,
        task.UserStory,
        task.StatusId,
        task.ProjectId,
        task.TeamId,
        AssignedUserIds = task.TaskAssignments.Select(ta => ta.UserId),
        RequirementIds = task.TaskRequirements.Select(tr => tr.RequirementId)
    });
}

    


    // POST: api/tasks/{taskId}/assign-team
[HttpPost("{taskId}/assign-team")]
public async Task<IActionResult> AssignTaskToTeam(
    int taskId,
    [FromBody] int teamId,
    [FromHeader(Name = "x-user-id")] int userId)
{
    // Validate user
    var user = await _db.Users.FindAsync(userId);
    if (user == null) return Unauthorized();

    // Validate task
    var task = await _db.Tasks.FirstOrDefaultAsync(t => t.Id == taskId && t.CompanyId == user.CompanyId);
    if (task == null) return NotFound("Task not found");

    // Validate team
    var team = await _db.Teams.FirstOrDefaultAsync(t => t.Id == teamId && t.CompanyId == user.CompanyId);
    if (team == null) return BadRequest("Invalid team");

    // Assign task to team
    task.TeamId = teamId;
    await _db.SaveChangesAsync();

    return Ok(new { task.Id, task.TeamId });
}


    // ================= APPROVE =================
    [HttpPut("{id}/approve")]
    public async Task<IActionResult> Approve(
        int id,
        [FromHeader(Name = "x-user-id")] string userIdHeader)
    {
        if (!int.TryParse(userIdHeader, out var userId))
            return BadRequest();

       
        var task = await _db.Tasks.FindAsync(id);
        if (task == null) return NotFound();

        task.IsApproved = true;
        task.IsRejected = false;
        task.ApprovedByUserId = userId;
        task.ApprovedDate = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Ok();
    }

    // ================= REJECT =================
    [HttpPut("{id}/reject")]
    public async Task<IActionResult> Reject(
        int id,
        [FromHeader(Name = "x-user-id")] string userIdHeader)
    {
        if (!int.TryParse(userIdHeader, out var userId))
            return BadRequest();

        var task = await _db.Tasks.FindAsync(id);
        if (task == null) return NotFound();

        task.IsRejected = true;
        task.IsApproved = false;
        task.RejectedByUserId = userId;
        task.RejectedDate = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Ok();
    }

    // ================= DELETE =================
   [HttpDelete("{id}")]
public async Task<IActionResult> Delete(
    int id,
    [FromHeader(Name = "x-user-id")] string userIdHeader)
{
    if (!int.TryParse(userIdHeader, out var userId))
        return BadRequest("Invalid x-user-id");

    var user = await _db.Users.FindAsync(userId);
    if (user == null) return Unauthorized();

    var task = await _db.Tasks
        .Include(t => t.TaskAssignments)
        .Include(t => t.TaskRequirements)
        .FirstOrDefaultAsync(t =>
            t.Id == id &&
            t.CompanyId == user.CompanyId
        );

    if (task == null)
        return NotFound("Task not found");

    // 🔥 Delete children first
    _db.TaskAssignments.RemoveRange(task.TaskAssignments);
    _db.TaskRequirements.RemoveRange(task.TaskRequirements);

    _db.Tasks.Remove(task);

    await _db.SaveChangesAsync();

    return NoContent();
}


    [HttpGet("pending")]
public async Task<IActionResult> GetPendingTasks(
    [FromHeader(Name = "x-user-id")] int userId)
{
    var user = await _db.Users.FindAsync(userId);
    if (user == null) return Unauthorized();

    var tasks = await _db.Tasks
        .Include(t => t.Project)
        .Include(t => t.Team)
        .Include(t => t.Status)
        .Where(t =>
            t.CompanyId == user.CompanyId &&
            t.Status.Name == "Pending")
        .Select(t => new {
            t.Id,
            t.Module,
            t.Description,
            ProjectName = t.Project!.Name,
            TeamName = t.Team!.Name,
            StatusName = t.Status.Name,
            t.EndDate
        })
        .ToListAsync();

    return Ok(tasks);
}

     [HttpGet("wip")]
public async Task<IActionResult> GetWipTasks(
    [FromHeader(Name = "x-user-id")] int userId)
{
    var user = await _db.Users.FindAsync(userId);
    if (user == null) return Unauthorized();

    var tasks = await _db.Tasks
        .Include(t => t.Project)
        .Include(t => t.Team)
        .Include(t => t.Status)
        .Where(t =>
            t.CompanyId == user.CompanyId &&
            t.Status.Name == "Work In Progress")
        .Select(t => new {
            t.Id,
            t.Module,
            t.Description,
            ProjectName = t.Project!.Name,
            TeamName = t.Team!.Name,
            StatusName = t.Status.Name,
            t.EndDate
        })
        .ToListAsync();

    return Ok(tasks);
}



    [HttpGet("completed")]
public async Task<IActionResult> GetCompletedTasks(
    [FromHeader(Name = "x-user-id")] int userId)
{
    var user = await _db.Users.FindAsync(userId);
    if (user == null) return Unauthorized();

    var tasks = await _db.Tasks
        .Include(t => t.Project)
        .Include(t => t.Team)
        .Include(t => t.Status)
        .Where(t =>
            t.CompanyId == user.CompanyId &&
            t.IsApproved)
        .Select(t => new {
            t.Id,
            t.Module,
            t.Description,
            ProjectName = t.Project!.Name,
            TeamName = t.Team!.Name,
            StatusName = "Completed",
            t.EndDate
        })
        .ToListAsync();

    return Ok(tasks);
}

 [HttpGet("my-recent")]
public async Task<IActionResult> GetMyRecentTasks(
    [FromHeader(Name = "x-user-id")] int userId,
    [FromHeader(Name = "x-company-id")] int companyId)
{
    var tasks = await _db.Tasks
        .Include(t => t.Status)
        .Where(t =>
            t.CompanyId == companyId &&
            t.TaskAssignments.Any(ta => ta.UserId == userId)
        )
        .OrderByDescending(t => t.CreatedAt)
        .Take(5)
        .Select(t => new
        {
            t.Id,
            t.Module,
            t.Description,
            Status = t.Status.Name,
            t.EndDate,
            t.CreatedAt
        })
        .ToListAsync();

    return Ok(tasks);
}

// [HttpPut("{id}")]
// public async Task<IActionResult> UpdateTask(
//     int id,
//     [FromBody] UpdateTaskDto dto,
//     [FromHeader(Name = "x-company-id")] int companyId,
//     [FromHeader(Name = "x-user-id")] int userId
// )
// {
//     var task = await _db.Tasks
//         .Include(t => t.TaskRequirements)
//         .Include(t => t.TaskAssignments)
//         .FirstOrDefaultAsync(t => t.Id == id && t.CompanyId == companyId);

//     if (task == null)
//         return NotFound("Task not found");

//     // Update scalar fields
//     task.Module = dto.Module!;
//     task.Description = dto.Description!;
//     task.References = dto.References;
//     task.Comment = dto.Comment;
//     task.StartDate = dto.StartDate;
//     task.EndDate = dto.EndDate;
//     task.Source = dto.Source!;
//     task.StatusId = dto.StatusId;
//     task.ProjectId = dto.ProjectId;
//     task.TeamId = dto.TeamId;

//     // Update requirements
//     task.TaskRequirements.Clear();
//     foreach (var reqId in dto.RequirementIds)
//     {
//         task.TaskRequirements.Add(new TaskRequirement
//         {
//             TaskId = id,
//             RequirementId = reqId
//         });
//     }

//     // Update assigned users
//     task.TaskAssignments.Clear();
//     foreach (var userIdAssigned in dto.AssignedUserIds)
//     {
//         task.TaskAssignments.Add(new TaskAssignment
//         {
//             TaskId = id,
//             UserId = userIdAssigned
//         });
//     }

//     await _db.SaveChangesAsync();

//     return Ok(new
//     {
//         task.Id,
//         task.Module,
//         task.Description,
//         task.StatusId,
//         task.ProjectId,
//         task.TeamId,
//         Requirements = task.TaskRequirements.Select(tr => tr.RequirementId),
//         AssignedUsers = task.TaskAssignments.Select(ta => ta.UserId)
//     });
// }



[HttpPatch("{id}/status")]
public async Task<IActionResult> UpdateStatus(
    int id,
    [FromBody] UpdateTaskStatusDto dto,
    [FromHeader(Name = "x-user-id")] string userIdHeader)
{
    if (!int.TryParse(userIdHeader, out var userId))
        return BadRequest("Invalid x-user-id");

    if (id != dto.TaskId)
        return BadRequest("Task id mismatch.");

  
    var user = await _db.Users.FindAsync(userId);
    if (user == null) return Unauthorized();

    var task = await _db.Tasks
        .Include(t => t.Status)
        .FirstOrDefaultAsync(t => t.Id == id && t.CompanyId == user.CompanyId);

    if (task == null) return NotFound("Task not found");

    var newStatus = await _db.TaskStatuses
        .FirstOrDefaultAsync(s => s.Id == dto.NewStatusId && (s.CompanyId == null || s.CompanyId == user.CompanyId));

    if (newStatus == null) return BadRequest("Invalid status");

    // Prevent changing from a final status
    if (task.Status != null && task.Status.IsFinal)
        return BadRequest($"Cannot change status from final status '{task.Status.Name}'.");

    // Prevent no-op
    if (task.StatusId == dto.NewStatusId)
        return BadRequest("Task already in the requested status.");

    task.StatusId = dto.NewStatusId;
    // If you later add UpdatedAt, set it here: task.UpdatedAt = DateTime.UtcNow;

    await _db.SaveChangesAsync();

    return Ok(new
    {
        task.Id,
        Status = new { Id = newStatus.Id, Name = newStatus.Name }
    });
}


[HttpGet("overdue")]
public async Task<IActionResult> GetOverdueTasks(
    [FromHeader(Name = "x-user-id")] int userId)
{
    var user = await _db.Users.FindAsync(userId);
    if (user == null) return Unauthorized();

    var today = DateTime.UtcNow.Date;

    var tasks = await _db.Tasks
        .Include(t => t.Project)
        .Include(t => t.Team)
        .Include(t => t.Status)
        .Where(t =>
            t.CompanyId == user.CompanyId &&
            t.EndDate < today &&
            !t.IsApproved)
        .Select(t => new {
            t.Id,
            t.Module,
            t.Description,
            ProjectName = t.Project!.Name,
            TeamName = t.Team!.Name,
            StatusName = "Overdue",
            t.EndDate
        })
        .ToListAsync();

    return Ok(tasks);
}



}
