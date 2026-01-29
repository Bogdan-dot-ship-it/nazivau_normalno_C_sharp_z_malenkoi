using BusinessLogic;
using Core;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace UI
{
    public class ClientsViewModel : ViewModelBase
    {
        private readonly ClientService _clientService = new ClientService();
        private readonly User _currentUser;

        public ObservableCollection<Client> Clients { get; } = new ObservableCollection<Client>();

        private Client? _selectedClient;
        public Client? SelectedClient
        {
            get => _selectedClient;
            set
            {
                _selectedClient = value;
                OnPropertyChanged();

                if (_selectedClient != null)
                {
                    FirstName = _selectedClient.FirstName;
                    LastName = _selectedClient.LastName;
                    PhoneNumber = _selectedClient.PhoneNumber;
                    Email = _selectedClient.Email;
                }

                (UpdateClientCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (DeleteClientCommand as RelayCommand)?.RaiseCanExecuteChanged();
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
                (AddClientCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (UpdateClientCommand as RelayCommand)?.RaiseCanExecuteChanged();
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
                (AddClientCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (UpdateClientCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        private string _phoneNumber = string.Empty;
        public string PhoneNumber
        {
            get => _phoneNumber;
            set
            {
                _phoneNumber = value;
                OnPropertyChanged();
                (AddClientCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (UpdateClientCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        private string _email = string.Empty;
        public string Email
        {
            get => _email;
            set
            {
                _email = value;
                OnPropertyChanged();
                (UpdateClientCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public ICommand RefreshCommand { get; }
        public ICommand AddClientCommand { get; }
        public ICommand UpdateClientCommand { get; }
        public ICommand DeleteClientCommand { get; }

        public ClientsViewModel(User currentUser)
        {
            _currentUser = currentUser;
            RefreshCommand = new RelayCommand(_ => Load());
            AddClientCommand = new RelayCommand(_ => AddClient(), _ => CanAddClient());
            UpdateClientCommand = new RelayCommand(_ => UpdateClient(), _ => CanUpdateClient());
            DeleteClientCommand = new RelayCommand(_ => DeleteClient(), _ => CanDeleteClient());
            Load();
        }

        private bool CanUpdateClient()
        {
            string role = _currentUser.Role?.Code?.Trim() ?? string.Empty;
            if (string.Equals(role, "MASTER", System.StringComparison.OrdinalIgnoreCase))
                return false;

            return SelectedClient != null && CanAddClient();
        }

        private void UpdateClient()
        {
            if (SelectedClient == null) return;

            string role = _currentUser.Role?.Code?.Trim() ?? string.Empty;
            int ownerUserId = string.Equals(role, "ADMIN", System.StringComparison.OrdinalIgnoreCase)
                ? SelectedClient.OwnerUserId
                : _currentUser.UserId;

            _clientService.UpdateClient(ownerUserId, SelectedClient.ClientId, FirstName, LastName, PhoneNumber, Email);
            Load();
        }

        private bool CanDeleteClient()
        {
            string role = _currentUser.Role?.Code?.Trim() ?? string.Empty;
            if (string.Equals(role, "MASTER", System.StringComparison.OrdinalIgnoreCase))
                return false;

            return SelectedClient != null;
        }

        private void DeleteClient()
        {
            if (SelectedClient == null) return;

            string role = _currentUser.Role?.Code?.Trim() ?? string.Empty;
            int ownerUserId = string.Equals(role, "ADMIN", System.StringComparison.OrdinalIgnoreCase)
                ? SelectedClient.OwnerUserId
                : _currentUser.UserId;

            _clientService.DeleteClient(ownerUserId, SelectedClient.ClientId);
            SelectedClient = null;

            FirstName = string.Empty;
            LastName = string.Empty;
            PhoneNumber = string.Empty;
            Email = string.Empty;

            Load();
        }

        private bool CanAddClient()
        {
            string role = _currentUser.Role?.Code?.Trim() ?? string.Empty;
            if (string.Equals(role, "MASTER", System.StringComparison.OrdinalIgnoreCase))
                return false;

            return !string.IsNullOrWhiteSpace(FirstName)
                   && !string.IsNullOrWhiteSpace(LastName)
                   && !string.IsNullOrWhiteSpace(PhoneNumber);
        }

        private void AddClient()
        {
            _clientService.CreateClient(_currentUser.UserId, FirstName, LastName, PhoneNumber, Email);

            FirstName = string.Empty;
            LastName = string.Empty;
            PhoneNumber = string.Empty;
            Email = string.Empty;

            Load();
        }

        private void Load()
        {
            string role = _currentUser.Role?.Code?.Trim() ?? string.Empty;
            var data = (string.Equals(role, "ADMIN", System.StringComparison.OrdinalIgnoreCase)
                        || string.Equals(role, "MASTER", System.StringComparison.OrdinalIgnoreCase))
                ? _clientService.GetAllClients()
                : _clientService.GetAllClients(_currentUser.UserId);
            Clients.Clear();
            foreach (var c in data)
                Clients.Add(c);

            (UpdateClientCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (DeleteClientCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }
    }
}
