namespace TaskTrackingApi.Dtos
{
    public class CreateUpdateUserDto
    {
        public int Id { get; set; }
        public string Email { get; set; } = null!;
        public int RoleId { get; set; } 
        public bool IsActive { get; set; }

        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string FullName { get; set; } = null!;

        // Used only when creating or changing password
        public string? Password { get; set; }
    }
}
