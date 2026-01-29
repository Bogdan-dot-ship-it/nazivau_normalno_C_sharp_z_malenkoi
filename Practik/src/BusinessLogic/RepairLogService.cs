using Core;
using DataAccess;
using System;

namespace BusinessLogic
{
    public class RepairLogService
    {
        private readonly RepairLogRepository _repairLogRepository = new RepairLogRepository();

        public void CreateRepairLog(int orderId, int userId, string action)
        {
            var log = new RepairLog
            {
                OrderId = orderId,
                UserId = userId,
                Action = action,
                Timestamp = DateTime.Now
            };
            _repairLogRepository.AddRepairLog(log);
        }
    }
}
