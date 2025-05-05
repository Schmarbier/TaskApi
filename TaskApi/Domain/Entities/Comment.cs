using System.Net.Mail;

namespace TaskApi.Domain.Entities
{
    public class Comment : AuditableEntity
    {
        public Guid Id { get; set; }
        public string Content { get; set; } = string.Empty;

        // Referencias
        public Guid TaskId { get; set; }
        public Task Task { get; set; } = null!;

        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        // Navegación
        public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
    }
}
