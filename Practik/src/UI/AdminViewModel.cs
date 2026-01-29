using BusinessLogic;
using Core;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Input;

namespace UI
{
    public class AdminViewModel : ViewModelBase
    {
        private readonly UserService _userService = new UserService();
        private readonly RepairOrderService _repairOrderService = new RepairOrderService();

        // Create User properties
        private string _username = string.Empty;
        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged();
                (CreateUserCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        private string _password = string.Empty;
        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged();
                (CreateUserCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        private string _role = "User";
        public string Role
        {
            get => _role;
            set
            {
                _role = value;
                OnPropertyChanged();
                (CreateUserCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        private string _firstName = string.Empty;
        public string FirstName
        {
            get => _firstName;
            set
            {
                _firstName = value;
                OnPropertyChanged();
                (CreateUserCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        private string _lastName = string.Empty;
        public string LastName
        {
            get => _lastName;
            set
            {
                _lastName = value;
                OnPropertyChanged();
                (CreateUserCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        // Assign Technician properties
        private RepairOrder? _selectedRepairOrder;
        public RepairOrder? SelectedRepairOrder
        {
            get => _selectedRepairOrder;
            set
            {
                _selectedRepairOrder = value;
                OnPropertyChanged();
                (AssignTechnicianCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        private void DeleteUser()
        {
            if (SelectedUser == null)
                return;

            if (SelectedUser.UserId == _currentUser.UserId)
            {
                System.Windows.MessageBox.Show(
                    "You cannot delete the currently logged-in user.",
                    "Error",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
                return;
            }

            var confirm = System.Windows.MessageBox.Show(
                $"Delete user '{SelectedUser.Username}'?",
                "Confirm deletion",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Warning);

            if (confirm != System.Windows.MessageBoxResult.Yes)
                return;

            try
            {
                bool deleted = _userService.DeleteUser(SelectedUser.UserId, _currentUser.UserId);
                if (!deleted)
                {
                    System.Windows.MessageBox.Show(
                        "User was not deleted.",
                        "Delete user",
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Warning);
                }

                LoadUsersAndRoles();
                LoadData();
            }
            catch (System.Exception ex)
            {
                System.Windows.MessageBox.Show(
                    $"Failed to delete user: {ex.Message}",
                    "Error",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
        }

        private int? _selectedTechnicianId;
        public int? SelectedTechnicianId
        {
            get => _selectedTechnicianId;
            set
            {
                _selectedTechnicianId = value;
                OnPropertyChanged();
                (AssignTechnicianCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public ObservableCollection<RepairOrder> RepairOrders { get; set; }
        public ObservableCollection<User> Technicians { get; set; }

        public ObservableCollection<User> Users { get; } = new ObservableCollection<User>();
        public ObservableCollection<UserRole> Roles { get; } = new ObservableCollection<UserRole>();

        private User? _selectedUser;
        public User? SelectedUser
        {
            get => _selectedUser;
            set
            {
                _selectedUser = value;
                OnPropertyChanged();
                SyncSelectedUserRole();
                (UpdateUserRoleCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (DeleteUserCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        private UserRole? _selectedUserRole;
        public UserRole? SelectedUserRole
        {
            get => _selectedUserRole;
            set
            {
                _selectedUserRole = value;
                OnPropertyChanged();
                (UpdateUserRoleCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public ICommand CreateUserCommand { get; }
        public ICommand AssignTechnicianCommand { get; }
        public ICommand RefreshUsersCommand { get; }
        public ICommand UpdateUserRoleCommand { get; }
        public ICommand DeleteUserCommand { get; }

        private readonly User _currentUser;

        public AdminViewModel(User currentUser)
        {
            _currentUser = currentUser;
            RepairOrders = new ObservableCollection<RepairOrder>();
            Technicians = new ObservableCollection<User>();
            CreateUserCommand = new RelayCommand(CreateUser);
            AssignTechnicianCommand = new RelayCommand(AssignTechnician, CanAssignTechnician);
            RefreshUsersCommand = new RelayCommand(_ => LoadUsersAndRoles());
            UpdateUserRoleCommand = new RelayCommand(_ => UpdateUserRole(), _ => CanUpdateUserRole());
            DeleteUserCommand = new RelayCommand(_ => DeleteUser(), _ => CanDeleteUser());
            LoadData();
            LoadUsersAndRoles();
        }

        private bool CanAssignTechnician(object? obj)
        {
            return SelectedRepairOrder != null
                   && SelectedTechnicianId.HasValue && SelectedTechnicianId.Value > 0;
        }

        private void CreateUser(object? obj)
        {
            string passwordValue = (obj as PasswordBox)?.Password ?? obj as string ?? Password;

            bool missingUsername = string.IsNullOrWhiteSpace(Username);
            bool missingPassword = string.IsNullOrWhiteSpace(passwordValue);
            bool missingFirstName = string.IsNullOrWhiteSpace(FirstName);
            bool missingLastName = string.IsNullOrWhiteSpace(LastName);
            bool missingRole = string.IsNullOrWhiteSpace(Role);

            if (missingUsername || missingPassword || missingFirstName || missingLastName || missingRole)
            {
                string message = "Please fill in all fields." +
                                 $"\n\nUsername empty: {missingUsername}" +
                                 $"\nPassword empty: {missingPassword} (len={(passwordValue ?? string.Empty).Length})" +
                                 $"\nFirstName empty: {missingFirstName}" +
                                 $"\nLastName empty: {missingLastName}" +
                                 $"\nRole empty: {missingRole} (value='{Role ?? string.Empty}')";

                System.Windows.MessageBox.Show(
                    message,
                    "Create user",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Warning);
                return;
            }

            try
            {
                string roleCode = Role.ToUpper() switch
                {
                    "ADMINISTRATOR" => "ADMIN",
                    "MASTER" => "MASTER",
                    "USER" => "USER",
                    _ => throw new System.InvalidOperationException("Invalid role selected.")
                };

                _userService.RegisterUser(Username, passwordValue, FirstName, LastName, roleCode);

                Username = string.Empty;
                Password = string.Empty;
                FirstName = string.Empty;
                LastName = string.Empty;
                Role = "User";

                LoadUsersAndRoles();
            }
            catch (System.Exception ex)
            {
                System.Windows.MessageBox.Show($"Failed to create user: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private void LoadUsersAndRoles()
        {
            var roles = _userService.GetAllRoles();
            Roles.Clear();
            foreach (var r in roles)
                Roles.Add(r);

            var users = _userService.GetAllUsers();
            Users.Clear();
            foreach (var u in users)
                Users.Add(u);

            SyncSelectedUserRole();

            (UpdateUserRoleCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (DeleteUserCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }

        private void SyncSelectedUserRole()
        {
            if (SelectedUser == null || Roles.Count == 0)
            {
                SelectedUserRole = null;
                return;
            }

            string code = SelectedUser.Role?.Code ?? string.Empty;
            foreach (var r in Roles)
            {
                if (string.Equals(r.Code, code, System.StringComparison.OrdinalIgnoreCase))
                {
                    SelectedUserRole = r;
                    return;
                }
            }

            SelectedUserRole = null;
        }

        private bool CanDeleteUser()
        {
            return SelectedUser != null
                   && SelectedUser.UserId > 0
                   && SelectedUser.UserId != _currentUser.UserId;
        }

        private bool CanUpdateUserRole()
        {
            return SelectedUser != null && SelectedUserRole != null;
        }

        private void UpdateUserRole()
        {
            if (SelectedUser == null || SelectedUserRole == null) return;

            try
            {
                _userService.UpdateUserRole(SelectedUser.UserId, SelectedUserRole.Code);
                LoadUsersAndRoles();
                LoadData();
            }
            catch (System.Exception ex)
            {
                System.Windows.MessageBox.Show($"Failed to update user role: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private void AssignTechnician(object? obj)
        {
            if (SelectedRepairOrder == null || !SelectedTechnicianId.HasValue)
                return;

            try
            {
                _repairOrderService.AssignTechnician(SelectedRepairOrder.OrderId, SelectedTechnicianId.Value);
                LoadData(); // Refresh data
            }
            catch (System.Exception ex)
            {
                System.Windows.MessageBox.Show($"Failed to assign technician: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private void LoadData()
        {
            var orders = _repairOrderService.GetAllRepairOrders();
            RepairOrders.Clear();
            foreach (var order in orders)
            {
                RepairOrders.Add(order);
            }

            var technicians = _userService.GetUsersByRole("MASTER");
            Technicians.Clear();
            foreach (var tech in technicians)
            {
                if (tech != null)
                {
                    Technicians.Add(tech);
                }
            }

            (AssignTechnicianCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }
    }
}
