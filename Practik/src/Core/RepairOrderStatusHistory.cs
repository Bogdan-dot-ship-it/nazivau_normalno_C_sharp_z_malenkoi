using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core
{
    public class RepairOrderStatusHistory
    {
        public int HistoryId { get; set; }
        public int OrderId { get; set; }
        public int StatusId { get; set; }
        public int UserId { get; set; }
        public DateTime DateChanged { get; set; }

        [ForeignKey(nameof(OrderId))]
        public RepairOrder RepairOrder { get; set; } = null!;
        public RepairOrderStatus Status { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}
