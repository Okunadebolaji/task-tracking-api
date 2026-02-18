namespace TaskTrackingApi.Models
{
    public class Permission
    {
        public int Id { get; set; }
        public string KeyName { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsActive { get; set; }

        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
