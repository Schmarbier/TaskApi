using Microsoft.EntityFrameworkCore;
using System;
using TaskApi.Domain.Entities;

namespace TaskApi.Infraestructure.Data
{
    public class ApiDbContext : DbContext
    {
        public ApiDbContext(DbContextOptions<ApiDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; } = null!;

        public DbSet<LogEvent> Logs { get; set; } = null!;

        public DbSet<TimeEntry> TimeEntries { get; set; } = null!;

        public DbSet<Project> Projects { get; set; } = null!;

        public DbSet<Domain.Entities.Task> Tasks { get; set; } = null!;

        public DbSet<Comment> Comments { get; set; } = null!;

        public DbSet<Attachment> Attachments { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración para evitar ciclos o múltiples rutas de cascada

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Task)
                .WithMany(t => t.Comments)
                .HasForeignKey(c => c.TaskId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Attachment>()
                .HasOne(a => a.Comment)
                .WithMany(c => c.Attachments)
                .HasForeignKey(a => a.CommentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TimeEntry>()
                .HasOne(te => te.Task)
                .WithMany(t => t.TimeEntries)
                .HasForeignKey(te => te.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TimeEntry>()
                .HasOne(te => te.User)
                .WithMany()
                .HasForeignKey(te => te.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Domain.Entities.Task>()
                .HasOne(t => t.Project)
                .WithMany(p => p.Tasks)
                .HasForeignKey(t => t.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Domain.Entities.Task>()
                .HasOne(t => t.AssignedUser)
                .WithMany()
                .HasForeignKey(t => t.AssignedUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Project>()
                .HasOne(p => p.AdminUser)
                .WithMany()
                .HasForeignKey(p => p.AdminUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
