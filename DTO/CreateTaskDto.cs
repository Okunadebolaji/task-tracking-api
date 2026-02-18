using System.ComponentModel.DataAnnotations;

namespace TaskTrackingApi.Dtos
{
    public class CreateTaskDto
    {
        [Required]
        public string Module { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        public string? References { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public string Status { get; set; } = "Pending";

        public string? Comment { get; set; }

        [Required]
        public int ProjectId { get; set; }

        [Required]
        public string Source { get; set; } = "Manual";
  public int? StatusId { get; set; }
         
         public string? UserStory { get; set; }
      public int? TeamId { get; set; }
         
          [Required]
    public List<int> RequirementIds { get; set; } = new();

      //  assigned team members
    public List<int>? AssignedUserIds { get; set; }

    }
}
