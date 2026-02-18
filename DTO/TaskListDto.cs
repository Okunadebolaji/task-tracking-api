public class TaskListDto
{
    public int Id { get; set; }
    public string Module { get; set; } = null!;
    public string Status { get; set; } = null!;
    public string ProjectName { get; set; } = null!;
    public string TeamName { get; set; } = null!;
    public bool IsApproved { get; set; }
    public bool IsRejected { get; set; }
}
