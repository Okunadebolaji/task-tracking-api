namespace TaskTrackingApi.Dtos
{
    public class DashboardCountersDto
    {
        public int Pending { get; set; }
        public int WIP { get; set; }
        public int Completed { get; set; }
        public int Overdue { get; set; }
    }
}
