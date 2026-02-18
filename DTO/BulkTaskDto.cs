using System.ComponentModel.DataAnnotations;
public class BulkTaskDto
{
    public string? Module { get; set; }
    public string? Description { get; set; }
    public string? Status { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }

     [Required]
        public int ProjectId { get; set; }

    public string? Source { get; set; }
}
