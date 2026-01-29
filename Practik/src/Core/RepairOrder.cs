using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core
{
    public class RepairOrder
    {
        public int OrderId { get; set; }
        public int DeviceId { get; set; }
        public int CreatedByUserId { get; set; }
        public int CurrentStatusId { get; set; }
        public string ProblemDescription { get; set; } = string.Empty;
        public DateTime DateCreated { get; set; }

        [NotMapped]
        public int? TechnicianId { get; set; }

        [NotMapped]
        public string TechnicianName { get; set; } = string.Empty;

        [NotMapped]
        public string Status { get; set; } = string.Empty;

        [NotMapped]
        public string ClientName { get; set; } = string.Empty;

        [NotMapped]
        public string DeviceTypeName { get; set; } = string.Empty;

        [NotMapped]
        public string Manufacturer { get; set; } = string.Empty;

        [NotMapped]
        public string Model { get; set; } = string.Empty;
    }
}
