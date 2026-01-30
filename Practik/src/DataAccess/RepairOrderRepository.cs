using Core;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;

namespace DataAccess
{
    public class RepairOrderRepository
    {
        public void AddRepairOrder(RepairOrder order)
        {
            using (SqlConnection connection = Database.GetConnection())
            {
                connection.Open();
                const string getStatusIdQuery = "SELECT TOP (1) StatusId FROM RepairOrderStatuses WHERE Code = @Status";
                using (SqlCommand getStatusIdCmd = new SqlCommand(getStatusIdQuery, connection))
                {
                    getStatusIdCmd.Parameters.AddWithValue("@Status", order.Status);
                    object? statusIdObj = getStatusIdCmd.ExecuteScalar();
                    if (statusIdObj == null || statusIdObj == DBNull.Value)
                        throw new InvalidOperationException($"Repair order status '{order.Status}' not found. Seed RepairOrderStatuses first.");

                    int statusId = Convert.ToInt32(statusIdObj);

                    const string insertQuery = @"INSERT INTO RepairOrders (DeviceId, CreatedByUserId, CurrentStatusId, ProblemDescription, DateCreated)
                                 VALUES (@DeviceId, @ReceptionistId, @StatusId, @ProblemDescription, GETDATE());";
                    using SqlCommand command = new SqlCommand(insertQuery, connection);
                    command.Parameters.AddWithValue("@DeviceId", order.DeviceId);
                    command.Parameters.AddWithValue("@ReceptionistId", order.CreatedByUserId);
                    command.Parameters.AddWithValue("@StatusId", statusId);
                    command.Parameters.AddWithValue("@ProblemDescription", order.ProblemDescription);
                    command.ExecuteNonQuery();
                }
            }
        }

        public bool UpdateProblemDescription(int orderId, string problemDescription)
        {
            using (SqlConnection connection = Database.GetConnection())
            {
                connection.Open();
                const string query = "UPDATE RepairOrders SET ProblemDescription = @ProblemDescription WHERE OrderId = @OrderId";
                using SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@OrderId", orderId);
                command.Parameters.AddWithValue("@ProblemDescription", problemDescription ?? string.Empty);
                return command.ExecuteNonQuery() > 0;
            }
        }

        public bool DeleteRepairOrder(int ownerUserId, int orderId)
        {
            using (SqlConnection connection = Database.GetConnection())
            {
                connection.Open();
                string query = @"DELETE ro
                                 FROM RepairOrders ro
                                 INNER JOIN Devices d ON d.DeviceId = ro.DeviceId
                                 WHERE ro.OrderId = @OrderId AND d.OwnerUserId = @OwnerUserId";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@OrderId", orderId);
                command.Parameters.AddWithValue("@OwnerUserId", ownerUserId);
                return command.ExecuteNonQuery() > 0;
            }
        }

        public bool DeleteRepairOrderById(int orderId)
        {
            using (SqlConnection connection = Database.GetConnection())
            {
                connection.Open();
                string query = "DELETE FROM RepairOrders WHERE OrderId = @OrderId";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@OrderId", orderId);
                return command.ExecuteNonQuery() > 0;
            }
        }

        public void AssignTechnician(int orderId, int technicianId)
        {
            using (SqlConnection connection = Database.GetConnection())
            {
                connection.Open();
                string query = @"
                    MERGE RepairOrderAssignments AS target
                    USING (SELECT @OrderId AS OrderId, @TechnicianId AS UserId) AS source
                    ON (target.OrderId = source.OrderId)
                    WHEN MATCHED THEN
                        UPDATE SET UserId = source.UserId, DateAssigned = GETDATE()
                    WHEN NOT MATCHED THEN
                        INSERT (OrderId, UserId, DateAssigned)
                        VALUES (source.OrderId, source.UserId, GETDATE());";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@OrderId", orderId);
                command.Parameters.AddWithValue("@TechnicianId", technicianId);
                command.ExecuteNonQuery();
            }
        }

        public List<RepairOrder> GetAllRepairOrders()
        {
            var orders = new List<RepairOrder>();
            using (SqlConnection connection = Database.GetConnection())
            {
                connection.Open();
                string query = @"SELECT ro.OrderId,
                                        ro.DeviceId,
                                        ro.CreatedByUserId,
                                        roa.UserId AS TechnicianId,
                                        u.Username AS TechnicianName,
                                        ros.Code AS Status,
                                        ro.ProblemDescription,
                                        c.FirstName,
                                        c.LastName,
                                        c.PhoneNumber,
                                        dt.Name AS DeviceTypeName,
                                        d.Manufacturer,
                                        d.Model
                                 FROM RepairOrders ro
                                 INNER JOIN Devices d ON d.DeviceId = ro.DeviceId
                                 INNER JOIN Clients c ON c.ClientId = d.ClientId
                                 INNER JOIN DeviceTypes dt ON dt.DeviceTypeId = d.DeviceTypeId
                                 INNER JOIN RepairOrderStatuses ros ON ro.CurrentStatusId = ros.StatusId
                                 LEFT JOIN RepairOrderAssignments roa ON ro.OrderId = roa.OrderId
                                 LEFT JOIN Users u ON u.UserId = roa.UserId
                                 ORDER BY ro.OrderId DESC";
                SqlCommand command = new SqlCommand(query, connection);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        orders.Add(MapReaderToRepairOrder(reader));
                    }
                }
            }
            return orders;
        }

        public List<RepairOrder> GetAllRepairOrders(int ownerUserId)
        {
            var orders = new List<RepairOrder>();
            using (SqlConnection connection = Database.GetConnection())
            {
                connection.Open();
                string query = @"SELECT ro.OrderId,
                                        ro.DeviceId,
                                        ro.CreatedByUserId,
                                        roa.UserId AS TechnicianId,
                                        u.Username AS TechnicianName,
                                        ros.Code AS Status,
                                        ro.ProblemDescription,
                                        c.FirstName,
                                        c.LastName,
                                        c.PhoneNumber,
                                        dt.Name AS DeviceTypeName,
                                        d.Manufacturer,
                                        d.Model
                                 FROM RepairOrders ro
                                 INNER JOIN Devices d ON d.DeviceId = ro.DeviceId
                                 INNER JOIN Clients c ON c.ClientId = d.ClientId
                                 INNER JOIN DeviceTypes dt ON dt.DeviceTypeId = d.DeviceTypeId
                                 INNER JOIN RepairOrderStatuses ros ON ro.CurrentStatusId = ros.StatusId
                                 LEFT JOIN RepairOrderAssignments roa ON ro.OrderId = roa.OrderId
                                 LEFT JOIN Users u ON u.UserId = roa.UserId
                                 WHERE d.OwnerUserId = @OwnerUserId
                                 ORDER BY ro.OrderId DESC";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@OwnerUserId", ownerUserId);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        orders.Add(MapReaderToRepairOrder(reader));
                    }
                }
            }
            return orders;
        }

        public void UpdateRepairStatus(int orderId, string status, int userId)
        {
            using (SqlConnection connection = Database.GetConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        string getStatusIdQuery = @"SELECT TOP (1) StatusId
                                                  FROM RepairOrderStatuses
                                                  WHERE UPPER(LTRIM(RTRIM(Code))) = UPPER(LTRIM(RTRIM(@Status)))
                                                     OR UPPER(LTRIM(RTRIM(Name))) = UPPER(LTRIM(RTRIM(@Status)))";
                        SqlCommand getStatusIdCmd = new SqlCommand(getStatusIdQuery, connection, transaction);
                        getStatusIdCmd.Parameters.AddWithValue("@Status", status);
                        object? statusIdObj = getStatusIdCmd.ExecuteScalar();
                        if (statusIdObj == null || statusIdObj == DBNull.Value)
                            throw new Exception($"Status '{status}' not found.");
                        int statusId = Convert.ToInt32(statusIdObj);

                        string updateQuery = "UPDATE RepairOrders SET CurrentStatusId = @StatusId WHERE OrderId = @OrderId";
                        SqlCommand updateCmd = new SqlCommand(updateQuery, connection, transaction);
                        updateCmd.Parameters.AddWithValue("@StatusId", statusId);
                        updateCmd.Parameters.AddWithValue("@OrderId", orderId);
                        updateCmd.ExecuteNonQuery();

                        string historyQuery = "INSERT INTO RepairOrderStatusHistories (OrderId, StatusId, UserId, DateChanged) VALUES (@OrderId, @StatusId, @UserId, GETDATE())";
                        SqlCommand historyCmd = new SqlCommand(historyQuery, connection, transaction);
                        historyCmd.Parameters.AddWithValue("@OrderId", orderId);
                        historyCmd.Parameters.AddWithValue("@StatusId", statusId);
                        historyCmd.Parameters.AddWithValue("@UserId", userId);
                        historyCmd.ExecuteNonQuery();

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public List<RepairOrder> GetRepairOrdersByTechnician(int technicianId)
        {
            var orders = new List<RepairOrder>();
            using (SqlConnection connection = Database.GetConnection())
            {
                connection.Open();
                string query = @"SELECT ro.OrderId,
                                        ro.DeviceId,
                                        ro.CreatedByUserId,
                                        roa.UserId AS TechnicianId,
                                        u.Username AS TechnicianName,
                                        ros.Code AS Status,
                                        ro.ProblemDescription,
                                        c.FirstName,
                                        c.LastName,
                                        c.PhoneNumber,
                                        dt.Name AS DeviceTypeName,
                                        d.Manufacturer,
                                        d.Model
                                 FROM RepairOrders ro
                                 INNER JOIN Devices d ON d.DeviceId = ro.DeviceId
                                 INNER JOIN Clients c ON c.ClientId = d.ClientId
                                 INNER JOIN DeviceTypes dt ON dt.DeviceTypeId = d.DeviceTypeId
                                 INNER JOIN RepairOrderStatuses ros ON ro.CurrentStatusId = ros.StatusId
                                 INNER JOIN RepairOrderAssignments roa ON ro.OrderId = roa.OrderId
                                 INNER JOIN Users u ON u.UserId = roa.UserId
                                 WHERE roa.UserId = @TechnicianId
                                 ORDER BY ro.OrderId DESC";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@TechnicianId", technicianId);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        orders.Add(MapReaderToRepairOrder(reader));
                    }
                }
            }
            return orders;
        }

        private RepairOrder MapReaderToRepairOrder(SqlDataReader reader)
        {
            return new RepairOrder
            {
                OrderId = reader.GetInt32(reader.GetOrdinal("OrderId")),
                DeviceId = reader.GetInt32(reader.GetOrdinal("DeviceId")),
                CreatedByUserId = reader.GetInt32(reader.GetOrdinal("CreatedByUserId")),
                TechnicianId = reader.IsDBNull(reader.GetOrdinal("TechnicianId")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("TechnicianId")),
                TechnicianName = reader.IsDBNull(reader.GetOrdinal("TechnicianName")) ? string.Empty : reader.GetString(reader.GetOrdinal("TechnicianName")),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                ProblemDescription = reader.GetString(reader.GetOrdinal("ProblemDescription")),
                ClientName = $"{reader.GetString(reader.GetOrdinal("FirstName"))} {reader.GetString(reader.GetOrdinal("LastName"))} ({reader.GetString(reader.GetOrdinal("PhoneNumber"))})",
                DeviceTypeName = reader.GetString(reader.GetOrdinal("DeviceTypeName")),
                Manufacturer = reader.GetString(reader.GetOrdinal("Manufacturer")),
                Model = reader.GetString(reader.GetOrdinal("Model"))
            };
        }
    }
}
