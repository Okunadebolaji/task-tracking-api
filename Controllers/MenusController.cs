using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskTrackingApi.Dtos;
using TaskTrackingApi.Models;

namespace TaskTrackingApi.Controllers
{
    [ApiController]
[Route("api/[controller]")]
public class MenusController : ControllerBase
{
    private readonly TaskTrackingDbContext _db;
private readonly IPermissionService _permissionService;

public MenusController(TaskTrackingDbContext db, IPermissionService permissionService)
{
    _db = db;
    _permissionService = permissionService;
}

    // ✅ ROLE-BASED MENUS (SIDEBAR)
  [HttpGet("by-role")]
public async Task<IActionResult> GetMenusByRole( [FromHeader(Name = "x-user-id")] string userIdHeader)
{
    if (!int.TryParse(userIdHeader, out var userId))
        return BadRequest("Invalid x-user-id");

    var user = await _db.Users.FindAsync(userId);
    if (user == null) return Unauthorized();

  

    var rolePermissions = await _db.RoleMenuPermissions
        .Include(rmp => rmp.Menu)
        .Where(rmp => rmp.RoleId == user.RoleId && rmp.CanView && rmp.Menu.IsActive)
        .OrderBy(rmp => rmp.Menu.SortOrder)
        .ToListAsync();

    var menuDtos = rolePermissions.Select(rmp => new MenuDto
    {
        MenuId = rmp.Menu.Id,
        Name = rmp.Menu.Name,
        Route = rmp.Menu.Route,
        Icon = rmp.Menu.Icon,
        ParentMenuId = rmp.Menu.ParentMenuId
    }).ToList();

    var tree = BuildMenuTree(menuDtos);
    return Ok(tree);
}


private List<MenuDto> BuildMenuTree(List<MenuDto> menus)
{
    var map = new Dictionary<int, MenuDto>();
    var roots = new List<MenuDto>();

    // Initialize  thechildren
    foreach (var m in menus)
    {
        m.Children = new List<MenuDto>();
        map[m.MenuId] = m;
    }

    foreach (var m in menus)
    {
        if (m.ParentMenuId.HasValue)
        {
            if (map.ContainsKey(m.ParentMenuId.Value))
                map[m.ParentMenuId.Value].Children.Add(m);
        }
        else
        {
            roots.Add(m);
        }
    }

    return roots;
}


}

}

    






    

    