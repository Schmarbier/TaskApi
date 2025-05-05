namespace TaskApi.Domain.Entities
{
    public class Project : AuditableEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        // Referencias
        public Guid AdminUserId { get; set; }
        public User AdminUser { get; set; } = null!;

        // Navegación
        public ICollection<User> AssignedUsers { get; set; } = new List<User>();
        public ICollection<Domain.Entities.Task> Tasks { get; set; } = new List<Domain.Entities.Task>();
    }
}
