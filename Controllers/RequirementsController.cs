using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskTrackingApi.Models;
using TaskTrackingApi.Dtos;

namespace TaskTrackingApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RequirementsController : ControllerBase
    {
        private readonly TaskTrackingDbContext _db;
        private readonly IPermissionService _permissionService;

        public RequirementsController(TaskTrackingDbContext db, IPermissionService permissionService)
        {
            _db = db;
            _permissionService = permissionService;
        }

        // 🔹 GET BY PROJECT
        [HttpGet("by-project/{projectId}")]
        public async Task<IActionResult> GetByProject(
            int projectId,
            [FromHeader(Name = "x-user-id")] string userIdHeader)
        {
            if (!int.TryParse(userIdHeader, out var userId))
                return BadRequest("Invalid x-user-id");

            var user = await _db.Users.FindAsync(userId);
            if (user == null) return Unauthorized();

            if (!await _permissionService.HasPermissionAsync(userId, "REQUIREMENTS_VIEW"))
                return StatusCode(403, "Permission denied");

            if (!await _db.Projects.AnyAsync(p => p.Id == projectId && p.CompanyId == user.CompanyId))
                return BadRequest("Invalid ProjectId");

            var data = await _db.Requirements
                .AsNoTracking()
                .Where(r => r.ProjectId == projectId)
                .Select(r => new RequirementDto
                {
                    Id = r.Id,
                    Module = r.Module,
                    Menu = r.Menu,
                    Requirement = r.RequirementText,
                    Category = r.Category,
                    Baseline = r.Baseline,
                    Status = r.Status,
                    ProjectId = r.ProjectId
                })
                .ToListAsync();

            return Ok(data);
        }

        // 🔹 CREATE
        [HttpPost]
        public async Task<IActionResult> Create(
            RequirementDto dto,
            [FromHeader(Name = "x-user-id")] string userIdHeader)
        {
            if (!int.TryParse(userIdHeader, out var userId))
                return BadRequest("Invalid x-user-id");

            var user = await _db.Users.FindAsync(userId);
            if (user == null) return Unauthorized();

            if (!await _permissionService.HasPermissionAsync(userId, "REQUIREMENTS_CREATE"))
                return StatusCode(403, "Permission denied");

            if (!await _db.Projects.AnyAsync(p => p.Id == dto.ProjectId && p.CompanyId == user.CompanyId))
                return BadRequest("Invalid ProjectId");

            var entity = new Requirement
            {
                Module = dto.Module,
                Menu = dto.Menu,
                RequirementText = dto.Requirement,
                Category = dto.Category,
                Baseline = dto.Baseline,
                Status = dto.Status,
                ProjectId = dto.ProjectId,
                CreatedByUserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _db.Requirements.Add(entity);
            await _db.SaveChangesAsync();

            return Ok(entity.Id);
        }

        // 🔹 UPDATE (NO PROJECT CHANGE)
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            int id,
            RequirementDto dto,
            [FromHeader(Name = "x-user-id")] string userIdHeader)
        {
            if (!int.TryParse(userIdHeader, out var userId))
                return BadRequest("Invalid x-user-id");

            var user = await _db.Users.FindAsync(userId);
            if (user == null) return Unauthorized();

            if (!await _permissionService.HasPermissionAsync(userId, "REQUIREMENTS_EDIT"))
                return StatusCode(403, "Permission denied");

            var req = await _db.Requirements.FindAsync(id);
            if (req == null) return NotFound();

            req.Module = dto.Module;
            req.Menu = dto.Menu;
            req.RequirementText = dto.Requirement;
            req.Category = dto.Category;
            req.Baseline = dto.Baseline;
            req.Status = dto.Status;

            await _db.SaveChangesAsync();
            return Ok();
        }

        // 🔹 DELETE
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(
            int id,
            [FromHeader(Name = "x-user-id")] string userIdHeader)
        {
            if (!int.TryParse(userIdHeader, out var userId))
                return BadRequest("Invalid x-user-id");

            var user = await _db.Users.FindAsync(userId);
            if (user == null) return Unauthorized();

            if (!await _permissionService.HasPermissionAsync(userId, "REQUIREMENTS_DELETE"))
                return StatusCode(403, "Permission denied");

            var req = await _db.Requirements.FindAsync(id);
            if (req == null) return NotFound();

            _db.Requirements.Remove(req);
            await _db.SaveChangesAsync();
            return Ok();
        }

        // 🔹 BULK CREATE
        [HttpPost("bulk")]
        public async Task<IActionResult> BulkCreate(
            List<RequirementDto> dtos,
            [FromHeader(Name = "x-user-id")] string userIdHeader)
        {
            if (!int.TryParse(userIdHeader, out var userId))
                return BadRequest("Invalid x-user-id");

            var user = await _db.Users.FindAsync(userId);
            if (user == null) return Unauthorized();

            if (!await _permissionService.HasPermissionAsync(userId, "REQUIREMENTS_CREATE"))
                return StatusCode(403, "Permission denied");

            var projectIds = dtos.Select(d => d.ProjectId).Distinct();

            if (!await _db.Projects.AnyAsync(p => projectIds.Contains(p.Id) && p.CompanyId == user.CompanyId))
                return BadRequest("One or more ProjectIds are invalid");

            var entities = dtos.Select(d => new Requirement
            {
                Module = d.Module,
                Menu = d.Menu,
                RequirementText = d.Requirement,
                Category = d.Category,
                Baseline = d.Baseline,
                Status = d.Status,
                ProjectId = d.ProjectId,
                CreatedByUserId = userId,
                CreatedAt = DateTime.UtcNow
            });

            _db.Requirements.AddRange(entities);
            await _db.SaveChangesAsync();

            return Ok();
        }
    }
}
