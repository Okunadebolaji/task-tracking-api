using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskTrackingApi.Dtos;
using TaskTrackingApi.Models;

namespace TaskTrackingApi.Controllers
{
    [ApiController]
    [Route("api/user-teams")]
    public class UserTeamsController : ControllerBase
    {
        private readonly TaskTrackingDbContext _db;

        public UserTeamsController(TaskTrackingDbContext db)
        {
            _db = db;
        }

        //  ADD USER TO TEAM
     [HttpPost]
public async Task<IActionResult> Add(
    CreateUserTeamDto dto,
    [FromHeader(Name = "x-company-id")] int companyId,
    [FromHeader(Name = "x-user-id")] int userId
)
{
    var user = await _db.Users
        .FirstOrDefaultAsync(u => u.Id == dto.UserId && u.CompanyId == companyId);

    if (user == null)
        return BadRequest("Invalid user");

    var team = await _db.Teams
        .FirstOrDefaultAsync(t => t.Id == dto.TeamId && t.CompanyId == companyId);

    if (team == null)
        return BadRequest("Invalid team");

    // ✅ ENFORCE MEMBER LIMIT
    var memberCount = await _db.UserTeams
        .CountAsync(ut => ut.TeamId == dto.TeamId);

    if (memberCount >= team.MaxMembers)
        return BadRequest($"Team member limit reached ({team.MaxMembers})");

    bool exists = await _db.UserTeams
        .AnyAsync(x => x.UserId == dto.UserId && x.TeamId == dto.TeamId);

    if (exists)
        return BadRequest("User already in team");

    _db.UserTeams.Add(new UserTeam
    {
        UserId = dto.UserId,
        TeamId = dto.TeamId
    });

    await _db.SaveChangesAsync();
    return Ok();
}


        // GET EACH TEAM FOR USER
        [HttpGet("by-user/{userId}")]
        public IActionResult GetByUser(int userId)
        {
            var data = _db.UserTeams
                .Where(x => x.UserId == userId)
                .Select(x => new UserTeamDto
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    UserEmail = x.User.Email,
                    TeamId = x.TeamId,
                    TeamName = x.Team.Name
                })
                .ToList();

            return Ok(data);
        }

        //  GET USERS IN A TEAM
       [HttpGet("by-team/{teamId}")]
public async Task<IActionResult> GetByTeam(int teamId, [FromHeader(Name = "x-company-id")] int companyId)
{
    var data = await _db.UserTeams
        .Where(x => x.TeamId == teamId && x.Team.CompanyId == companyId)
        .Select(x => new
        {
            userTeamId = x.Id,        // UserTeam ID for deletion
            userId = x.UserId,
            fullName = x.User.FirstName + " " + x.User.LastName,  // full name
            email = x.User.Email
        })
        .ToListAsync();

    return Ok(data);
}

        // REMOVE USER FROM TEAM
       [HttpDelete("{id}")]
public async Task<IActionResult> Remove(
    int id,
    [FromHeader(Name = "x-company-id")] int companyId
)
{
    var entity = await _db.UserTeams
        .Include(x => x.Team)
        .FirstOrDefaultAsync(x => x.Id == id && x.Team.CompanyId == companyId);

    if (entity == null)
        return NotFound();

    _db.UserTeams.Remove(entity);
    await _db.SaveChangesAsync();

    return Ok();
}

    }
}
