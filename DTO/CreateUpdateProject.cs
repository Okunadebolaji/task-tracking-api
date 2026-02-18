using System.ComponentModel.DataAnnotations;

namespace TaskTrackingApi.Dtos
{
    public class CreateUpdateProjectDto
    {
        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int CompanyId { get; set; }
}
    }

