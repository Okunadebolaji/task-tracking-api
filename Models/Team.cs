namespace TaskTrackingApi.Models
{
    public class Team
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;

        public int? ProjectId { get; set; }
        public Project Project { get; set; } = null!;

          public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

           public int MaxMembers { get; set; } = 5;
         public int CompanyId { get; set; }
    public Company Company { get; set; } = null!;
    
               public ICollection<Task> Tasks { get; set; } = new List<Task>();
            public ICollection<UserTeam> UsersTeams { get; set; } = new List<UserTeam>();
    }
}
