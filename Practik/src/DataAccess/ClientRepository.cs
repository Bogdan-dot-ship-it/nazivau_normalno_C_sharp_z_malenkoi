using Core;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;

namespace DataAccess
{
    public class ClientRepository
    {
        public void AddClient(Client client)
        {
            using (SqlConnection connection = Database.GetConnection())
            {
                connection.Open();
                string query = "INSERT INTO Clients (OwnerUserId, FirstName, LastName, PhoneNumber, Email) VALUES (@OwnerUserId, @FirstName, @LastName, @PhoneNumber, @Email)";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@OwnerUserId", client.OwnerUserId);
                command.Parameters.AddWithValue("@FirstName", client.FirstName);
                command.Parameters.AddWithValue("@LastName", client.LastName);
                command.Parameters.AddWithValue("@PhoneNumber", client.PhoneNumber);
                command.Parameters.AddWithValue("@Email", client.Email);
                command.ExecuteNonQuery();
            }
        }

        public bool UpdateClient(Client client)
        {
            using (SqlConnection connection = Database.GetConnection())
            {
                connection.Open();
                string query = "UPDATE Clients SET FirstName = @FirstName, LastName = @LastName, PhoneNumber = @PhoneNumber, Email = @Email WHERE ClientId = @ClientId AND OwnerUserId = @OwnerUserId";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@ClientId", client.ClientId);
                command.Parameters.AddWithValue("@OwnerUserId", client.OwnerUserId);
                command.Parameters.AddWithValue("@FirstName", client.FirstName);
                command.Parameters.AddWithValue("@LastName", client.LastName);
                command.Parameters.AddWithValue("@PhoneNumber", client.PhoneNumber);
                command.Parameters.AddWithValue("@Email", client.Email);
                return command.ExecuteNonQuery() > 0;
            }
        }

        public bool DeleteClient(int ownerUserId, int clientId)
        {
            using (SqlConnection connection = Database.GetConnection())
            {
                connection.Open();
                string query = "DELETE FROM Clients WHERE ClientId = @ClientId AND OwnerUserId = @OwnerUserId";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@ClientId", clientId);
                command.Parameters.AddWithValue("@OwnerUserId", ownerUserId);
                return command.ExecuteNonQuery() > 0;
            }
        }

        public List<Client> GetAllClients()
        {
            var clients = new List<Client>();
            using (SqlConnection connection = Database.GetConnection())
            {
                connection.Open();
                string query = "SELECT ClientId, OwnerUserId, FirstName, LastName, PhoneNumber, Email FROM Clients ORDER BY ClientId DESC";
                SqlCommand command = new SqlCommand(query, connection);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        clients.Add(new Client
                        {
                            ClientId = reader.GetInt32(reader.GetOrdinal("ClientId")),
                            OwnerUserId = reader.GetInt32(reader.GetOrdinal("OwnerUserId")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            PhoneNumber = reader.GetString(reader.GetOrdinal("PhoneNumber")),
                            Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? string.Empty : reader.GetString(reader.GetOrdinal("Email"))
                        });
                    }
                }
            }

            return clients;
        }

        public List<Client> GetAllClients(int ownerUserId)
        {
            var clients = new List<Client>();
            using (SqlConnection connection = Database.GetConnection())
            {
                connection.Open();
                string query = "SELECT ClientId, OwnerUserId, FirstName, LastName, PhoneNumber, Email FROM Clients WHERE OwnerUserId = @OwnerUserId ORDER BY ClientId DESC";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@OwnerUserId", ownerUserId);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        clients.Add(new Client
                        {
                            ClientId = reader.GetInt32(reader.GetOrdinal("ClientId")),
                            OwnerUserId = reader.GetInt32(reader.GetOrdinal("OwnerUserId")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            PhoneNumber = reader.GetString(reader.GetOrdinal("PhoneNumber")),
                            Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? string.Empty : reader.GetString(reader.GetOrdinal("Email"))
                        });
                    }
                }
            }

            return clients;
        }
    }
}
