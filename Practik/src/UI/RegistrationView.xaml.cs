using System.Windows.Controls;

namespace UI
{
    public partial class RegistrationView : UserControl
    {
        public RegistrationView()
        {
            InitializeComponent();
        }

        private void PasswordBox_OnPasswordChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is RegistrationViewModel vm && sender is PasswordBox box)
            {
                vm.Password = box.Password;
            }
        }
    }
}
