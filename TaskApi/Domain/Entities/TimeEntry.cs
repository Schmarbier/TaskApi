using TaskApi.Domain.Common;

namespace TaskApi.Domain.Entities
{
    public class TimeEntry : AuditableEntity
    {
        public Guid Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public TimeSpan? Duration => EndTime.HasValue ? EndTime.Value - StartTime : null;

        // Referencias
        public Guid TaskId { get; set; }
        public Task Task { get; set; } = null!;

        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
    }
}
