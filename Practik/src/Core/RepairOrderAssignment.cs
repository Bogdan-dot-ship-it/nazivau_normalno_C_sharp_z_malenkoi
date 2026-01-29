using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core
{
    public class RepairOrderAssignment
    {
        public int AssignmentId { get; set; }
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public DateTime DateAssigned { get; set; }

        [ForeignKey(nameof(OrderId))]
        public RepairOrder RepairOrder { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}
