using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core
{
    public class WorkAct
    {
        public int ActId { get; set; }
        public int OrderId { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime DateCreated { get; set; }

        [ForeignKey(nameof(OrderId))]
        public RepairOrder RepairOrder { get; set; } = null!;
    }
}
