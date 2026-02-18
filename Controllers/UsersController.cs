using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskTrackingApi.Models;
using TaskTrackingApi.Dtos;
using BCrypt.Net;

namespace TaskTrackingApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly TaskTrackingDbContext _context;

      private readonly IPermissionService _permissionService;

public UsersController(TaskTrackingDbContext context, IPermissionService permissionService)
{
    _context = context;
    _permissionService = permissionService;
}


        // ===================== GET ALL (BY COMPANY) =====================
       [HttpGet]
public async Task<IActionResult> GetUsers(
    [FromHeader(Name = "x-company-id")] int companyId,
    [FromHeader(Name = "x-user-id")] int userId
)
{
    // 🔐 permission check
   var canView = await _permissionService.HasPermissionAsync(userId, "USERS_VIEW");
var canCreate = await _permissionService.HasPermissionAsync(userId, "USERS_CREATE");
var canEdit = await _permissionService.HasPermissionAsync(userId, "USERS_EDIT");
var canDelete = await _permissionService.HasPermissionAsync(userId, "USERS_DELETE");


    if (!canView)
        return StatusCode(403, "Permission denied");

    var users = await _context.Users
        .Include(u => u.Role)
        .Where(u => u.CompanyId == companyId)
        .Select(u => new
        {
            u.Id,
            u.Email,
            u.FullName,
            u.IsActive,
            role = new
            {
                u.Role!.Id,
                u.Role!.Name
            }
        })
        .ToListAsync();

    return Ok(new
    {
        permissions = new
        {
            canView,
            canCreate,
            canEdit,
            canDelete
        },
        data = users
    });
}


// By company

[HttpGet("by-company")]
public async Task<IActionResult> GetUsersByCompany(
    [FromHeader(Name = "x-company-id")] int companyId,
    [FromHeader(Name = "x-user-id")] int userId
)
{
    // optional permission check
   if (!await _permissionService.HasPermissionAsync(userId, "USERS_EDIT"))
    return StatusCode(403, "Permission denied");

    var users = await _context.Users
        .Where(u => u.CompanyId == companyId && u.IsActive)
        .Select(u => new
        {
            
            u.Id,
            FullName = u.FirstName + " " + u.LastName,
            u.Email
        })
        .ToListAsync();

    return Ok(users);
}




        // ===================== GET ONE =====================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(
            int id,
            [FromHeader(Name = "x-company-id")] int companyId
        )
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == id && u.CompanyId == companyId);

            if (user == null)
                return NotFound();

            return Ok(new
            {
                user.Id,
                user.Email,
                user.FullName,
                user.IsActive,
                role = new
                {
                    user.Role!.Id,
                    user.Role.Name
                }
            });
        }

        // ===================== CREATE =====================
      [HttpPost]
public async Task<IActionResult> CreateUser(
    CreateUpdateUserDto dto,
    [FromHeader(Name = "x-user-id")] int userId
)
{
   if (!await _permissionService.HasPermissionAsync(userId, "USERS_CREATE"))
    return StatusCode(403, "Permission denied");



    var creator = await _context.Users.FindAsync(userId);
    if (creator == null)
        return Unauthorized();

    var role = await _context.Roles.FindAsync(dto.RoleId);
    if (role == null)
        return BadRequest("Invalid role");

    if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
        return BadRequest("Email already exists");

    var tempPassword = dto.LastName;
    var user = new User
    {
        Email = dto.Email,
        FirstName = dto.FirstName,
        LastName = dto.LastName,
        RoleId = dto.RoleId,
        CompanyId = creator.CompanyId,
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(tempPassword),
        MustChangePassword = true,
        IsActive = true
    };

    _context.Users.Add(user);
    await _context.SaveChangesAsync();

    return Ok(new
    {
        user.Id,
        TemporaryPassword = tempPassword
    });
}



        // ===================== UPDATE =====================
       [HttpPut("{id}")]
public async Task<IActionResult> UpdateUser(
    int id,
    CreateUpdateUserDto dto,
    [FromHeader(Name = "x-company-id")] int companyId,
    [FromHeader(Name = "x-user-id")] int userId
)
{
    if (!await _permissionService.HasPermissionAsync(userId, "USERS_EDIT"))
    return StatusCode(403, "Permission denied");


    if (id != dto.Id)
        return BadRequest("ID mismatch");

    var user = await _context.Users
        .FirstOrDefaultAsync(u => u.Id == id && u.CompanyId == companyId);

    if (user == null)
        return NotFound();

    user.Email = dto.Email;
    user.FirstName = dto.FirstName;
    user.LastName = dto.LastName;
    user.IsActive = dto.IsActive;
    user.RoleId = dto.RoleId;

    await _context.SaveChangesAsync();
    return NoContent();
}


        // ===================== DELETE =====================
       [HttpDelete("{id}")]
public async Task<IActionResult> DeleteUser(int id)
{
    var user = await _context.Users.FindAsync(id);
    if (user == null) return NotFound();

    // 🔥 REMOVE USER-TEAM RELATIONSHIPS FIRST
    var userTeams = await _context.UserTeams
        .Where(ut => ut.UserId == id)
        .ToListAsync();

    _context.UserTeams.RemoveRange(userTeams);

    // 🔥 NOW REMOVE USER
    _context.Users.Remove(user);

    await _context.SaveChangesAsync();
    return Ok();
}

}

}