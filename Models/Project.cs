using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TaskTrackingApi.Models
{
    public class Project
    {
        public int Id { get; set; }

        [Required, MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int CompanyId { get; set; }
    public Company Company { get; set; } = null!;

        // Navigation
       public ICollection<Team> Teams { get; set; } = new List<Team>();
        public ICollection<Task> Tasks { get; set; } = new List<Task>();
        public ICollection<Requirement> Requirements { get; set; } = new List<Requirement>();
    }
}
