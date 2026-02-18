namespace TaskTrackingApi.Models
{
    public class Task
    {
        public int Id { get; set; }

        public string Module { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string? References { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public string? Comment { get; set; }
        public string Source { get; set; } = "Manual";

        public bool IsApproved { get; set; }
        public bool IsRejected { get; set; }

        public int CreatedByUserId { get; set; }
        public User CreatedByUser { get; set; } = null!;

        public int? ApprovedByUserId { get; set; }
        public User? ApprovedByUser { get; set; }

        public int? RejectedByUserId { get; set; }
        public User? RejectedByUser { get; set; }

        public DateTime CreatedAt { get; set; }

        // 🔗 Project
        public int ProjectId { get; set; }
        public Project Project { get; set; } = null!;

        // 🔗 Team
        public int? TeamId { get; set; }
        public Team? Team { get; set; } = null!;

        // 🔗 Many-to-many
   
         public DateTime ApprovedDate { get; set; }
            public DateTime RejectedDate { get; set; }

  public int CompanyId { get; set; }  // 🔑 For multi-company isolation
public Company? Company { get; set; } = null!;

public int StatusId { get; set; }
public TaskStatus Status { get; set; } = null!;

public string? UserStory { get; set; }
        public ICollection<TaskRequirement> TaskRequirements { get; set; } = new List<TaskRequirement>();
         public ICollection<TaskAssignment> TaskAssignments { get; set; } = new List<TaskAssignment>();
    }
}
