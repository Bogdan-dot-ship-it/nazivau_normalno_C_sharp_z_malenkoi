using Core;
using DataAccess;

namespace BusinessLogic
{
    public class WorkReportService
    {
        private readonly WorkReportRepository _workReportRepository = new WorkReportRepository();

        public void CreateWorkReport(int orderId, string reportContent)
        {
            var report = new WorkReport
            {
                OrderId = orderId,
                ReportContent = reportContent
            };
            _workReportRepository.AddWorkReport(report);
        }
    }
}
