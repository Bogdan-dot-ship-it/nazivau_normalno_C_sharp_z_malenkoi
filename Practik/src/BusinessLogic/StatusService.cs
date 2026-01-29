using DataAccess;
using System.Collections.Generic;

namespace BusinessLogic
{
    public class StatusService
    {
        private readonly StatusRepository _statusRepository = new StatusRepository();

        public List<string> GetAllStatuses()
        {
            return _statusRepository.GetAllStatuses();
        }
    }
}
