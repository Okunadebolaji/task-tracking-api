using Microsoft.EntityFrameworkCore;

namespace TaskTrackingApi.Models
{
    public partial class TaskTrackingDbContext : DbContext
    {
        public TaskTrackingDbContext(DbContextOptions<TaskTrackingDbContext> options)
            : base(options)
        {
        }

        public DbSet<Requirement> Requirements { get; set; }
        public DbSet<Task> Tasks { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Company> Companies => Set<Company>();
        public DbSet<ProjectUser> ProjectUsers => Set<ProjectUser>();
        public DbSet<Menu> Menus => Set<Menu>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<TaskStatus> TaskStatuses { get; set; }
        public DbSet<TaskAssignment> TaskAssignments { get; set; }
        public DbSet<RoleMenuPermission> RoleMenuPermissions => Set<RoleMenuPermission>();
        public DbSet<TaskRequirement> TaskRequirements { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Team> Teams => Set<Team>();
        public DbSet<UserTeam> UserTeams => Set<UserTeam>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ---------------- Requirements ----------------
            modelBuilder.Entity<Requirement>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Category).HasMaxLength(100);
                entity.Property(e => e.Module).HasMaxLength(150);
                entity.Property(e => e.Menu).HasMaxLength(200);
                entity.Property(e => e.Status).HasMaxLength(50);
                
                // FIXED: Removed double dots and used PostgreSQL syntax
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(r => r.Project)
                    .WithMany(p => p.Requirements)
                    .HasForeignKey(r => r.ProjectId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ---------------- Tasks ----------------
            modelBuilder.Entity<Task>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Module).HasMaxLength(150);
                entity.Property(e => e.References).HasMaxLength(500);
                entity.Property(e => e.Source).HasMaxLength(50);
                
                // FIXED: Changed getutcdate() to CURRENT_TIMESTAMP
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(t => t.CreatedByUser)
                    .WithMany(u => u.TaskCreatedByUsers)
                    .HasForeignKey(t => t.CreatedByUserId)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(t => t.ApprovedByUser)
                    .WithMany(u => u.TaskApprovedByUsers)
                    .HasForeignKey(t => t.ApprovedByUserId);

                entity.HasOne(t => t.RejectedByUser)
                    .WithMany(u => u.TaskRejectedByUsers)
                    .HasForeignKey(t => t.RejectedByUserId);

                entity.HasOne(t => t.Team)
                    .WithMany(team => team.Tasks)
                    .HasForeignKey(t => t.TeamId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Project>()
                .HasOne(p => p.Company)
                .WithMany()
                .HasForeignKey(p => p.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Team>()
                .HasOne(t => t.Company)
                .WithMany()
                .HasForeignKey(t => t.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            // ---------------- Users ----------------
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Email).HasMaxLength(150);
                entity.Property(e => e.FirstName).HasMaxLength(100).HasDefaultValue("");
                entity.Property(e => e.LastName).HasMaxLength(100).HasDefaultValue("");
                entity.Property(e => e.PasswordHash).HasMaxLength(255);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                
                // FIXED: Removed double dots and used PostgreSQL syntax
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(u => u.Company)
                      .WithMany(c => c.Users)
                      .HasForeignKey(u => u.CompanyId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(u => u.Role)
                      .WithMany(r => r.Users)
                      .HasForeignKey(u => u.RoleId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ---------------- Company ----------------
            modelBuilder.Entity<Company>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Code).HasMaxLength(50).IsRequired();
                entity.HasIndex(e => e.Code).IsUnique();
            });

            // ---------------- Menus ----------------
            modelBuilder.Entity<Menu>()
                .HasOne(m => m.ParentMenu)
                .WithMany(m => m.Children)
                .HasForeignKey(m => m.ParentMenuId)
                .OnDelete(DeleteBehavior.Restrict);

            // ---------------- Roles & Permissions ----------------
            modelBuilder.Entity<RoleMenuPermission>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Role)
                    .WithMany(r => r.RoleMenuPermissions)
                    .HasForeignKey(e => e.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Menu)
                    .WithMany(m => m.RoleMenuPermissions)
                    .HasForeignKey(e => e.MenuId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ---------------- TaskRequirements ----------------
            modelBuilder.Entity<TaskRequirement>()
                .HasOne(tr => tr.Task)
                .WithMany(t => t.TaskRequirements)
                .HasForeignKey(tr => tr.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TaskRequirement>()
                .HasOne(tr => tr.Requirement)
                .WithMany(r => r.TaskRequirements)
                .HasForeignKey(tr => tr.RequirementId)
                .OnDelete(DeleteBehavior.Cascade);

            // ---------------- Teams ----------------
            modelBuilder.Entity<Team>().HasKey(t => t.Id);

            modelBuilder.Entity<UserTeam>()
                .HasIndex(ut => new { ut.UserId, ut.TeamId })
                .IsUnique();

            modelBuilder.Entity<Task>()
                .HasOne(t => t.Status)
                .WithMany(s => s.Tasks)
                .HasForeignKey(t => t.StatusId)
                .OnDelete(DeleteBehavior.Restrict);

            // Seed global statuses
            modelBuilder.Entity<TaskStatus>().HasData(
                new TaskStatus { Id = 1, Name = "Pending", IsFinal = false, IsDefault = true, SortOrder = 1 },
                new TaskStatus { Id = 2, Name = "In Progress", IsFinal = false, IsDefault = false, SortOrder = 2 },
                new TaskStatus { Id = 3, Name = "Completed", IsFinal = true, IsDefault = false, SortOrder = 3 },
                new TaskStatus { Id = 4, Name = "Rejected", IsFinal = true, IsDefault = false, SortOrder = 4 }
            );

            // ===== Permissions =====
            modelBuilder.Entity<Permission>(entity =>
            {
                entity.ToTable("Permissions");
                entity.HasKey(p => p.Id);
                entity.Property(p => p.KeyName).IsRequired().HasMaxLength(100);
                entity.Property(p => p.Name).IsRequired().HasMaxLength(100);
                entity.Property(p => p.IsActive).HasDefaultValue(true);
            });

            // ===== RolePermissions =====
            modelBuilder.Entity<RolePermission>(entity =>
            {
                entity.ToTable("RolePermissions");
                entity.HasKey(rp => rp.Id);
                entity.HasOne(rp => rp.Role).WithMany(r => r.RolePermissions).HasForeignKey(rp => rp.RoleId);
                entity.HasOne(rp => rp.Permission).WithMany(p => p.RolePermissions).HasForeignKey(rp => rp.PermissionId);
            });

            // TaskAssignments many-to-many
            modelBuilder.Entity<TaskAssignment>()
                .HasOne(ta => ta.Task)
                .WithMany(t => t.TaskAssignments)
                .HasForeignKey(ta => ta.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TaskAssignment>()
                .HasOne(ta => ta.User)
                .WithMany(u => u.TaskAssignments)
                .HasForeignKey(ta => ta.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TaskAssignment>()
                .HasIndex(ta => new { ta.TaskId, ta.UserId }).IsUnique();

            base.OnModelCreating(modelBuilder);
            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}