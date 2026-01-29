using System.Windows.Controls;

namespace UI
{
    public partial class LoginView : UserControl
    {
        public LoginView()
        {
            InitializeComponent();
        }

        private void PasswordBox_OnPasswordChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is LoginViewModel vm && sender is PasswordBox box)
            {
                vm.Password = box.Password;
            }
        }
    }
}
