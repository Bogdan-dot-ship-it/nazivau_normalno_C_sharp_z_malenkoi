using Microsoft.Data.SqlClient;
using System;

namespace DataAccess
{
    public static class Database
    {
        public static string DatabaseName { get; set; } = "workshop_db";

        private static string DefaultServerName { get; } = "(local)";

        public static string ConnectionString => BuildConnectionString(includeDatabase: true);

        public static string ServerConnectionString => BuildConnectionString(includeDatabase: false);

        private static string BuildConnectionString(bool includeDatabase)
        {
            string? connectionStringOverride = Environment.GetEnvironmentVariable("WORKSHOP_CONNECTION_STRING");
            if (!string.IsNullOrWhiteSpace(connectionStringOverride))
            {
                return connectionStringOverride;
            }

            string? serverOverride = Environment.GetEnvironmentVariable("WORKSHOP_SQLSERVER");
            string server = string.IsNullOrWhiteSpace(serverOverride) ? DefaultServerName : serverOverride;

            var builder = new SqlConnectionStringBuilder
            {
                DataSource = server,
                IntegratedSecurity = true,
                Encrypt = true,
                TrustServerCertificate = true
            };

            if (includeDatabase)
            {
                builder.InitialCatalog = DatabaseName;
            }

            return builder.ConnectionString;
        }

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(ConnectionString);
        }

        public static SqlConnection GetServerConnection()
        {
            return new SqlConnection(ServerConnectionString);
        }

        public static void Initialize()
        {
            using (SqlConnection connection = GetConnection())
            {
                connection.Open();

                const string ensureOwnerColumnsSql = @"
IF COL_LENGTH('Clients', 'OwnerUserId') IS NULL
BEGIN
    ALTER TABLE Clients ADD OwnerUserId INT NOT NULL CONSTRAINT DF_Clients_OwnerUserId DEFAULT(0);
END

IF COL_LENGTH('Devices', 'OwnerUserId') IS NULL
BEGIN
    ALTER TABLE Devices ADD OwnerUserId INT NOT NULL CONSTRAINT DF_Devices_OwnerUserId DEFAULT(0);
END
";

                using (SqlCommand command = new SqlCommand(ensureOwnerColumnsSql, connection))
                {
                    command.ExecuteNonQuery();
                }

                const string seedDeviceTypesSql = @"
IF NOT EXISTS (SELECT 1 FROM DeviceTypes)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM DeviceTypes WHERE Name = 'Phone') INSERT INTO DeviceTypes (Name) VALUES ('Phone');
    IF NOT EXISTS (SELECT 1 FROM DeviceTypes WHERE Name = 'Laptop') INSERT INTO DeviceTypes (Name) VALUES ('Laptop');
    IF NOT EXISTS (SELECT 1 FROM DeviceTypes WHERE Name = 'Tablet') INSERT INTO DeviceTypes (Name) VALUES ('Tablet');
    IF NOT EXISTS (SELECT 1 FROM DeviceTypes WHERE Name = 'PC') INSERT INTO DeviceTypes (Name) VALUES ('PC');
END
";

                using (SqlCommand command = new SqlCommand(seedDeviceTypesSql, connection))
                {
                    command.ExecuteNonQuery();
                }

                const string seedRepairOrderStatusesSql = @"
IF NOT EXISTS (SELECT 1 FROM RepairOrderStatuses)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM RepairOrderStatuses WHERE Code = 'NEW') INSERT INTO RepairOrderStatuses (Code, Name) VALUES ('NEW', 'New');
    IF NOT EXISTS (SELECT 1 FROM RepairOrderStatuses WHERE Code = 'IN_PROGRESS') INSERT INTO RepairOrderStatuses (Code, Name) VALUES ('IN_PROGRESS', 'In progress');
    IF NOT EXISTS (SELECT 1 FROM RepairOrderStatuses WHERE Code = 'DONE') INSERT INTO RepairOrderStatuses (Code, Name) VALUES ('DONE', 'Done');
END
ELSE
BEGIN
    IF NOT EXISTS (SELECT 1 FROM RepairOrderStatuses WHERE Code = 'NEW') INSERT INTO RepairOrderStatuses (Code, Name) VALUES ('NEW', 'New');
    IF NOT EXISTS (SELECT 1 FROM RepairOrderStatuses WHERE Code = 'IN_PROGRESS') INSERT INTO RepairOrderStatuses (Code, Name) VALUES ('IN_PROGRESS', 'In progress');
    IF NOT EXISTS (SELECT 1 FROM RepairOrderStatuses WHERE Code = 'DONE') INSERT INTO RepairOrderStatuses (Code, Name) VALUES ('DONE', 'Done');
END
";

                using (SqlCommand command = new SqlCommand(seedRepairOrderStatusesSql, connection))
                {
                    command.ExecuteNonQuery();
                }

                const string seedUserRolesSql = @"
IF NOT EXISTS (SELECT 1 FROM UserRoles)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM UserRoles WHERE Code = 'ADMIN') INSERT INTO UserRoles (Code, Name) VALUES ('ADMIN', 'Administrator');
    IF NOT EXISTS (SELECT 1 FROM UserRoles WHERE Code = 'MASTER') INSERT INTO UserRoles (Code, Name) VALUES ('MASTER', 'Master');
    IF NOT EXISTS (SELECT 1 FROM UserRoles WHERE Code = 'USER') INSERT INTO UserRoles (Code, Name) VALUES ('USER', 'User');
END
ELSE
BEGIN
    IF NOT EXISTS (SELECT 1 FROM UserRoles WHERE Code = 'ADMIN') INSERT INTO UserRoles (Code, Name) VALUES ('ADMIN', 'Administrator');
    IF NOT EXISTS (SELECT 1 FROM UserRoles WHERE Code = 'MASTER') INSERT INTO UserRoles (Code, Name) VALUES ('MASTER', 'Master');
    IF NOT EXISTS (SELECT 1 FROM UserRoles WHERE Code = 'USER') INSERT INTO UserRoles (Code, Name) VALUES ('USER', 'User');
END
";

                using (SqlCommand command = new SqlCommand(seedUserRolesSql, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
