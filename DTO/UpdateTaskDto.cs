namespace TaskTrackingApi.Dtos
{
    public class UpdateTaskDto
    {
        public string? Module { get; set; }
        public string? Description { get; set; }
        public string? References { get; set; }
        public string? Comment { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Source { get; set; }
        public int StatusId { get; set; }
        public int ProjectId { get; set; }
        public int TeamId { get; set; }
         public string? UserStory { get; set; }
        public List<int> RequirementIds { get; set; } = new();
        public List<int> AssignedUserIds { get; set; } = new();
    }
}
