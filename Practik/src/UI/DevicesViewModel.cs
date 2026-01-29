using BusinessLogic;
using Core;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace UI
{
    public class DevicesViewModel : ViewModelBase
    {
        private readonly DeviceService _deviceService = new DeviceService();
        private readonly ClientService _clientService = new ClientService();
        private readonly User _currentUser;

        public ObservableCollection<Device> Devices { get; } = new ObservableCollection<Device>();
        public ObservableCollection<DeviceType> DeviceTypes { get; } = new ObservableCollection<DeviceType>();
        public ObservableCollection<Client> Clients { get; } = new ObservableCollection<Client>();

        private Device? _selectedDevice;
        public Device? SelectedDevice
        {
            get => _selectedDevice;
            set
            {
                _selectedDevice = value;
                OnPropertyChanged();
                SyncFormWithSelectedDevice();
                (UpdateDeviceCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (DeleteDeviceCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        private Client? _selectedClient;
        public Client? SelectedClient
        {
            get => _selectedClient;
            set
            {
                _selectedClient = value;
                OnPropertyChanged();
                (AddDeviceCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (UpdateDeviceCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        private DeviceType? _selectedDeviceType;
        public DeviceType? SelectedDeviceType
        {
            get => _selectedDeviceType;
            set
            {
                _selectedDeviceType = value;
                OnPropertyChanged();
                (AddDeviceCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (UpdateDeviceCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        private string _manufacturer = string.Empty;
        public string Manufacturer
        {
            get => _manufacturer;
            set
            {
                _manufacturer = value;
                OnPropertyChanged();
                (AddDeviceCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (UpdateDeviceCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        private string _model = string.Empty;
        public string Model
        {
            get => _model;
            set
            {
                _model = value;
                OnPropertyChanged();
                (AddDeviceCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (UpdateDeviceCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        private string _serialNumber = string.Empty;
        public string SerialNumber
        {
            get => _serialNumber;
            set
            {
                _serialNumber = value;
                OnPropertyChanged();
                (UpdateDeviceCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public ICommand RefreshCommand { get; }
        public ICommand AddDeviceCommand { get; }
        public ICommand UpdateDeviceCommand { get; }
        public ICommand DeleteDeviceCommand { get; }

        public DevicesViewModel(User currentUser)
        {
            _currentUser = currentUser;
            RefreshCommand = new RelayCommand(_ => Load());
            AddDeviceCommand = new RelayCommand(_ => AddDevice(), _ => CanAddDevice());
            UpdateDeviceCommand = new RelayCommand(_ => UpdateDevice(), _ => CanUpdateDevice());
            DeleteDeviceCommand = new RelayCommand(_ => DeleteDevice(), _ => CanDeleteDevice());
            Load();
        }

        private void UpdateDevice()
        {
            if (SelectedDevice == null || SelectedDeviceType == null || SelectedClient == null)
                return;

            try
            {
                string role = _currentUser.Role?.Code?.Trim() ?? string.Empty;
                bool isAdmin = string.Equals(role, "ADMIN", System.StringComparison.OrdinalIgnoreCase);

                int ownerUserId = isAdmin ? SelectedClient.OwnerUserId : _currentUser.UserId;

                if (isAdmin)
                {
                    _deviceService.UpdateDeviceById(
                        SelectedDevice.DeviceId,
                        ownerUserId,
                        SelectedClient.ClientId,
                        SelectedDeviceType.Name,
                        Manufacturer,
                        Model,
                        SerialNumber);
                }
                else
                {
                    _deviceService.UpdateDevice(
                        ownerUserId,
                        SelectedDevice.DeviceId,
                        SelectedClient.ClientId,
                        SelectedDeviceType.Name,
                        Manufacturer,
                        Model,
                        SerialNumber);
                }

                Load();
            }
            catch (System.Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "Update device failed", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private void DeleteDevice()
        {
            if (SelectedDevice == null)
                return;

            try
            {
                string role = _currentUser.Role?.Code?.Trim() ?? string.Empty;
                bool isAdmin = string.Equals(role, "ADMIN", System.StringComparison.OrdinalIgnoreCase);

                if (isAdmin)
                {
                    _deviceService.DeleteDeviceById(SelectedDevice.DeviceId);
                }
                else
                {
                    _deviceService.DeleteDevice(_currentUser.UserId, SelectedDevice.DeviceId);
                }

                SelectedDevice = null;
                Load();
            }
            catch (System.Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "Delete device failed", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private void SyncFormWithSelectedDevice()
        {
            if (SelectedDevice == null)
                return;

            Manufacturer = SelectedDevice.Manufacturer;
            Model = SelectedDevice.Model;
            SerialNumber = SelectedDevice.SerialNumber;

            foreach (var c in Clients)
            {
                if (c.ClientId == SelectedDevice.ClientId)
                {
                    SelectedClient = c;
                    break;
                }
            }

            foreach (var t in DeviceTypes)
            {
                if (string.Equals(t.Name, SelectedDevice.DeviceTypeName, System.StringComparison.OrdinalIgnoreCase))
                {
                    SelectedDeviceType = t;
                    break;
                }
            }
        }

        private bool CanAddDevice()
        {
            return SelectedClient != null
                   && SelectedDeviceType != null
                   && !string.IsNullOrWhiteSpace(Manufacturer)
                   && !string.IsNullOrWhiteSpace(Model);
        }

        private bool CanUpdateDevice()
        {
            return SelectedDevice != null && CanAddDevice();
        }

        private bool CanDeleteDevice()
        {
            return SelectedDevice != null;
        }

        private void AddDevice()
        {
            if (SelectedDeviceType == null || SelectedClient == null) return;

            string role = _currentUser.Role?.Code?.Trim() ?? string.Empty;
            int ownerUserId = string.Equals(role, "ADMIN", System.StringComparison.OrdinalIgnoreCase)
                ? SelectedClient.OwnerUserId
                : _currentUser.UserId;

            _deviceService.CreateDevice(ownerUserId, SelectedClient.ClientId, SelectedDeviceType.Name, Manufacturer, Model, SerialNumber);

            SelectedClient = null;
            SelectedDeviceType = null;
            Manufacturer = string.Empty;
            Model = string.Empty;
            SerialNumber = string.Empty;

            Load();
        }

        private void Load()
        {
            var types = _deviceService.GetAllDeviceTypes();
            DeviceTypes.Clear();
            foreach (var t in types)
                DeviceTypes.Add(t);

            string role = _currentUser.Role?.Code?.Trim() ?? string.Empty;
            var clients = string.Equals(role, "ADMIN", System.StringComparison.OrdinalIgnoreCase)
                ? _clientService.GetAllClients()
                : _clientService.GetAllClients(_currentUser.UserId);
            Clients.Clear();
            foreach (var c in clients)
                Clients.Add(c);

            var devices = string.Equals(role, "ADMIN", System.StringComparison.OrdinalIgnoreCase)
                ? _deviceService.GetAllDevices()
                : _deviceService.GetAllDevices(_currentUser.UserId);
            Devices.Clear();
            foreach (var d in devices)
                Devices.Add(d);

            (AddDeviceCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (UpdateDeviceCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (DeleteDeviceCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }
    }
}
