using TaskTrackingApi.Dtos;
public class TaskDto
{
    public int Id { get; set; }
    public string Module { get; set; } = null!;
    public string Description { get; set; } = null!;
public int StatusId { get; set; }
public string? StatusName { get; set; }

    public string? Source { get; set; }
    public string? References { get; set; }

    public string? CreatedByUser { get; set; }
    public string? ApprovedByUser { get; set; }
    public string? RejectedByUser { get; set; }
    public string? TeamName { get; set; }
public string? UserStory { get; set; }
    // Optional: Include linked requirements
    public List<TaskRequirementDto> Requirements { get; set; } = new();
}
