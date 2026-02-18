using System.Collections.Generic;
using TaskTrackingApi.Models;
public class Menu
{
    public int Id { get; set; }
    public string UniqueKey { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Route { get; set; }
    public string? Icon { get; set; }

    public int? ParentMenuId { get; set; }
    public Menu? ParentMenu { get; set; }
  public ICollection<Menu> Children { get; set; } = new List<Menu>();

    public ICollection<RoleMenuPermission> RoleMenuPermissions { get; set; }
        = new List<RoleMenuPermission>();
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}
