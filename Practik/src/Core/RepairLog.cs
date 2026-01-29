using System;

namespace Core
{
    public class RepairLog
    {
        public int LogId { get; set; }
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public string Action { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}
