using System;

namespace Core
{
    public class WorkActReportData
    {
        public int OrderId { get; set; }

        public DateTime DateReceived { get; set; }
        public DateTime? DateAssigned { get; set; }
        public DateTime? DateCompleted { get; set; }

        public string ClientFirstName { get; set; } = string.Empty;
        public string ClientLastName { get; set; } = string.Empty;
        public string ClientPhone { get; set; } = string.Empty;
        public string ClientEmail { get; set; } = string.Empty;

        public string DeviceTypeName { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;

        public string ProblemDescription { get; set; } = string.Empty;
        public string CurrentStatus { get; set; } = string.Empty;

        public string AcceptedBy { get; set; } = string.Empty;
        public string Technician { get; set; } = string.Empty;
        public string CompletedBy { get; set; } = string.Empty;
        public string AssignedBy { get; set; } = string.Empty;
    }
}
