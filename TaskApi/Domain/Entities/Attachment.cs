using TaskApi.Domain.Common;

namespace TaskApi.Domain.Entities
{
    public class Attachment : AuditableEntity
    {
        public Guid Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public long FileSize { get; set; }

        // Referencias
        public Guid CommentId { get; set; }
        public Comment Comment { get; set; } = null!;
    }
}
