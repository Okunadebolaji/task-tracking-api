using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;


namespace TaskTrackingApi.Models;

public partial class User
{
    public int Id { get; set; }

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;


    public int RoleId { get; set; }
public Role? Role { get; set; }

public int CompanyId { get; set; }
public Company Company { get; set; } = null!;

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public string FirstName { get; set; } = null!;

  public bool MustChangePassword { get; set; } = true;

    public string LastName { get; set; } = null!;
public ICollection<TaskAssignment> TaskAssignments { get; set; } = new List<TaskAssignment>();

[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
public string? FullName { get; set; }

    public virtual ICollection<Task> TaskApprovedByUsers { get; set; } = new List<Task>();

    public virtual ICollection<Task> TaskCreatedByUsers { get; set; } = new List<Task>();

    public virtual ICollection<Task> TaskRejectedByUsers { get; set; } = new List<Task>();

   
}
