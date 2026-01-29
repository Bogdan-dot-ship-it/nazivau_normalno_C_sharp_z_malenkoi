using System.Windows;
using DataAccess;
using Microsoft.EntityFrameworkCore;
using BusinessLogic;

namespace UI
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                using (var context = new WorkshopDbContext())
                {
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
