using Core;
using System.Windows.Input;

namespace UI
{
    public class ShellViewModel : ViewModelBase
    {
        private static string GetRoleCode(User? user) => user?.Role?.Code?.Trim() ?? string.Empty;

        private bool _isMenuOpen;
        public bool IsMenuOpen
        {
            get => _isMenuOpen;
            set { _isMenuOpen = value; OnPropertyChanged(); }
        }

        private ViewModelBase? _currentViewModel;
        public ViewModelBase? CurrentViewModel
        {
            get => _currentViewModel;
            set { _currentViewModel = value; OnPropertyChanged(); }
        }

        private User? _currentUser;
        public User? CurrentUser
        {
            get => _currentUser;
            set
            {
                _currentUser = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsUserLoggedIn));
                OnPropertyChanged(nameof(CanShowAdmin));
                OnPropertyChanged(nameof(CanShowMaster));
                OnPropertyChanged(nameof(CanShowUser));
                OnPropertyChanged(nameof(CanShowClients));
                OnPropertyChanged(nameof(CanShowOrders));
                OnPropertyChanged(nameof(CanShowDevices));
            }
        }

        public bool IsUserLoggedIn => CurrentUser != null;

        public bool CanShowAdmin => IsUserLoggedIn && string.Equals(GetRoleCode(CurrentUser), "ADMIN", System.StringComparison.OrdinalIgnoreCase);
        public bool CanShowMaster => IsUserLoggedIn && string.Equals(GetRoleCode(CurrentUser), "MASTER", System.StringComparison.OrdinalIgnoreCase);

        public bool CanShowUser => IsUserLoggedIn && string.Equals(GetRoleCode(CurrentUser), "USER", System.StringComparison.OrdinalIgnoreCase);

        public bool CanShowClients => IsUserLoggedIn && (CanShowAdmin || CanShowUser || CanShowMaster);
        public bool CanShowOrders => IsUserLoggedIn && (CanShowAdmin || CanShowUser || CanShowMaster);
        public bool CanShowDevices => IsUserLoggedIn && (CanShowAdmin || CanShowUser);

        public ICommand ShowDashboardViewCommand { get; }
        public ICommand ShowClientsViewCommand { get; }
        public ICommand ShowDevicesViewCommand { get; }
        public ICommand ShowOrdersViewCommand { get; }
        public ICommand ShowAdminViewCommand { get; }
        public ICommand ShowMasterViewCommand { get; }
        public ICommand LogoutCommand { get; }

        public ShellViewModel()
        {
            ShowLoginView();

            ShowDashboardViewCommand = new RelayCommand(_ => ShowDashboardView(), _ => IsUserLoggedIn);
            ShowClientsViewCommand = new RelayCommand(_ => { if (CurrentUser != null) CurrentViewModel = new ClientsViewModel(CurrentUser); }, _ => IsUserLoggedIn);
            ShowDevicesViewCommand = new RelayCommand(_ => { if (CurrentUser != null) CurrentViewModel = new DevicesViewModel(CurrentUser); }, _ => IsUserLoggedIn);
            ShowOrdersViewCommand = new RelayCommand(_ => { if (CurrentUser != null) CurrentViewModel = new OrdersViewModel(CurrentUser); }, _ => IsUserLoggedIn);
            ShowAdminViewCommand = new RelayCommand(_ => { if (CurrentUser != null) CurrentViewModel = new AdminViewModel(CurrentUser); }, _ => CanShowAdminView());
            ShowMasterViewCommand = new RelayCommand(_ => { if (CurrentUser != null) CurrentViewModel = new MasterViewModel(CurrentUser); }, _ => CanShowMasterView());
            LogoutCommand = new RelayCommand(_ => Logout());
        }

        private void ShowLoginView()
        {
            IsMenuOpen = false;
            var loginViewModel = new LoginViewModel();
            loginViewModel.LoginSuccess += OnLoginSuccess;
            loginViewModel.RegisterRequested += ShowRegistrationView;
            CurrentViewModel = loginViewModel;
            CurrentUser = null;
        }

        private void ShowRegistrationView()
        {
            IsMenuOpen = false;
            var registrationViewModel = new RegistrationViewModel();
            registrationViewModel.BackToLoginRequested += ShowLoginView;
            registrationViewModel.RegistrationSuccess += ShowLoginView;
            CurrentViewModel = registrationViewModel;
        }

        private void OnLoginSuccess(User user)
        {
            CurrentUser = user;
            IsMenuOpen = true;

            ShowDashboardView();
        }

        private void ShowDashboardView()
        {
            if (CurrentUser == null)
            {
                ShowLoginView();
                return;
            }

            CurrentViewModel = new DashboardViewModel(
                CurrentUser,
                ShowAdminViewCommand,
                ShowMasterViewCommand,
                ShowClientsViewCommand,
                ShowDevicesViewCommand,
                ShowOrdersViewCommand);
        }

        private void Logout()
        {
            IsMenuOpen = false;
            ShowLoginView();
        }

        private bool CanShowAdminView()
        {
            return CanShowAdmin;
        }

        private bool CanShowMasterView()
        {
            return CanShowMaster;
        }
    }
}
