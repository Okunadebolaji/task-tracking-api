public class TaskStatusDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public bool IsFinal { get; set; }
    public bool IsDefault { get; set; }
}
