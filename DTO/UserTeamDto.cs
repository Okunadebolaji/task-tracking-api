namespace TaskTrackingApi.Dtos
{
    public class UserTeamDto
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public string UserEmail { get; set; } = null!;

        public int TeamId { get; set; }
        public string TeamName { get; set; } = null!;
    }
}
