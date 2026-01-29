using Core;
using Microsoft.Data.SqlClient;

namespace DataAccess
{
    public class WorkReportRepository
    {
        public void AddWorkReport(WorkReport report)
        {
            using (SqlConnection connection = Database.GetConnection())
            {
                connection.Open();
                string query = @"IF EXISTS (SELECT 1 FROM WorkActs WHERE OrderId = @OrderId)
BEGIN
    THROW 50001, 'Work act already exists for this order.', 1;
END

INSERT INTO WorkActs (OrderId, Description, DateCreated)
VALUES (@OrderId, @ReportContent, GETDATE());";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@OrderId", report.OrderId);
                command.Parameters.AddWithValue("@ReportContent", report.ReportContent);
                command.ExecuteNonQuery();
            }
        }
    }
}
