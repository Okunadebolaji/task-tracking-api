namespace TaskTrackingApi.Models
{
    public class TaskRequirement
{
    public int Id { get; set; }

    public int TaskId { get; set; }
    public Task Task { get; set; } = null!;

    public int RequirementId { get; set; }
    public Requirement Requirement { get; set; } = null!;
}

}
