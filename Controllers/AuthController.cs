using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskTrackingApi.Dtos;
using TaskTrackingApi.Models;

namespace TaskTrackingApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly TaskTrackingDbContext _context;
        private readonly ICompanyCodeGenerator _codeGenerator;

        public AuthController(TaskTrackingDbContext context,ICompanyCodeGenerator codeGenerator)
        {
            _context = context;
            _codeGenerator = codeGenerator;
        }

        // =========================================
        // CHECK IF COMPANY HAS SUPER ADMIN
        // =========================================
        [HttpGet("has-superadmin/{companyCode}")]
        public async Task<IActionResult> HasSuperAdmin(string companyCode)
        {
            var company = await _context.Companies
                .FirstOrDefaultAsync(c => c.Code == companyCode);

            if (company == null)
                return Ok("wrong company code or does not exist");

            var exists = await _context.Users
                .Include(u => u.Role)
                .AnyAsync(u =>
                    u.CompanyId == company.Id &&
                    u.Role!.Name == "SuperAdmin"
                );

            return Ok(exists);
        }

        // SUPER ADMIN SIGNUP (ONE PER COMPANY)
     
       [HttpPost("superadmin-signup")]
    public async Task<IActionResult> SuperAdminSignup(SuperAdminSignupDto dto)
    {
        var superAdminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "SuperAdmin");
        if (superAdminRole == null) return BadRequest("SuperAdmin role not found");

        var existingCompany = await _context.Companies
            .FirstOrDefaultAsync(c => c.Name == dto.CompanyName);

        var company = existingCompany ?? new Company
        {
            Name = dto.CompanyName,
            Code = await _codeGenerator.GenerateAsync(dto.CompanyName)
        };

        if (existingCompany == null)
        {
            _context.Companies.Add(company);
            await _context.SaveChangesAsync();
        }

        string tempPassword = dto.LastName;

        var user = new User
        {
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            RoleId = superAdminRole.Id,
            CompanyId = company.Id,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(tempPassword),
            IsActive = true,
            MustChangePassword = true
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok(new { userId = user.Id, tempPassword });
    }
        // LOGIN
     
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Company)
                .FirstOrDefaultAsync(u =>
                    u.Email == dto.Email &&
                    u.IsActive
                );

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized("Invalid email or password");

            // Force password change
            if (user.MustChangePassword)
            {
                return Ok(new
                {
                    requiresPasswordChange = true,
                    userId = user.Id
                });
            }

            var permissions = await _context.RoleMenuPermissions
                .Include(rmp => rmp.Menu)
                .Where(rmp =>
                    rmp.RoleId == user.RoleId &&
                    rmp.CanView
                )
                .OrderBy(rmp => rmp.Menu.SortOrder)
                .Select(rmp => new
                {
                    menuId = rmp.Menu.Id,
                    menuKey = rmp.Menu.UniqueKey,
                    menuName = rmp.Menu.Name,
                    route = rmp.Menu.Route,
                    icon = rmp.Menu.Icon,
                    parentMenuId = rmp.Menu.ParentMenuId,

                    permissions = new
                    {
                        rmp.CanView,
                        rmp.CanCreate,
                        rmp.CanEdit,
                        rmp.CanDelete,
                        rmp.CanApprove,
                        rmp.CanReject
                    }
                })
                .ToListAsync();

            return Ok(new
            {
                requiresPasswordChange = false,

                user = new
                {
                    user.Id,
                    user.Email,
                    user.FullName,

                    roleId = user.RoleId,
                    roleName = user.Role!.Name,

                    companyId = user.CompanyId,
                    companyName = user.Company!.Name
                },

                menus = permissions
            });
        }

        // =========================================
        // CHANGE PASSWORD
        // =========================================
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
        {
            var user = await _context.Users.FindAsync(dto.UserId);
            if (user == null)
                return NotFound("User not found");

            if (!BCrypt.Net.BCrypt.Verify(dto.OldPassword, user.PasswordHash))
                return Unauthorized("Old password is incorrect");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            user.MustChangePassword = false;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Password changed successfully" });
        }
    }
}
