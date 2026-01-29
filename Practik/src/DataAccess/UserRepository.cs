using Core;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;

namespace DataAccess
{
    public class UserRepository
    {
        public int GetUsersCount()
        {
            using (SqlConnection connection = Database.GetConnection())
            {
                connection.Open();
                const string query = "SELECT COUNT(1) FROM Users";
                using SqlCommand command = new SqlCommand(query, connection);
                object? result = command.ExecuteScalar();
                return result == null || result == System.DBNull.Value ? 0 : (int)result;
            }
        }

        public void AddUser(User user)
        {
            using (SqlConnection connection = Database.GetConnection())
            {
                connection.Open();
                using var transaction = connection.BeginTransaction();

                int roleId = EnsureRoleExists(connection, transaction, user.Role.Code, user.Role.Name);

                const string query = "INSERT INTO Users (Username, PasswordHash, RoleId, FirstName, LastName) VALUES (@Username, @PasswordHash, @RoleId, @FirstName, @LastName)";
                using SqlCommand command = new SqlCommand(query, connection, transaction);
                command.Parameters.AddWithValue("@Username", user.Username);
                command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
                command.Parameters.AddWithValue("@RoleId", roleId);
                command.Parameters.AddWithValue("@FirstName", user.FirstName);
                command.Parameters.AddWithValue("@LastName", user.LastName);
                command.ExecuteNonQuery();

                transaction.Commit();
            }
        }

        private static int EnsureRoleExists(SqlConnection connection, SqlTransaction transaction, string roleCode, string? roleName)
        {
            const string ensureRoleSql = @"
IF NOT EXISTS (SELECT 1 FROM UserRoles WHERE Code = @RoleCode)
BEGIN
    INSERT INTO UserRoles (Code, Name) VALUES (@RoleCode, @RoleName);
END

SELECT TOP (1) RoleId FROM UserRoles WHERE Code = @RoleCode;";

            using SqlCommand cmd = new SqlCommand(ensureRoleSql, connection, transaction);
            cmd.Parameters.AddWithValue("@RoleCode", roleCode);
            cmd.Parameters.AddWithValue("@RoleName", string.IsNullOrWhiteSpace(roleName) ? roleCode : roleName);

            object? result = cmd.ExecuteScalar();
            if (result == null || result == System.DBNull.Value)
            {
                throw new System.InvalidOperationException($"Role '{roleCode}' was not found and could not be created.");
            }

            return (int)result;
        }

        public User? GetUserByUsername(string username)
        {
            User? user = null;
            using (SqlConnection connection = Database.GetConnection())
            {
                connection.Open();
                string query = "SELECT TOP (1) u.UserId, u.Username, u.PasswordHash, u.RoleId, r.RoleId AS RoleRoleId, r.Code AS RoleCode, r.Name AS RoleName, u.FirstName, u.LastName FROM Users u INNER JOIN UserRoles r ON r.RoleId = u.RoleId WHERE u.Username = @Username";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Username", username);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        user = new User
                        {
                            UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                            Username = reader.GetString(reader.GetOrdinal("Username")),
                            PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
                            RoleId = reader.GetInt32(reader.GetOrdinal("RoleId")),
                            Role = new UserRole
                            {
                                RoleId = reader.GetInt32(reader.GetOrdinal("RoleRoleId")),
                                Code = reader.GetString(reader.GetOrdinal("RoleCode")),
                                Name = reader.IsDBNull(reader.GetOrdinal("RoleName")) ? string.Empty : reader.GetString(reader.GetOrdinal("RoleName"))
                            },
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName"))
                        };
                    }
                }
            }
            return user;
        }

        public List<User> GetUsersByRole(string role)
        {
            var users = new List<User>();
            using (SqlConnection connection = Database.GetConnection())
            {
                connection.Open();
                string query = "SELECT u.UserId, u.Username, u.PasswordHash, u.RoleId, r.RoleId AS RoleRoleId, r.Code AS RoleCode, r.Name AS RoleName, u.FirstName, u.LastName FROM Users u INNER JOIN UserRoles r ON r.RoleId = u.RoleId WHERE r.Code = @RoleCode";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@RoleCode", role);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        users.Add(new User
                        {
                            UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                            Username = reader.GetString(reader.GetOrdinal("Username")),
                            PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
                            RoleId = reader.GetInt32(reader.GetOrdinal("RoleId")),
                            Role = new UserRole
                            {
                                RoleId = reader.GetInt32(reader.GetOrdinal("RoleRoleId")),
                                Code = reader.GetString(reader.GetOrdinal("RoleCode")),
                                Name = reader.IsDBNull(reader.GetOrdinal("RoleName")) ? string.Empty : reader.GetString(reader.GetOrdinal("RoleName"))
                            },
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName"))
                        });
                    }
                }
            }
            return users;
        }

        public List<User> GetAllUsers()
        {
            var users = new List<User>();
            using (SqlConnection connection = Database.GetConnection())
            {
                connection.Open();
                string query = "SELECT u.UserId, u.Username, u.PasswordHash, u.RoleId, r.RoleId AS RoleRoleId, r.Code AS RoleCode, r.Name AS RoleName, u.FirstName, u.LastName FROM Users u INNER JOIN UserRoles r ON r.RoleId = u.RoleId ORDER BY u.UserId";
                SqlCommand command = new SqlCommand(query, connection);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        users.Add(new User
                        {
                            UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                            Username = reader.GetString(reader.GetOrdinal("Username")),
                            PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
                            RoleId = reader.GetInt32(reader.GetOrdinal("RoleId")),
                            Role = new UserRole
                            {
                                RoleId = reader.GetInt32(reader.GetOrdinal("RoleRoleId")),
                                Code = reader.GetString(reader.GetOrdinal("RoleCode")),
                                Name = reader.IsDBNull(reader.GetOrdinal("RoleName")) ? string.Empty : reader.GetString(reader.GetOrdinal("RoleName"))
                            },
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName"))
                        });
                    }
                }
            }
            return users;
        }

        public List<UserRole> GetAllRoles()
        {
            var roles = new List<UserRole>();
            using (SqlConnection connection = Database.GetConnection())
            {
                connection.Open();
                const string query = "SELECT RoleId, Code, Name FROM UserRoles ORDER BY RoleId";
                using SqlCommand command = new SqlCommand(query, connection);
                using SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    roles.Add(new UserRole
                    {
                        RoleId = reader.GetInt32(reader.GetOrdinal("RoleId")),
                        Code = reader.GetString(reader.GetOrdinal("Code")),
                        Name = reader.IsDBNull(reader.GetOrdinal("Name")) ? string.Empty : reader.GetString(reader.GetOrdinal("Name"))
                    });
                }
            }
            return roles;
        }

        public bool UpdateUserRole(int userId, string roleCode)
        {
            using (SqlConnection connection = Database.GetConnection())
            {
                connection.Open();
                const string updateSql = "UPDATE u SET RoleId = r.RoleId FROM Users u INNER JOIN UserRoles r ON r.Code = @RoleCode WHERE u.UserId = @UserId";
                using SqlCommand command = new SqlCommand(updateSql, connection);
                command.Parameters.AddWithValue("@UserId", userId);
                command.Parameters.AddWithValue("@RoleCode", roleCode);
                return command.ExecuteNonQuery() > 0;
            }
        }
    }
}
