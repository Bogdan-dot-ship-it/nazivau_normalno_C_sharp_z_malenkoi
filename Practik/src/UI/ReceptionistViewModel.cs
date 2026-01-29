using BusinessLogic;
using Core;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace UI
{
    public class ReceptionistViewModel : ViewModelBase
    {
        private readonly ClientService _clientService = new ClientService();
        private readonly DeviceService _deviceService = new DeviceService();
        private readonly RepairOrderService _repairOrderService = new RepairOrderService();
        private readonly User _currentUser;

        public ObservableCollection<Client> Clients { get; } = new ObservableCollection<Client>();
        public ObservableCollection<DeviceType> DeviceTypes { get; } = new ObservableCollection<DeviceType>();
        public ObservableCollection<Device> Devices { get; } = new ObservableCollection<Device>();

        // Client properties
        private string _clientFirstName = string.Empty;
        public string ClientFirstName { get => _clientFirstName; set { _clientFirstName = value; OnPropertyChanged(); } }

        private string _clientLastName = string.Empty;
        public string ClientLastName { get => _clientLastName; set { _clientLastName = value; OnPropertyChanged(); } }

        private string _clientPhoneNumber = string.Empty;
        public string ClientPhoneNumber { get => _clientPhoneNumber; set { _clientPhoneNumber = value; OnPropertyChanged(); } }

        private string _clientEmail = string.Empty;
        public string ClientEmail { get => _clientEmail; set { _clientEmail = value; OnPropertyChanged(); } }

        // Device properties
        private Client? _selectedClient;
        public Client? SelectedClient { get => _selectedClient; set { _selectedClient = value; OnPropertyChanged(); (RegisterDeviceCommand as RelayCommand)?.RaiseCanExecuteChanged(); } }

        private DeviceType? _selectedDeviceType;
        public DeviceType? SelectedDeviceType { get => _selectedDeviceType; set { _selectedDeviceType = value; OnPropertyChanged(); (RegisterDeviceCommand as RelayCommand)?.RaiseCanExecuteChanged(); } }

        private string _deviceManufacturer = string.Empty;
        public string DeviceManufacturer { get => _deviceManufacturer; set { _deviceManufacturer = value; OnPropertyChanged(); (RegisterDeviceCommand as RelayCommand)?.RaiseCanExecuteChanged(); } }

        private string _deviceModel = string.Empty;
        public string DeviceModel { get => _deviceModel; set { _deviceModel = value; OnPropertyChanged(); (RegisterDeviceCommand as RelayCommand)?.RaiseCanExecuteChanged(); } }

        private string _deviceSerialNumber = string.Empty;
        public string DeviceSerialNumber { get => _deviceSerialNumber; set { _deviceSerialNumber = value; OnPropertyChanged(); } }

        // Repair Order properties
        private Device? _selectedOrderDevice;
        public Device? SelectedOrderDevice { get => _selectedOrderDevice; set { _selectedOrderDevice = value; OnPropertyChanged(); (CreateRepairOrderCommand as RelayCommand)?.RaiseCanExecuteChanged(); } }

        private string _problemDescription = string.Empty;
        public string ProblemDescription { get => _problemDescription; set { _problemDescription = value; OnPropertyChanged(); (CreateRepairOrderCommand as RelayCommand)?.RaiseCanExecuteChanged(); } }

        public ICommand RegisterClientCommand { get; }
        public ICommand RegisterDeviceCommand { get; }
        public ICommand CreateRepairOrderCommand { get; }
        public ICommand RefreshCommand { get; }

        public ReceptionistViewModel(User currentUser)
        {
            _currentUser = currentUser;
            RegisterClientCommand = new RelayCommand(RegisterClient, CanRegisterClient);
            RegisterDeviceCommand = new RelayCommand(RegisterDevice, CanRegisterDevice);
            CreateRepairOrderCommand = new RelayCommand(CreateRepairOrder, CanCreateRepairOrder);
            RefreshCommand = new RelayCommand(_ => Load());

            Load();
        }

        private bool CanRegisterClient(object? obj)
        {
            return !string.IsNullOrWhiteSpace(ClientFirstName) &&
                   !string.IsNullOrWhiteSpace(ClientLastName) &&
                   !string.IsNullOrWhiteSpace(ClientPhoneNumber);
        }

        private void RegisterClient(object? obj)
        {
            _clientService.CreateClient(_currentUser.UserId, ClientFirstName, ClientLastName, ClientPhoneNumber, ClientEmail);
            System.Windows.MessageBox.Show("Client registered successfully.");
            ClientFirstName = string.Empty;
            ClientLastName = string.Empty;
            ClientPhoneNumber = string.Empty;
            ClientEmail = string.Empty;

            Load();
        }

        private bool CanRegisterDevice(object? obj)
        {
            return SelectedClient != null &&
                   SelectedDeviceType != null &&
                   !string.IsNullOrWhiteSpace(DeviceManufacturer) &&
                   !string.IsNullOrWhiteSpace(DeviceModel);
        }

        private void RegisterDevice(object? obj)
        {
            if (SelectedClient == null || SelectedDeviceType == null) return;

            _deviceService.CreateDevice(_currentUser.UserId, SelectedClient.ClientId, SelectedDeviceType.Name, DeviceManufacturer, DeviceModel, DeviceSerialNumber);
            System.Windows.MessageBox.Show("Device registered successfully.");

            SelectedClient = null;
            SelectedDeviceType = null;
            DeviceManufacturer = string.Empty;
            DeviceModel = string.Empty;
            DeviceSerialNumber = string.Empty;

            Load();
        }

        private bool CanCreateRepairOrder(object? obj)
        {
            return SelectedOrderDevice != null && !string.IsNullOrWhiteSpace(ProblemDescription);
        }

        private void CreateRepairOrder(object? obj)
        {
            if (SelectedOrderDevice == null) return;

            _repairOrderService.CreateRepairOrder(SelectedOrderDevice.DeviceId, _currentUser.UserId, ProblemDescription);
            System.Windows.MessageBox.Show("Repair order created successfully.");
            SelectedOrderDevice = null;
            ProblemDescription = string.Empty;

            Load();
        }

        private void Load()
        {
            var clients = _clientService.GetAllClients(_currentUser.UserId);
            Clients.Clear();
            foreach (var c in clients)
                Clients.Add(c);

            var types = _deviceService.GetAllDeviceTypes();
            DeviceTypes.Clear();
            foreach (var t in types)
                DeviceTypes.Add(t);

            var devices = _deviceService.GetAllDevices(_currentUser.UserId);
            Devices.Clear();
            foreach (var d in devices)
                Devices.Add(d);
        }
    }
}
