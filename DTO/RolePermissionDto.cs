public class RolePermissionDto
{
    public string Menu { get; set; } = null!;
    public bool CanView { get; set; }
    public bool CanCreate { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
    public bool CanApprove { get; set; }
    public bool CanReject { get; set; }
}
