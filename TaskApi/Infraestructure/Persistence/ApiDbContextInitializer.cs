using Microsoft.EntityFrameworkCore;
using TaskApi.Domain.Entities;
using TaskApi.Infraestructure.Data;
using TaskApi.Infraestructure.Identity;
using Task = System.Threading.Tasks.Task;

namespace TaskApi.Infraestructure.Persistence
{
    public class ApiDbContextInitializer
    {
        private readonly ApiDbContext _context;
        private readonly ILogger<ApiDbContextInitializer> _logger;
        private readonly TokenService _tokenService;

        public ApiDbContextInitializer(
            ApiDbContext context,
            ILogger<ApiDbContextInitializer> logger,
            TokenService tokenService)
        {
            _context = context;
            _logger = logger;
            _tokenService = tokenService;
        }

        public async Task InitializeDatabaseAsync()
        {
            try
            {
                if (_context.Database.IsSqlServer())
                {
                    await _context.Database.MigrateAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante la migración de la base de datos.");
                throw;
            }
        }

        public async Task SeedDatabaseAsync()
        {
            try
            {
                await TrySeedAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante la siembra de la base de datos.");
                throw;
            }
        }

        private async Task TrySeedAsync()
        {
            // Seed usuarios predeterminados
            if (!_context.Users.Any())
            {
                // Crear usuario administrador
                var adminUser = new User
                {
                    Id = Guid.NewGuid(),
                    Username = "admin",
                    Email = "admin@example.com",
                    PasswordHash = HashPassword("Admin123!"), // Esto debe ser reemplazado con tu método de hash
                    Role = "Admin",
                    CreatedDate = DateTime.UtcNow
                };

                // Crear usuario normal
                var normalUser = new User
                {
                    Id = Guid.NewGuid(),
                    Username = "user",
                    Email = "user@example.com",
                    PasswordHash = HashPassword("User123!"), // Esto debe ser reemplazado con tu método de hash
                    Role = "User",
                    CreatedDate = DateTime.UtcNow
                };

                _context.Users.AddRange(adminUser, normalUser);
                await _context.SaveChangesAsync();

                // Crear un proyecto de ejemplo
                var project = new Project
                {
                    Id = Guid.NewGuid(),
                    Name = "Proyecto de ejemplo",
                    Description = "Este es un proyecto de ejemplo creado automáticamente",
                    AdminUserId = adminUser.Id,
                    CreatedDate = DateTime.UtcNow
                };

                _context.Projects.Add(project);

                // Asignar usuario normal al proyecto
                project.AssignedUsers = new List<User> { normalUser };

                // Crear tareas de ejemplo
                var task1 = new Domain.Entities.Task
                {
                    Id = Guid.NewGuid(),
                    Title = "Tarea de ejemplo 1",
                    Description = "Esta es una tarea de ejemplo",
                    Status = Domain.Enums.TaskStatus.NotStarted,
                    Priority = 1,
                    ProjectId = project.Id,
                    AssignedUserId = normalUser.Id,
                    CreatedDate = DateTime.UtcNow,
                    DueDate = DateTime.UtcNow.AddDays(7)
                };

                var task2 = new Domain.Entities.Task
                {
                    Id = Guid.NewGuid(),
                    Title = "Tarea de ejemplo 2",
                    Description = "Esta es otra tarea de ejemplo",
                    Status = Domain.Enums.TaskStatus.InProgress,
                    Priority = 2,
                    ProjectId = project.Id,
                    AssignedUserId = normalUser.Id,
                    CreatedDate = DateTime.UtcNow,
                    DueDate = DateTime.UtcNow.AddDays(5)
                };

                _context.Tasks.AddRange(task1, task2);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Base de datos inicializada con datos semilla");
            }
        }

        private string HashPassword(string password)
        {
            // Este es un método simplificado para desarrollo
            // En producción, usa un algoritmo más seguro
            using var hmac = new System.Security.Cryptography.HMACSHA512();
            var salt = hmac.Key;
            var hash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));

            // Combinar salt y hash
            var hashBytes = new byte[salt.Length + hash.Length];
            Array.Copy(salt, 0, hashBytes, 0, salt.Length);
            Array.Copy(hash, 0, hashBytes, salt.Length, hash.Length);

            return Convert.ToBase64String(hashBytes);
        }
    }
}
