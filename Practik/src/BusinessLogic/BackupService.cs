using DataAccess;
using Microsoft.Data.SqlClient;
using System;
using System.Diagnostics;
using System.IO;

namespace BusinessLogic
{
    public class BackupService
    {
        public (bool Success, string Message) CreateBackup()
        {
            try
            {
                var builder = new SqlConnectionStringBuilder(Database.ConnectionString);
                string server = builder.DataSource;
                string database = builder.InitialCatalog;

                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string backupFileName = $"workshop_backup_{DateTime.Now:yyyyMMdd_HHmmss}.bak";
                string backupFilePath = Path.Combine(desktopPath, backupFileName);

                string command = $"BACKUP DATABASE [{database}] TO DISK = N'{backupFilePath}' WITH NOFORMAT, NOINIT, NAME = N'{database}-full', SKIP, NOREWIND, NOUNLOAD, STATS = 10";

                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "sqlcmd",
                    Arguments = $"-S {server} -E -Q \"{command}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardError = true
                };

                using (Process process = Process.Start(startInfo)!)
                {
                    string error = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    if (process.ExitCode != 0)
                    {
                        throw new Exception($"sqlcmd failed with exit code {process.ExitCode}.\nError: {error}");
                    }
                }

                return (true, $"Backup created successfully at: {backupFilePath}");
            }
            catch (Exception ex)
            {
                return (false, $"Backup failed: {ex.Message}");
            }
        }
    }
}
