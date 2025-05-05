using Microsoft.EntityFrameworkCore;
using System;
using TaskApi.Domain.Entities;

namespace TaskApi.Infraestructure.Data
{
    public class ApiDbContext : DbContext
    {
        public ApiDbContext(DbContextOptions<ApiDbContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; } = null!;

        public DbSet<LogEvent> Logs { get; set; } = null!;
    }
}
