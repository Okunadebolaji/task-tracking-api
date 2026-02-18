using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskTrackingApi.Models;
using TaskTrackingApi.Dtos;

namespace TaskTrackingApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectsController : ControllerBase
    {
        private readonly TaskTrackingDbContext _context;

        public ProjectsController(TaskTrackingDbContext context)
        {
            _context = context;
        }

        // GET: api/projects
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProjectDto>>> GetProjects(
            [FromHeader(Name = "x-company-id")] string companyIdHeader,
            [FromHeader(Name = "x-user-id")] string userIdHeader)
        {
            if (!int.TryParse(userIdHeader, out var userId))
                return BadRequest("Invalid x-user-id");

            if (!int.TryParse(companyIdHeader, out var companyId))
                return BadRequest("Invalid x-company-id");

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return Unauthorized();

            var projects = await _context.Projects
                .Where(p => p.CompanyId == companyId)
                .Include(p => p.Tasks)
                    .ThenInclude(t => t.Status)
                .ToListAsync();

            var dtos = projects.Select(p => new ProjectDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description ?? string.Empty,
                TaskCount = p.Tasks.Count,
                CompletedCount = p.Tasks.Count(t => t.Status?.Name == "Completed"),
                PendingCount = p.Tasks.Count(t => t.Status?.Name == "Pending"),
                RejectedCount = p.Tasks.Count(t => t.IsRejected)
            }).ToList();

            return Ok(dtos);
        }

        // GET: api/projects/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ProjectDto>> GetProject(
            int id,
            [FromHeader(Name = "x-company-id")] string companyIdHeader,
            [FromHeader(Name = "x-user-id")] string userIdHeader)
        {
            if (!int.TryParse(userIdHeader, out var userId))
                return BadRequest("Invalid x-user-id");

            if (!int.TryParse(companyIdHeader, out var companyId))
                return BadRequest("Invalid x-company-id");

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return Unauthorized();

            var project = await _context.Projects
                .Where(p => p.Id == id && p.CompanyId == companyId)
                .Include(p => p.Tasks)
                    .ThenInclude(t => t.Status)
                .FirstOrDefaultAsync();

            if (project == null) return NotFound();

            return Ok(new ProjectDto
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description ?? string.Empty,
                TaskCount = project.Tasks.Count,
                CompletedCount = project.Tasks.Count(t => t.Status?.Name == "Completed"),
                PendingCount = project.Tasks.Count(t => t.Status?.Name == "Pending"),
                RejectedCount = project.Tasks.Count(t => t.IsRejected)
            });
        }

        // POST: api/projects
        [HttpPost]
        public async Task<ActionResult<ProjectDto>> CreateProject(
            CreateUpdateProjectDto dto,
            [FromHeader(Name = "x-user-id")] string userIdHeader)
        {
            if (!int.TryParse(userIdHeader, out var userId))
                return BadRequest("Invalid x-user-id");

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return Unauthorized();

            var project = new Project
            {
                Name = dto.Name,
                Description = dto.Description,
                CompanyId = dto.CompanyId
            };

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetProject),
                new { id = project.Id },
                new ProjectDto
                {
                    Id = project.Id,
                    Name = project.Name,
                    Description = project.Description ?? string.Empty,
                    TaskCount = 0,
                    CompletedCount = 0,
                    PendingCount = 0,
                    RejectedCount = 0
                });
        }

        // PUT: api/projects/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProject(
            int id,
            CreateUpdateProjectDto dto,
            [FromHeader(Name = "x-user-id")] string userIdHeader)
        {
            if (!int.TryParse(userIdHeader, out var userId))
                return BadRequest("Invalid x-user-id");

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return Unauthorized();

            var project = await _context.Projects.FindAsync(id);
            if (project == null) return NotFound();

            project.Name = dto.Name;
            project.Description = dto.Description;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/projects/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(
            int id,
            [FromHeader(Name = "x-user-id")] string userIdHeader)
        {
            if (!int.TryParse(userIdHeader, out var userId))
                return BadRequest("Invalid x-user-id");

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return Unauthorized();

            var project = await _context.Projects.FindAsync(id);
            if (project == null) return NotFound();

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
