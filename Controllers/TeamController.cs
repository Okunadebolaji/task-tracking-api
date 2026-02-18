using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskTrackingApi.Models;
using TaskTrackingApi.Dtos;

[ApiController]
[Route("api/[controller]")]
public class TeamsController : ControllerBase
{
    private readonly TaskTrackingDbContext _db;
private readonly IPermissionService _permissionService;

public TeamsController(TaskTrackingDbContext db, IPermissionService permissionService)
{
    _db = db;
    _permissionService = permissionService;
}


    // ============================================================
    // GET ALL TEAMS BY COMPANY
    // ============================================================
   [HttpGet]
public async Task<IActionResult> GetAll(
    [FromHeader(Name = "x-company-id")] int companyId,
    [FromHeader(Name = "x-user-id")] string userIdHeader)
{
    if (!int.TryParse(userIdHeader, out var userId))
        return BadRequest("Invalid x-user-id");

    var user = await _db.Users.FindAsync(userId);
    if (user == null) return Unauthorized();

    if (!await _permissionService.HasPermissionAsync(userId, "TEAMS_VIEW"))
        return StatusCode(403, "Permission denied");

    var teams = await _db.Teams
        .Where(t => t.CompanyId == companyId)
        .Select(t => new TeamDto
        {
            Id = t.Id,
            Name = t.Name,
            ProjectId = t.ProjectId,
            IsActive = t.IsActive,
            MaxMembers = t.MaxMembers
        })
        .ToListAsync();

    return Ok(teams);
}


    // ============================================================
    // GET TEAMS BY PROJECT
    // ============================================================
    [HttpGet("by-project/{projectId}")]
public async Task<IActionResult> GetByProject(
    int projectId,
    [FromHeader(Name = "x-company-id")] int companyId,
    [FromHeader(Name = "x-user-id")] string userIdHeader)
{
    if (!int.TryParse(userIdHeader, out var userId))
        return BadRequest("Invalid x-user-id");

    var user = await _db.Users.FindAsync(userId);
    if (user == null) return Unauthorized();

    if (!await _permissionService.HasPermissionAsync(userId, "TEAMS_VIEW"))
        return StatusCode(403, "Permission denied");

    var teams = await _db.Teams
        .Where(t => t.ProjectId == projectId && t.CompanyId == companyId)
        .Select(t => new TeamDto
        {
            Id = t.Id,
            Name = t.Name,
            ProjectId = t.ProjectId,
            IsActive = t.IsActive,
            MaxMembers = t.MaxMembers
        })
        .ToListAsync();

    return Ok(teams);
}


    // ============================================================
    // GET TEAM MEMBERS (USERS IN A TEAM)
    // ============================================================
  [HttpGet("{teamId}/members")]
public async Task<IActionResult> GetTeamMembers(
    int teamId,
    [FromHeader(Name = "x-company-id")] int companyId,
    [FromHeader(Name = "x-user-id")] string userIdHeader)
{
    if (!int.TryParse(userIdHeader, out var userId))
        return BadRequest("Invalid x-user-id");

    var user = await _db.Users.FindAsync(userId);
    if (user == null) return Unauthorized();

    if (!await _permissionService.HasPermissionAsync(userId, "TEAMS_MEMBERS_VIEW"))
        return StatusCode(403, "Permission denied");

    var teamExists = await _db.Teams.AnyAsync(t => t.Id == teamId && t.CompanyId == companyId);
    if (!teamExists)
        return Unauthorized("Team does not belong to your company");

    var users = await _db.UserTeams
        .Where(ut => ut.TeamId == teamId && ut.User.CompanyId == companyId)
        .Select(ut => new { ut.User.Id, ut.User.FullName, ut.User.Email })
        .ToListAsync();

    return Ok(users);
}


    // ============================================================
    // CREATE TEAM (NO PROJECT ASSIGNMENT HERE)
    // ============================================================
  [HttpPost]
public async Task<IActionResult> Create(
    TeamDto dto,
    [FromHeader(Name = "x-company-id")] int companyId,
    [FromHeader(Name = "x-user-id")] string userIdHeader)
{
    if (!int.TryParse(userIdHeader, out var userId))
        return BadRequest("Invalid x-user-id");

    var user = await _db.Users.FindAsync(userId);
    if (user == null) return Unauthorized();

    if (!await _permissionService.HasPermissionAsync(userId, "TEAMS_ADD"))
        return StatusCode(403, "Permission denied");

    var exists = await _db.Teams.AnyAsync(t =>
        t.CompanyId == companyId && t.Name.ToLower() == dto.Name.ToLower());

    if (exists)
        return BadRequest("A team with this name already exists");

    if (dto.MaxMembers < 5 || dto.MaxMembers > 10)
        return BadRequest("Team size must be between 5 and 10 members");

    var team = new Team
    {
        Name = dto.Name,
        CompanyId = companyId,
        IsActive = true,
        ProjectId = null,
        CreatedAt = DateTime.UtcNow,
        MaxMembers = dto.MaxMembers
    };

    _db.Teams.Add(team);
    await _db.SaveChangesAsync();

    return Ok(team.Id);
}


    // ============================================================
    // UPDATE TEAM METADATA ONLY (NO PROJECT ASSIGNMENT)
    // ============================================================
 [HttpPut("{id}")]
public async Task<IActionResult> UpdateTeam(
    int id,
    TeamDto dto,
    [FromHeader(Name = "x-company-id")] int companyId,
    [FromHeader(Name = "x-user-id")] string userIdHeader)
{
    if (!int.TryParse(userIdHeader, out var userId))
        return BadRequest("Invalid x-user-id");

    var user = await _db.Users.FindAsync(userId);
    if (user == null) return Unauthorized();

    if (!await _permissionService.HasPermissionAsync(userId, "TEAMS_EDIT"))
        return StatusCode(403, "Permission denied");

    var team = await _db.Teams.FirstOrDefaultAsync(t => t.Id == id && t.CompanyId == companyId);
    if (team == null) return NotFound("Team not found");

    var currentCount = await _db.UserTeams.CountAsync(x => x.TeamId == id);
    if (dto.MaxMembers < currentCount)
        return BadRequest($"Cannot reduce limit below current members ({currentCount})");

    if (dto.MaxMembers < 5 || dto.MaxMembers > 10)
        return BadRequest("Team size must be between 5 and 10 members");

    team.Name = dto.Name;
    team.IsActive = dto.IsActive;
    team.MaxMembers = dto.MaxMembers;

    await _db.SaveChangesAsync();
    return NoContent();
}


    // ============================================================
    // ASSIGN PROJECT TO TEAM (EXPLICIT RESPONSIBILITY)
    // ============================================================
    [HttpPut("{teamId}/assign-project")]
public async Task<IActionResult> AssignProjectToTeam(
    int teamId,
    [FromBody] AssignTeamToProjectDto dto,
    [FromHeader(Name = "x-company-id")] int companyId,
    [FromHeader(Name = "x-user-id")] string userIdHeader)
{
    if (!int.TryParse(userIdHeader, out var userId))
        return BadRequest("Invalid x-user-id");

    var user = await _db.Users.FindAsync(userId);
    if (user == null) return Unauthorized();

    if (!await _permissionService.HasPermissionAsync(userId, "TEAMS_EDIT"))
        return StatusCode(403, "Permission denied");

    var team = await _db.Teams.FirstOrDefaultAsync(t => t.Id == teamId && t.CompanyId == companyId);
    if (team == null) return NotFound("Team not found");

    var projectExists = await _db.Projects.AnyAsync(p => p.Id == dto.ProjectId && p.CompanyId == companyId);
    if (!projectExists) return BadRequest("Invalid project");

    team.ProjectId = dto.ProjectId;
    await _db.SaveChangesAsync();

    return Ok();
}

    // ============================================================
    // DELETE TEAM
    // ============================================================
  [HttpDelete("{id}")]
public async Task<IActionResult> Delete(
    int id,
    [FromHeader(Name = "x-company-id")] int companyId,
    [FromHeader(Name = "x-user-id")] string userIdHeader)
{
    if (!int.TryParse(userIdHeader, out var userId))
        return BadRequest("Invalid x-user-id");

    var user = await _db.Users.FindAsync(userId);
    if (user == null) return Unauthorized();

    if (!await _permissionService.HasPermissionAsync(userId, "TEAMS_DELETE"))
        return StatusCode(403, "Permission denied");

    var team = await _db.Teams.FirstOrDefaultAsync(t => t.Id == id && t.CompanyId == companyId);
    if (team == null) return NotFound();

    var userTeamCount = await _db.UserTeams.CountAsync(ut => ut.TeamId == id);
    var taskCount = await _db.Tasks.CountAsync(ts => ts.TeamId == id);

    if (userTeamCount > 0 || taskCount > 0)
    {
        var parts = new List<string>();
        if (userTeamCount > 0) parts.Add($"members ({userTeamCount})");
        if (taskCount > 0) parts.Add($"tasks ({taskCount})");

        var details = string.Join(", ", parts);
        return BadRequest($"Cannot delete team because related records exist: {details}. Remove or reassign them first.");
    }

    _db.Teams.Remove(team);
    await _db.SaveChangesAsync();
    return NoContent();
}


}
