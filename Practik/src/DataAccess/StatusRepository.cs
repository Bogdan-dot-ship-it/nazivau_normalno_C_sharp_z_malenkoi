using Microsoft.Data.SqlClient;
using System.Collections.Generic;

namespace DataAccess
{
    public class StatusRepository
    {
        public List<string> GetAllStatuses()
        {
            var statuses = new List<string>();
            using (SqlConnection connection = Database.GetConnection())
            {
                connection.Open();
                string query = "SELECT Code FROM RepairOrderStatuses ORDER BY StatusId";
                SqlCommand command = new SqlCommand(query, connection);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        statuses.Add(reader.GetString(0));
                    }
                }
            }
            return statuses;
        }
    }
}
