using Core;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;

namespace DataAccess
{
    public class DeviceTypeRepository
    {
        public List<DeviceType> GetAllDeviceTypes()
        {
            var types = new List<DeviceType>();
            using (SqlConnection connection = Database.GetConnection())
            {
                connection.Open();
                string query = "SELECT DeviceTypeId, Name FROM DeviceTypes ORDER BY Name";
                SqlCommand command = new SqlCommand(query, connection);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        types.Add(new DeviceType
                        {
                            DeviceTypeId = reader.GetInt32(reader.GetOrdinal("DeviceTypeId")),
                            Name = reader.GetString(reader.GetOrdinal("Name"))
                        });
                    }
                }
            }

            return types;
        }
    }
}
