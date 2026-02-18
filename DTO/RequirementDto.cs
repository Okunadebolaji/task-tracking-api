

namespace TaskTrackingApi.Dtos
{
    public class RequirementDto
    {
        public int Id { get; set; }
        public string Module { get; set; } = null!;
        public string Menu { get; set; } = null!;
        public string Requirement { get; set; } = null!;
        public string Category { get; set; } = null!;
        public int Baseline { get; set; }
        public string Status { get; set; } = null!;

        public int ProjectId { get; set; } 
    }
}
