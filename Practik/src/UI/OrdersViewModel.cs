using BusinessLogic;
using Core;
using Microsoft.Win32;
using System.IO;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace UI
{
    public class OrdersViewModel : ViewModelBase
    {
        private readonly RepairOrderService _repairOrderService = new RepairOrderService();
        private readonly DeviceService _deviceService = new DeviceService();
        private readonly StatusService _statusService = new StatusService();
        private readonly WorkReportService _workReportService = new WorkReportService();
        private readonly User _currentUser;

        public ObservableCollection<RepairOrder> Orders { get; } = new ObservableCollection<RepairOrder>();
        public ObservableCollection<Device> Devices { get; } = new ObservableCollection<Device>();
        public ObservableCollection<string> Statuses { get; } = new ObservableCollection<string>();

        private RepairOrder? _selectedOrder;
        public RepairOrder? SelectedOrder
        {
            get => _selectedOrder;
            set
            {
                _selectedOrder = value;
                OnPropertyChanged();
                (DeleteOrderCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (UpdateStatusCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (MarkCompletedCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (CreateWorkActCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public void SaveProblemDescription(RepairOrder order)
        {
            if (order == null)
                return;

            string role = _currentUser.Role?.Code?.Trim() ?? string.Empty;
            bool canEdit = string.Equals(role, "ADMIN", System.StringComparison.OrdinalIgnoreCase)
                           || string.Equals(role, "MASTER", System.StringComparison.OrdinalIgnoreCase)
                           || string.Equals(role, "USER", System.StringComparison.OrdinalIgnoreCase);

            if (!canEdit)
                return;

            try
            {
                bool updated = _repairOrderService.UpdateProblemDescription(order.OrderId, order.ProblemDescription);
                if (!updated)
                {
                    System.Windows.MessageBox.Show(
                        "Failed to update problem description.",
                        "Update failed",
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Warning);
                }
            }
            catch (System.Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "Update problem failed", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private void MarkCompleted()
        {
            SelectedStatus = "DONE";
            UpdateStatus();
        }

        private string _selectedStatus = string.Empty;
        public string SelectedStatus
        {
            get => _selectedStatus;
            set
            {
                _selectedStatus = value;
                OnPropertyChanged();
                (UpdateStatusCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        private Device? _selectedDevice;
        public Device? SelectedDevice { get => _selectedDevice; set { _selectedDevice = value; OnPropertyChanged(); (CreateOrderCommand as RelayCommand)?.RaiseCanExecuteChanged(); } }

        private string _problemDescription = string.Empty;
        public string ProblemDescription { get => _problemDescription; set { _problemDescription = value; OnPropertyChanged(); (CreateOrderCommand as RelayCommand)?.RaiseCanExecuteChanged(); } }

        public ICommand RefreshCommand { get; }
        public ICommand CreateOrderCommand { get; }
        public ICommand DeleteOrderCommand { get; }
        public ICommand UpdateStatusCommand { get; }
        public ICommand MarkCompletedCommand { get; }
        public ICommand CreateWorkActCommand { get; }

        public OrdersViewModel(User currentUser)
        {
            _currentUser = currentUser;
            RefreshCommand = new RelayCommand(_ => Load());
            CreateOrderCommand = new RelayCommand(_ => CreateOrder(), _ => CanCreateOrder());
            DeleteOrderCommand = new RelayCommand(_ => DeleteOrder(), _ => CanDeleteOrder());
            UpdateStatusCommand = new RelayCommand(_ => UpdateStatus(), _ => CanUpdateStatus());
            MarkCompletedCommand = new RelayCommand(_ => MarkCompleted(), _ => CanMarkCompleted());
            CreateWorkActCommand = new RelayCommand(_ => CreateWorkAct(), _ => CanCreateWorkAct());
            Load();
        }

        private bool CanCreateWorkAct()
        {
            string role = _currentUser.Role?.Code?.Trim() ?? string.Empty;
            return (string.Equals(role, "MASTER", System.StringComparison.OrdinalIgnoreCase)
                    || string.Equals(role, "ADMIN", System.StringComparison.OrdinalIgnoreCase))
                   && SelectedOrder != null
                   && (string.Equals(SelectedOrder.Status?.Trim(), "DONE", System.StringComparison.OrdinalIgnoreCase)
                       || string.Equals(SelectedOrder.Status?.Trim(), "COMPLETED", System.StringComparison.OrdinalIgnoreCase));
        }

        private void CreateWorkAct()
        {
            if (SelectedOrder == null)
                return;

            try
            {
                string reportContent =
                    $"Work act for order {SelectedOrder.OrderId}.\n" +
                    $"Client: {SelectedOrder.ClientName}\n" +
                    $"Device: {SelectedOrder.DeviceTypeName} {SelectedOrder.Manufacturer} {SelectedOrder.Model}\n" +
                    $"Technician: {SelectedOrder.TechnicianName}\n" +
                    $"Problem: {SelectedOrder.ProblemDescription}\n" +
                    $"Status: {SelectedOrder.Status}";

                var dialog = new SaveFileDialog
                {
                    Title = "Save Work Act",
                    FileName = $"WorkAct_Order_{SelectedOrder.OrderId}",
                    DefaultExt = ".txt",
                    Filter = "Text file (*.txt)|*.txt|Markdown (*.md)|*.md|All files (*.*)|*.*",
                    AddExtension = true,
                    OverwritePrompt = true
                };

                bool? result = dialog.ShowDialog();
                if (result != true)
                    return;

                File.WriteAllText(dialog.FileName, reportContent);

                try
                {
                    _workReportService.CreateWorkReport(SelectedOrder.OrderId, reportContent);
                }
                catch (System.Exception ex)
                {
                    System.Windows.MessageBox.Show(
                        $"File saved to: {dialog.FileName}\n\nNote: {ex.Message}",
                        "Work act exported",
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Warning);
                    return;
                }

                System.Windows.MessageBox.Show(
                    $"File saved to: {dialog.FileName}",
                    "Work act exported",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information);
            }
            catch (System.Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "Create work act failed", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private bool CanCreateOrder()
        {
            string role = _currentUser.Role?.Code?.Trim() ?? string.Empty;
            return (string.Equals(role, "USER", System.StringComparison.OrdinalIgnoreCase)
                    || string.Equals(role, "ADMIN", System.StringComparison.OrdinalIgnoreCase))
                   && SelectedDevice != null
                   && !string.IsNullOrWhiteSpace(ProblemDescription);
        }

        private bool CanDeleteOrder()
        {
            string role = _currentUser.Role?.Code?.Trim() ?? string.Empty;
            return (string.Equals(role, "USER", System.StringComparison.OrdinalIgnoreCase)
                    || string.Equals(role, "ADMIN", System.StringComparison.OrdinalIgnoreCase))
                   && SelectedOrder != null;
        }

        private bool CanUpdateStatus()
        {
            string role = _currentUser.Role?.Code?.Trim() ?? string.Empty;
            return (string.Equals(role, "ADMIN", System.StringComparison.OrdinalIgnoreCase)
                    || string.Equals(role, "MASTER", System.StringComparison.OrdinalIgnoreCase))
                   && SelectedOrder != null
                   && !string.IsNullOrWhiteSpace(SelectedStatus);
        }

        private bool CanMarkCompleted()
        {
            string role = _currentUser.Role?.Code?.Trim() ?? string.Empty;
            return (string.Equals(role, "MASTER", System.StringComparison.OrdinalIgnoreCase)
                    || string.Equals(role, "ADMIN", System.StringComparison.OrdinalIgnoreCase))
                   && SelectedOrder != null;
        }

        private void CreateOrder()
        {
            if (SelectedDevice == null) return;

            try
            {
                _repairOrderService.CreateRepairOrder(SelectedDevice.DeviceId, _currentUser.UserId, ProblemDescription);

                SelectedDevice = null;
                ProblemDescription = string.Empty;

                Load();
            }
            catch (System.Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "Create order failed", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private void DeleteOrder()
        {
            if (SelectedOrder == null)
                return;

            string role = _currentUser.Role?.Code?.Trim() ?? string.Empty;

            try
            {
                if (string.Equals(role, "ADMIN", System.StringComparison.OrdinalIgnoreCase))
                {
                    _repairOrderService.DeleteRepairOrderById(SelectedOrder.OrderId);
                }
                else
                {
                    _repairOrderService.DeleteRepairOrder(_currentUser.UserId, SelectedOrder.OrderId);
                }

                SelectedOrder = null;
                Load();
            }
            catch (System.Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "Delete order failed", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private void UpdateStatus()
        {
            if (SelectedOrder == null)
                return;

            try
            {
                _repairOrderService.UpdateRepairStatus(SelectedOrder.OrderId, SelectedStatus, _currentUser.UserId);
                Load();
            }
            catch (System.Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "Update status failed", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private void Load()
        {
            string role = _currentUser.Role?.Code?.Trim() ?? string.Empty;

            var statuses = _statusService.GetAllStatuses();
            Statuses.Clear();
            foreach (var s in statuses)
                Statuses.Add(s);

            var devices = string.Equals(role, "ADMIN", System.StringComparison.OrdinalIgnoreCase)
                ? _deviceService.GetAllDevices()
                : _deviceService.GetAllDevices(_currentUser.UserId);
            Devices.Clear();
            foreach (var d in devices)
                Devices.Add(d);

            var data = (string.Equals(role, "ADMIN", System.StringComparison.OrdinalIgnoreCase)
                        || string.Equals(role, "MASTER", System.StringComparison.OrdinalIgnoreCase))
                ? _repairOrderService.GetAllRepairOrders()
                : _repairOrderService.GetAllRepairOrders(_currentUser.UserId);
            Orders.Clear();
            foreach (var o in data)
                Orders.Add(o);

            (CreateOrderCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (DeleteOrderCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (UpdateStatusCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (MarkCompletedCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (CreateWorkActCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }
    }
}
