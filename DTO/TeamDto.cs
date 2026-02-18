namespace TaskTrackingApi.Dtos
{
    public class TeamDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int? ProjectId { get; set; }
        public bool IsActive { get; set; }
          public int MaxMembers { get; set; }
        public int CompanyId { get; set; }
}
    
    }

