using BusinessLogic;
using Core;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace UI
{
    public class DashboardViewModel : ViewModelBase
    {
        private readonly User _currentUser;
        private readonly ClientService _clientService = new ClientService();
        private readonly RepairOrderService _repairOrderService = new RepairOrderService();

        private static string GetRoleCode(User user) => user.Role?.Code?.Trim() ?? string.Empty;

        public string Title { get; }
        public string Subtitle { get; }

        public bool IsAdmin { get; }
        public bool IsMaster { get; }
        public bool IsUser { get; }

        public int ClientsCount { get; private set; }
        public int OrdersCount { get; private set; }
        public ObservableCollection<Client> RecentClients { get; } = new ObservableCollection<Client>();
        public ObservableCollection<RepairOrder> RecentOrders { get; } = new ObservableCollection<RepairOrder>();

        public bool CanWorkWithClients { get; }
        public bool CanWorkWithDevices { get; }
        public bool CanWorkWithOrders { get; }

        public ICommand GoToAdminCommand { get; }
        public ICommand GoToMasterCommand { get; }
        public ICommand GoToClientsCommand { get; }
        public ICommand GoToDevicesCommand { get; }
        public ICommand GoToOrdersCommand { get; }

        public ICommand RefreshCommand { get; }

        public DashboardViewModel(
            User currentUser,
            ICommand goToAdminCommand,
            ICommand goToMasterCommand,
            ICommand goToClientsCommand,
            ICommand goToDevicesCommand,
            ICommand goToOrdersCommand)
        {
            _currentUser = currentUser;

            Title = $"Вітаю, {currentUser.FirstName} {currentUser.LastName}";
            string roleText = !string.IsNullOrWhiteSpace(currentUser.Role?.Name) ? currentUser.Role.Name : (currentUser.Role?.Code ?? string.Empty);
            Subtitle = $"Роль: {roleText}";

            string role = GetRoleCode(currentUser);
            IsAdmin = string.Equals(role, "ADMIN", System.StringComparison.OrdinalIgnoreCase);
            IsMaster = string.Equals(role, "MASTER", System.StringComparison.OrdinalIgnoreCase);
            IsUser = string.Equals(role, "USER", System.StringComparison.OrdinalIgnoreCase);

            CanWorkWithClients = IsAdmin || IsUser || IsMaster;
            CanWorkWithDevices = IsAdmin || IsUser;
            CanWorkWithOrders = IsAdmin || IsUser || IsMaster;

            GoToAdminCommand = goToAdminCommand;
            GoToMasterCommand = goToMasterCommand;
            GoToClientsCommand = goToClientsCommand;
            GoToDevicesCommand = goToDevicesCommand;
            GoToOrdersCommand = goToOrdersCommand;

            RefreshCommand = new RelayCommand(_ => Load());

            Load();
        }

        private void Load()
        {
            var clients = (IsAdmin || IsMaster)
                ? _clientService.GetAllClients()
                : _clientService.GetAllClients(_currentUser.UserId);
            ClientsCount = clients.Count;
            OnPropertyChanged(nameof(ClientsCount));
            RecentClients.Clear();
            for (int i = 0; i < clients.Count && i < 5; i++)
                RecentClients.Add(clients[i]);

            var orders = (IsAdmin || IsMaster)
                ? _repairOrderService.GetAllRepairOrders()
                : _repairOrderService.GetAllRepairOrders(_currentUser.UserId);
            OrdersCount = orders.Count;
            OnPropertyChanged(nameof(OrdersCount));
            RecentOrders.Clear();
            for (int i = 0; i < orders.Count && i < 5; i++)
                RecentOrders.Add(orders[i]);
        }
    }
}
