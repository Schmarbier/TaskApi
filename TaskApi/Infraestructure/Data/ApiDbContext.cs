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

    }
}
