using Core;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;

namespace DataAccess
{
    public class DeviceRepository
    {
        public void AddDevice(Device device)
        {
            using (SqlConnection connection = Database.GetConnection())
            {
                connection.Open();
                string query = "INSERT INTO Devices (OwnerUserId, ClientId, DeviceTypeId, Manufacturer, Model, SerialNumber) VALUES (@OwnerUserId, @ClientId, (SELECT TOP (1) DeviceTypeId FROM DeviceTypes WHERE Name = @DeviceType), @Manufacturer, @Model, @SerialNumber)";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@OwnerUserId", device.OwnerUserId);
                command.Parameters.AddWithValue("@ClientId", device.ClientId);
                command.Parameters.AddWithValue("@DeviceType", device.DeviceTypeName);
                command.Parameters.AddWithValue("@Manufacturer", device.Manufacturer);
                command.Parameters.AddWithValue("@Model", device.Model);
                command.Parameters.AddWithValue("@SerialNumber", device.SerialNumber);
                command.ExecuteNonQuery();
            }
        }

        public bool UpdateDevice(Device device)
        {
            using (SqlConnection connection = Database.GetConnection())
            {
                connection.Open();
                string query = @"UPDATE Devices
                                 SET ClientId = @ClientId,
                                     DeviceTypeId = (SELECT TOP (1) DeviceTypeId FROM DeviceTypes WHERE Name = @DeviceType),
                                     Manufacturer = @Manufacturer,
                                     Model = @Model,
                                     SerialNumber = @SerialNumber
                                 WHERE DeviceId = @DeviceId AND OwnerUserId = @OwnerUserId";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@DeviceId", device.DeviceId);
                command.Parameters.AddWithValue("@OwnerUserId", device.OwnerUserId);
                command.Parameters.AddWithValue("@ClientId", device.ClientId);
                command.Parameters.AddWithValue("@DeviceType", device.DeviceTypeName);
                command.Parameters.AddWithValue("@Manufacturer", device.Manufacturer);
                command.Parameters.AddWithValue("@Model", device.Model);
                command.Parameters.AddWithValue("@SerialNumber", device.SerialNumber);
                return command.ExecuteNonQuery() > 0;
            }
        }

        public bool UpdateDeviceById(Device device)
        {
            using (SqlConnection connection = Database.GetConnection())
            {
                connection.Open();
                string query = @"UPDATE Devices
                                 SET OwnerUserId = @OwnerUserId,
                                     ClientId = @ClientId,
                                     DeviceTypeId = (SELECT TOP (1) DeviceTypeId FROM DeviceTypes WHERE Name = @DeviceType),
                                     Manufacturer = @Manufacturer,
                                     Model = @Model,
                                     SerialNumber = @SerialNumber
                                 WHERE DeviceId = @DeviceId";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@DeviceId", device.DeviceId);
                command.Parameters.AddWithValue("@OwnerUserId", device.OwnerUserId);
                command.Parameters.AddWithValue("@ClientId", device.ClientId);
                command.Parameters.AddWithValue("@DeviceType", device.DeviceTypeName);
                command.Parameters.AddWithValue("@Manufacturer", device.Manufacturer);
                command.Parameters.AddWithValue("@Model", device.Model);
                command.Parameters.AddWithValue("@SerialNumber", device.SerialNumber);
                return command.ExecuteNonQuery() > 0;
            }
        }

        public bool DeleteDevice(int ownerUserId, int deviceId)
        {
            using (SqlConnection connection = Database.GetConnection())
            {
                connection.Open();
                string query = "DELETE FROM Devices WHERE DeviceId = @DeviceId AND OwnerUserId = @OwnerUserId";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@DeviceId", deviceId);
                command.Parameters.AddWithValue("@OwnerUserId", ownerUserId);
                return command.ExecuteNonQuery() > 0;
            }
        }

        public bool DeleteDeviceById(int deviceId)
        {
            using (SqlConnection connection = Database.GetConnection())
            {
                connection.Open();
                string query = "DELETE FROM Devices WHERE DeviceId = @DeviceId";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@DeviceId", deviceId);
                return command.ExecuteNonQuery() > 0;
            }
        }

        public List<Device> GetAllDevices()
        {
            var devices = new List<Device>();
            using (SqlConnection connection = Database.GetConnection())
            {
                connection.Open();
                string query = @"SELECT d.DeviceId, d.OwnerUserId, d.ClientId, c.FirstName, c.LastName, c.PhoneNumber, d.DeviceTypeId, dt.Name AS DeviceTypeName, d.Manufacturer, d.Model, d.SerialNumber
                                 FROM Devices d
                                 INNER JOIN DeviceTypes dt ON dt.DeviceTypeId = d.DeviceTypeId
                                 INNER JOIN Clients c ON c.ClientId = d.ClientId
                                 ORDER BY d.DeviceId DESC";
                SqlCommand command = new SqlCommand(query, connection);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        devices.Add(new Device
                        {
                            DeviceId = reader.GetInt32(reader.GetOrdinal("DeviceId")),
                            OwnerUserId = reader.GetInt32(reader.GetOrdinal("OwnerUserId")),
                            ClientId = reader.GetInt32(reader.GetOrdinal("ClientId")),
                            DeviceTypeId = reader.GetInt32(reader.GetOrdinal("DeviceTypeId")),
                            DeviceTypeName = reader.GetString(reader.GetOrdinal("DeviceTypeName")),
                            ClientName = $"{reader.GetString(reader.GetOrdinal("FirstName"))} {reader.GetString(reader.GetOrdinal("LastName"))} ({reader.GetString(reader.GetOrdinal("PhoneNumber"))})",
                            Manufacturer = reader.GetString(reader.GetOrdinal("Manufacturer")),
                            Model = reader.GetString(reader.GetOrdinal("Model")),
                            SerialNumber = reader.IsDBNull(reader.GetOrdinal("SerialNumber")) ? string.Empty : reader.GetString(reader.GetOrdinal("SerialNumber"))
                        });
                    }
                }
            }
            return devices;
        }

        public List<Device> GetAllDevices(int ownerUserId)
        {
            var devices = new List<Device>();
            using (SqlConnection connection = Database.GetConnection())
            {
                connection.Open();
                string query = @"SELECT d.DeviceId, d.OwnerUserId, d.ClientId, c.FirstName, c.LastName, c.PhoneNumber, d.DeviceTypeId, dt.Name AS DeviceTypeName, d.Manufacturer, d.Model, d.SerialNumber
                                 FROM Devices d
                                 INNER JOIN DeviceTypes dt ON dt.DeviceTypeId = d.DeviceTypeId
                                 INNER JOIN Clients c ON c.ClientId = d.ClientId
                                 WHERE d.OwnerUserId = @OwnerUserId
                                 ORDER BY d.DeviceId DESC";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@OwnerUserId", ownerUserId);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        devices.Add(new Device
                        {
                            DeviceId = reader.GetInt32(reader.GetOrdinal("DeviceId")),
                            OwnerUserId = reader.GetInt32(reader.GetOrdinal("OwnerUserId")),
                            ClientId = reader.GetInt32(reader.GetOrdinal("ClientId")),
                            DeviceTypeId = reader.GetInt32(reader.GetOrdinal("DeviceTypeId")),
                            DeviceTypeName = reader.GetString(reader.GetOrdinal("DeviceTypeName")),
                            ClientName = $"{reader.GetString(reader.GetOrdinal("FirstName"))} {reader.GetString(reader.GetOrdinal("LastName"))} ({reader.GetString(reader.GetOrdinal("PhoneNumber"))})",
                            Manufacturer = reader.GetString(reader.GetOrdinal("Manufacturer")),
                            Model = reader.GetString(reader.GetOrdinal("Model")),
                            SerialNumber = reader.IsDBNull(reader.GetOrdinal("SerialNumber")) ? string.Empty : reader.GetString(reader.GetOrdinal("SerialNumber"))
                        });
                    }
                }
            }
            return devices;
        }
    }
}
