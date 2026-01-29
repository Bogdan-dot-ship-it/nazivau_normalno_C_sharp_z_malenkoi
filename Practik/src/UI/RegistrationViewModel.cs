using BusinessLogic;
using System;
using System.Windows.Input;

namespace UI
{
    public class RegistrationViewModel : ViewModelBase
    {
        private readonly UserService _userService = new UserService();
        private string _username = string.Empty;
        public string Username { get => _username; set { _username = value; OnPropertyChanged(); (RegisterCommand as RelayCommand)?.RaiseCanExecuteChanged(); } }

        private string _password = string.Empty;
        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged();
                (RegisterCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        private string _firstName = string.Empty;
        public string FirstName { get => _firstName; set { _firstName = value; OnPropertyChanged(); (RegisterCommand as RelayCommand)?.RaiseCanExecuteChanged(); } }

        private string _lastName = string.Empty;
        public string LastName { get => _lastName; set { _lastName = value; OnPropertyChanged(); (RegisterCommand as RelayCommand)?.RaiseCanExecuteChanged(); } }

        private string _errorMessage = string.Empty;
        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        public RelayCommand RegisterCommand { get; }

        public RelayCommand BackToLoginCommand { get; }

        public event Action? BackToLoginRequested;

        public event Action? RegistrationSuccess;

        public RegistrationViewModel()
        {
            RegisterCommand = new RelayCommand(Register);
            BackToLoginCommand = new RelayCommand(_ => BackToLoginRequested?.Invoke());
        }

        private void Register(object? parameter)
        {
            try
            {
                ErrorMessage = string.Empty;

                if (string.IsNullOrWhiteSpace(Username) ||
                    string.IsNullOrWhiteSpace(Password) ||
                    string.IsNullOrWhiteSpace(FirstName) ||
                    string.IsNullOrWhiteSpace(LastName))
                {
                    ErrorMessage = "Fill in all fields.";
                    return;
                }

                _userService.RegisterUser(Username, Password, FirstName, LastName);
                RegistrationSuccess?.Invoke();
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.ToString();
            }
        }
    }
}
