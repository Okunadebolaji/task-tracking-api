namespace TaskTrackingApi.Models;

public class RoleMenuPermission
{
    public int Id { get; set; }

    public int RoleId { get; set; }
    public Role Role { get; set; } = null!;

    public int MenuId { get; set; }
    public Menu Menu { get; set; } = null!;

    public bool CanView { get; set; }
    public bool CanCreate { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
    public bool CanApprove { get; set; }
    public bool CanReject { get; set; }
}
