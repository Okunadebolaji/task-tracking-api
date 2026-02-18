

namespace TaskTrackingApi.Models
{
public class TaskStatus
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;

    public int? CompanyId { get; set; }

    public bool IsFinal { get; set; }
    public bool IsDefault { get; set; }

    public int SortOrder { get; set; }
public ICollection<Task> Tasks { get; set; } = new List<Task>();
    public Company Company { get; set; } = null!;
}

}