namespace TaskTrackingApi.Dtos
{
    public class TaskRequirementDto
    {
        public int TaskId { get; set; }
        public int RequirementId { get; set; }
    public string RequirementText { get; set; } = null!;
    }
}
