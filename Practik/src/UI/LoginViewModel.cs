using BusinessLogic;
using Core;
using System;
using System.Security;
using System.Windows.Input;

namespace UI
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly UserService _userService = new UserService();
        private string _username = string.Empty;
        private string _password = string.Empty;
        private string _errorMessage = string.Empty;

        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged();
                (LoginCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged();
                (LoginCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        public ICommand LoginCommand { get; }

        public ICommand RegisterCommand { get; }

        public event Action<User>? LoginSuccess;

        public event Action? RegisterRequested;

        public LoginViewModel()
        {
            LoginCommand = new RelayCommand(Login);
            RegisterCommand = new RelayCommand(_ => RegisterRequested?.Invoke());
        }

        private void Login(object? obj)
        {
            try
            {
                ErrorMessage = string.Empty;

                if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
                {
                    ErrorMessage = "Enter username and password.";
                    return;
                }

                User? user = _userService.Authenticate(Username, Password);
                if (user != null)
                {
                    LoginSuccess?.Invoke(user);
                }
                else
                {
                    ErrorMessage = "Invalid username or password.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.ToString();
            }
        }
    }
}
