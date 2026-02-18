namespace TaskTrackingApi.Models
{
    public class TaskAssignment
    {
        public int Id { get; set; }

        // 🔗 Task
        public int TaskId { get; set; }
        public Task Task { get; set; } = null!;

        // 🔗 User
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    }
}