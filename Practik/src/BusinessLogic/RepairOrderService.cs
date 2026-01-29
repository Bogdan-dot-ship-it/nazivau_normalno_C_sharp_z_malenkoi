using Core;
using DataAccess;
using System.Collections.Generic;

namespace BusinessLogic
{
    public class RepairOrderService
    {
        private readonly RepairOrderRepository _repairOrderRepository = new RepairOrderRepository();

        public void CreateRepairOrder(int deviceId, int receptionistId, string problemDescription)
        {
            var order = new RepairOrder
            {
                DeviceId = deviceId,
                CreatedByUserId = receptionistId,
                Status = "NEW",
                ProblemDescription = problemDescription
            };
            _repairOrderRepository.AddRepairOrder(order);
        }

        public void AssignTechnician(int orderId, int technicianId)
        {
            _repairOrderRepository.AssignTechnician(orderId, technicianId);
        }

        public bool DeleteRepairOrder(int ownerUserId, int orderId)
        {
            return _repairOrderRepository.DeleteRepairOrder(ownerUserId, orderId);
        }

        public bool DeleteRepairOrderById(int orderId)
        {
            return _repairOrderRepository.DeleteRepairOrderById(orderId);
        }

        public List<RepairOrder> GetAllRepairOrders()
        {
            return _repairOrderRepository.GetAllRepairOrders();
        }

        public List<RepairOrder> GetAllRepairOrders(int ownerUserId)
        {
            return _repairOrderRepository.GetAllRepairOrders(ownerUserId);
        }

        public void UpdateRepairStatus(int orderId, string status, int userId)
        {
            _repairOrderRepository.UpdateRepairStatus(orderId, status, userId);
        }

        public List<RepairOrder> GetRepairOrdersByTechnician(int technicianId)
        {
            return _repairOrderRepository.GetRepairOrdersByTechnician(technicianId);
        }
    }
}
