using System.ComponentModel.DataAnnotations;

namespace TaskTrackingApi.Models
{
    public class Role
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

         public string? Description { get; set; }
    public bool IsSystem { get; set; }

public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();

        public ICollection<RoleMenuPermission> RoleMenuPermissions { get; set; }
            = new List<RoleMenuPermission>();

        public ICollection<User> Users { get; set; }
            = new List<User>();
    }
}
