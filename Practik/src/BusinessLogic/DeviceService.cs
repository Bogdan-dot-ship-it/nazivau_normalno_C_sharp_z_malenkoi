using Core;
using DataAccess;
using System.Collections.Generic;

namespace BusinessLogic
{
    public class DeviceService
    {
        private readonly DeviceRepository _deviceRepository = new DeviceRepository();
        private readonly DeviceTypeRepository _deviceTypeRepository = new DeviceTypeRepository();

        public void CreateDevice(int ownerUserId, int clientId, string deviceType, string manufacturer, string model, string serialNumber)
        {
            var device = new Device
            {
                OwnerUserId = ownerUserId,
                ClientId = clientId,
                DeviceTypeName = deviceType,
                Manufacturer = manufacturer,
                Model = model,
                SerialNumber = serialNumber
            };
            _deviceRepository.AddDevice(device);
        }

        public List<Device> GetAllDevices()
        {
            return _deviceRepository.GetAllDevices();
        }

        public List<Device> GetAllDevices(int ownerUserId)
        {
            return _deviceRepository.GetAllDevices(ownerUserId);
        }

        public bool UpdateDevice(int ownerUserId, int deviceId, int clientId, string deviceType, string manufacturer, string model, string serialNumber)
        {
            var device = new Device
            {
                DeviceId = deviceId,
                OwnerUserId = ownerUserId,
                ClientId = clientId,
                DeviceTypeName = deviceType,
                Manufacturer = manufacturer,
                Model = model,
                SerialNumber = serialNumber
            };
            return _deviceRepository.UpdateDevice(device);
        }

        public bool UpdateDeviceById(int deviceId, int ownerUserId, int clientId, string deviceType, string manufacturer, string model, string serialNumber)
        {
            var device = new Device
            {
                DeviceId = deviceId,
                OwnerUserId = ownerUserId,
                ClientId = clientId,
                DeviceTypeName = deviceType,
                Manufacturer = manufacturer,
                Model = model,
                SerialNumber = serialNumber
            };
            return _deviceRepository.UpdateDeviceById(device);
        }

        public bool DeleteDevice(int ownerUserId, int deviceId)
        {
            return _deviceRepository.DeleteDevice(ownerUserId, deviceId);
        }

        public bool DeleteDeviceById(int deviceId)
        {
            return _deviceRepository.DeleteDeviceById(deviceId);
        }

        public List<DeviceType> GetAllDeviceTypes()
        {
            return _deviceTypeRepository.GetAllDeviceTypes();
        }
    }
}
