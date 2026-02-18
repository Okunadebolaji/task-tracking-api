namespace TaskTrackingApi.Models
{
    public class Requirement
    {
        public int Id { get; set; }

        public string Module { get; set; } = null!;
        public string Menu { get; set; } = null!;
        public string RequirementText { get; set; } = null!;
        public string Category { get; set; } = null!;
        public int Baseline { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; }

        // 🔗 Project
        public int ProjectId { get; set; }
        public Project Project { get; set; } = null!;


      // 🔗 AUDIT
    public int CreatedByUserId { get; set; }
    public User CreatedByUser { get; set; } = null!;
        // 🔗 Many-to-many
         public ICollection<TaskRequirement> TaskRequirements { get; set; } = new List<TaskRequirement>();
    }
}
