using System.Windows;
using DataAccess;
using Microsoft.EntityFrameworkCore;
using BusinessLogic;
using Microsoft.Data.SqlClient;

namespace UI
{
    public partial class App : Application
    {
        private static readonly string[] BaselineMigrationIds =
        {
            "20260202160000_CreateUserRoles",
            "20260202160010_CreateUsers",
            "20260202160020_CreateClients",
            "20260202160030_CreateDeviceTypes",
            "20260202160040_CreateDevices",
            "20260202160050_CreateRepairOrderStatuses",
            "20260202160060_CreateRepairOrders",
            "20260202160070_CreateRepairOrderAssignments",
            "20260202160080_CreateRepairOrderStatusHistories",
            "20260202160090_CreateWorkActs",
            "20260202160100_CreateAuditLogs"
        };

        private static bool TableExists(SqlConnection connection, string tableName)
        {
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT 1 WHERE OBJECT_ID(@obj, 'U') IS NOT NULL";
            cmd.Parameters.AddWithValue("@obj", $"dbo.{tableName}");
            object? result = cmd.ExecuteScalar();
            return result != null && result != DBNull.Value;
        }

        private static void BaselineMigrations(SqlConnection connection)
        {
            if (!TableExists(connection, "__EFMigrationsHistory"))
                return;

            foreach (string migrationId in BaselineMigrationIds)
            {
                using var cmd = connection.CreateCommand();
                cmd.CommandText = @"
IF NOT EXISTS (SELECT 1 FROM __EFMigrationsHistory WHERE MigrationId = @MigrationId)
BEGIN
    INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion)
    VALUES (@MigrationId, @ProductVersion)
END
";
                cmd.Parameters.AddWithValue("@MigrationId", migrationId);
                cmd.Parameters.AddWithValue("@ProductVersion", "8.0.6");
                cmd.ExecuteNonQuery();
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                using (var context = new WorkshopDbContext())
                {
                    try
                    {
                        using var connection = Database.GetConnection();
                        connection.Open();

                        // If schema already exists, the DB was created by older migrations.
                        // Baseline the new per-table migrations to prevent "table already exists" errors.
                        if (TableExists(connection, "UserRoles"))
                        {
                            BaselineMigrations(connection);
                        }
                    }
                    catch
                    {
                        // If connection fails (e.g., DB doesn't exist yet), proceed with normal migration.
                    }

                    context.Database.Migrate();
                }

                Database.Initialize();

                var userService = new UserService();
                userService.EnsureDefaultAdminUser();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Database Migration Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
                return;
            }

            var shellView = new ShellView
            {
                DataContext = new ShellViewModel()
            };
            shellView.Show();
        }
    }
}
