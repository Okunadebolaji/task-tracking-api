namespace TaskTrackingApi.Dtos
{
    public class ProjectDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int TaskCount { get; set; }
        public int CompletedCount { get; set; }
        public int PendingCount { get; set; }
        public int RejectedCount { get; set; }
    }
}
