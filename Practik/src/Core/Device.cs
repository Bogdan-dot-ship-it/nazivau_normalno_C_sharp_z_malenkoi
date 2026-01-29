using System.ComponentModel.DataAnnotations.Schema;

namespace Core
{
    public class Device
    {
        public int DeviceId { get; set; }
        public int OwnerUserId { get; set; }
        public int ClientId { get; set; }
        public int DeviceTypeId { get; set; }

        [NotMapped]
        public string DeviceTypeName { get; set; } = string.Empty;

        [NotMapped]
        public string ClientName { get; set; } = string.Empty;

        public string Manufacturer { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
    }
}
