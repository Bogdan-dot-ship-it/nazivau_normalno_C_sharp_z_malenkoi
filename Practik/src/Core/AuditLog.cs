using System;

namespace Core
{
    public class AuditLog
    {
        public int LogId { get; set; }
        public int UserId { get; set; }
        public string ActionType { get; set; } = string.Empty;
        public string? Details { get; set; }
        public DateTime DateCreated { get; set; }

        public User User { get; set; } = null!;
    }
}
