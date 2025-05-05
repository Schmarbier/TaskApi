namespace TaskApi.Domain.Entities
{
    public class Task : AuditableEntity
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public TaskStatus Status { get; set; }
        public int Priority { get; set; }
        public DateTime? DueDate { get; set; }

        // Referencias
        public Guid ProjectId { get; set; }
        public Project Project { get; set; } = null!;

        public Guid? AssignedUserId { get; set; }
        public User? AssignedUser { get; set; }

        // Navegación
        public ICollection<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}
