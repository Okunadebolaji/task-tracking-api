namespace TaskTrackingApi.Dtos
{
    public class UpdateTaskStatusDto
    {
        public int TaskId { get; set; }
        public int NewStatusId { get; set; }
    }
}

