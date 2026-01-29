using BusinessLogic;
using Core;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace UI
{
    public class MasterViewModel : ViewModelBase
    {
        private readonly RepairOrderService _repairOrderService = new RepairOrderService();
        private readonly RepairLogService _repairLogService = new RepairLogService();
        private readonly StatusService _statusService = new StatusService();
        private readonly User _currentUser;

        private RepairOrder? _selectedOrder;
        public RepairOrder? SelectedOrder { get => _selectedOrder; set { _selectedOrder = value; OnPropertyChanged(); } }

        private string _newStatus = string.Empty;
        public string NewStatus { get => _newStatus; set { _newStatus = value; OnPropertyChanged(); } }

        public ObservableCollection<RepairOrder> RepairOrders { get; set; }
        public ObservableCollection<string> Statuses { get; set; }

        public ICommand UpdateStatusCommand { get; }
        public ICommand MarkCompletedCommand { get; }

        public MasterViewModel(User currentUser)
        {
            _currentUser = currentUser;
            RepairOrders = new ObservableCollection<RepairOrder>();
            Statuses = new ObservableCollection<string>();
            UpdateStatusCommand = new RelayCommand(UpdateStatus, CanUpdateStatus);
            MarkCompletedCommand = new RelayCommand(MarkCompleted, CanMarkCompleted);
            LoadRepairOrders();
            LoadStatuses();
        }

        private bool CanUpdateStatus(object? obj)
        {
            return SelectedOrder != null && !string.IsNullOrEmpty(NewStatus);
        }

        private bool CanMarkCompleted(object? obj)
        {
            return SelectedOrder != null;
        }

        private void UpdateStatus(object? obj)
        {
            if (SelectedOrder == null) return;

            _repairOrderService.UpdateRepairStatus(SelectedOrder.OrderId, NewStatus, _currentUser.UserId);
            _repairLogService.CreateRepairLog(SelectedOrder.OrderId, _currentUser.UserId, $"Status updated to {NewStatus}");

            if (NewStatus == "DONE" || NewStatus == "COMPLETED")
            {
                var workReportService = new WorkReportService();
                string reportContent = $"Work report for order {SelectedOrder.OrderId}.\nProblem: {SelectedOrder.ProblemDescription}\nStatus: Completed";
                workReportService.CreateWorkReport(SelectedOrder.OrderId, reportContent);

                var backupService = new BackupService();
                var backupResult = backupService.CreateBackup();
                if (backupResult.Success)
                {
                }
                else
                {
                    System.Windows.MessageBox.Show(backupResult.Message, "Backup Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }

            LoadRepairOrders(); // Refresh the list
        }

        private void MarkCompleted(object? obj)
        {
            if (SelectedOrder == null) return;

            NewStatus = "DONE";
            UpdateStatus(obj);
        }

        private void LoadStatuses()
        {
            var statuses = _statusService.GetAllStatuses();
            Statuses.Clear();
            foreach (var status in statuses)
            {
                Statuses.Add(status);
            }
        }

        private void LoadRepairOrders()
        {
            var orders = _repairOrderService.GetRepairOrdersByTechnician(_currentUser.UserId);
            RepairOrders.Clear();
            foreach (var order in orders)
            {
                RepairOrders.Add(order);
            }
        }
    }
}
