using System.ComponentModel.DataAnnotations;

namespace TaskApi.Domain.Entities
{
    public class LogEvent
    {
        [Key]
        public int Id { get; set; }

        public string? Message { get; set; }

        public string? MessageTemplate { get; set; }

        [MaxLength(128)]
        public string Level { get; set; } = string.Empty;

        public DateTime TimeStamp { get; set; }

        public string? Exception { get; set; }

        public string? Properties { get; set; }

        public string? LogEventJson { get; set; }
        public string? UserId { get; set; }
    }
}
