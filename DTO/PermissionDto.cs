namespace TaskTrackingApi.Dtos
{
    public class PermissionDto
    {
        public int Id { get; set; }
        public string KeyName { get; set; } = null!;
        public string Name { get; set; } = null!;
        public bool IsAllowed { get; set; }
    }
}
