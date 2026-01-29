using Core;
using Microsoft.Data.SqlClient;

namespace DataAccess
{
    public class RepairLogRepository
    {
        public void AddRepairLog(RepairLog log)
        {
            using (SqlConnection connection = Database.GetConnection())
            {
                connection.Open();
                string query = "INSERT INTO AuditLogs (UserId, ActionType, Details, DateCreated) VALUES (@UserId, @Action, @Details, @Timestamp)";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@UserId", log.UserId);
                command.Parameters.AddWithValue("@Action", log.Action);
                command.Parameters.AddWithValue("@Details", $"Order ID: {log.OrderId}");
                command.Parameters.AddWithValue("@Timestamp", log.Timestamp);
                command.ExecuteNonQuery();
            }
        }
    }
}
