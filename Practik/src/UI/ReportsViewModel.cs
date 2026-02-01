using BusinessLogic;
using Core;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows.Input;

namespace UI
{
    public class ReportsViewModel : ViewModelBase
    {
        private readonly RepairOrderService _repairOrderService = new RepairOrderService();
        private readonly WorkReportService _workReportService = new WorkReportService();
        private readonly User _currentUser;

        private WorkActReportData? _reportData;

        public ObservableCollection<RepairOrder> CompletedOrders { get; } = new ObservableCollection<RepairOrder>();

        private RepairOrder? _selectedOrder;
        public RepairOrder? SelectedOrder
        {
            get => _selectedOrder;
            set
            {
                _selectedOrder = value;
                OnPropertyChanged();
                LoadReportData();
                (ExportCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        private static string Display(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? "-" : value.Trim();
        }

        private static string DisplayDate(DateTime? value)
        {
            return value.HasValue ? value.Value.ToString("yyyy-MM-dd HH:mm") : "-";
        }

        private static string IndentMultiline(string? value, string indent)
        {
            string v = value ?? string.Empty;
            v = v.Replace("\r\n", "\n").Replace("\r", "\n");
            if (string.IsNullOrWhiteSpace(v))
                return "-";

            var sb = new StringBuilder();
            using (var reader = new StringReader(v))
            {
                string? line;
                bool first = true;
                while ((line = reader.ReadLine()) != null)
                {
                    if (!first)
                        sb.AppendLine();
                    sb.Append(indent).Append(line);
                    first = false;
                }
            }
            return sb.ToString();
        }

        private string _workDoneDescription = string.Empty;
        public string WorkDoneDescription
        {
            get => _workDoneDescription;
            set
            {
                _workDoneDescription = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PreviewText));
            }
        }

        public string PreviewText
        {
            get
            {
                if (_reportData == null)
                    return string.Empty;

                return BuildPlainText(_reportData, WorkDoneDescription);
            }
        }

        public ICommand RefreshCommand { get; }
        public ICommand ExportCommand { get; }

        public ReportsViewModel(User currentUser)
        {
            _currentUser = currentUser;

            RefreshCommand = new RelayCommand(_ => Load());
            ExportCommand = new RelayCommand(_ => Export(), _ => CanExport());

            Load();
        }

        private static bool IsCompletedStatus(string? status)
        {
            string s = (status ?? string.Empty).Trim();
            return string.Equals(s, "DONE", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(s, "COMPLETED", StringComparison.OrdinalIgnoreCase);
        }

        private void Load()
        {
            CompletedOrders.Clear();

            string role = _currentUser.Role?.Code?.Trim() ?? string.Empty;

            var orders = (string.Equals(role, "ADMIN", StringComparison.OrdinalIgnoreCase)
                          || string.Equals(role, "MASTER", StringComparison.OrdinalIgnoreCase))
                ? _repairOrderService.GetAllRepairOrders()
                : _repairOrderService.GetAllRepairOrders(_currentUser.UserId);

            foreach (var order in orders)
            {
                if (!IsCompletedStatus(order.Status))
                    continue;

                CompletedOrders.Add(order);
            }

            if (SelectedOrder != null)
            {
                bool exists = false;
                foreach (var o in CompletedOrders)
                {
                    if (o.OrderId == SelectedOrder.OrderId)
                    {
                        exists = true;
                        break;
                    }
                }

                if (!exists)
                    SelectedOrder = null;
            }

            if (SelectedOrder != null)
                LoadReportData();

            (ExportCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }

        private void LoadReportData()
        {
            _reportData = null;

            if (SelectedOrder == null)
            {
                OnPropertyChanged(nameof(PreviewText));
                return;
            }

            try
            {
                _reportData = _repairOrderService.GetWorkActReportData(SelectedOrder.OrderId);
            }
            catch (System.Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "Report data", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }

            OnPropertyChanged(nameof(PreviewText));
        }

        private bool CanExport()
        {
            string role = _currentUser.Role?.Code?.Trim() ?? string.Empty;
            return (string.Equals(role, "ADMIN", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(role, "MASTER", StringComparison.OrdinalIgnoreCase))
                   && SelectedOrder != null;
        }

        private void Export()
        {
            if (SelectedOrder == null)
                return;

            if (_reportData == null)
                LoadReportData();

            if (_reportData == null)
                return;

            try
            {
                string reportContentText = BuildPlainText(_reportData, WorkDoneDescription);

                var dialog = new SaveFileDialog
                {
                    Title = "Save Work Act",
                    FileName = $"WorkAct_Order_{SelectedOrder.OrderId}",
                    DefaultExt = ".txt",
                    Filter = "Text file (*.txt)|*.txt|Markdown (*.md)|*.md|Excel CSV (*.csv)|*.csv|Word RTF (*.rtf)|*.rtf|All files (*.*)|*.*",
                    AddExtension = true,
                    OverwritePrompt = true
                };

                bool? result = dialog.ShowDialog();
                if (result != true)
                    return;

                string ext = (Path.GetExtension(dialog.FileName) ?? string.Empty).ToLowerInvariant();
                if (string.IsNullOrWhiteSpace(ext))
                {
                    string selectedExt = dialog.FilterIndex switch
                    {
                        2 => ".md",
                        3 => ".csv",
                        4 => ".rtf",
                        _ => ".txt"
                    };

                    dialog.FileName += selectedExt;
                    ext = selectedExt;
                }
                string fileContent = ext switch
                {
                    ".md" => BuildMarkdownWorkAct(_reportData, WorkDoneDescription),
                    ".csv" => BuildCsvWorkAct(_reportData, WorkDoneDescription),
                    ".rtf" => BuildRtfWorkAct(reportContentText),
                    _ => reportContentText
                };

                if (ext == ".rtf")
                {
                    File.WriteAllText(dialog.FileName, fileContent, Encoding.ASCII);
                }
                else
                {
                    File.WriteAllText(dialog.FileName, fileContent, Encoding.UTF8);
                }

                try
                {
                    _workReportService.CreateWorkReport(SelectedOrder.OrderId, reportContentText);
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
                System.Windows.MessageBox.Show(ex.Message, "Export failed", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private static string BuildPlainText(WorkActReportData data, string workDone)
        {
            var sb = new StringBuilder();
            sb.AppendLine("АКТ ВИКОНАНИХ РОБІТ");
            sb.AppendLine();

            sb.AppendLine("1. Замовлення");
            sb.AppendLine($"   1.1. №: {data.OrderId}");
            sb.AppendLine($"   1.2. Статус: {Display(data.CurrentStatus)}");
            sb.AppendLine();

            sb.AppendLine("2. Дати та відповідальні");
            sb.AppendLine($"   2.1. Дата прийому: {DisplayDate(data.DateReceived)}");
            sb.AppendLine($"   2.2. Прийнято: {Display(data.AcceptedBy)}");
            sb.AppendLine($"   2.3. Дата призначення: {DisplayDate(data.DateAssigned)}");
            sb.AppendLine($"   2.4. Призначив: {Display(data.AssignedBy)}");
            sb.AppendLine($"   2.5. Виконавець: {Display(data.Technician)}");
            sb.AppendLine($"   2.6. Дата завершення: {DisplayDate(data.DateCompleted)}");
            sb.AppendLine($"   2.7. Завершив: {Display(data.CompletedBy)}");
            sb.AppendLine();

            sb.AppendLine("3. Клієнт");
            sb.AppendLine($"   3.1. ПІБ: {Display($"{data.ClientFirstName} {data.ClientLastName}".Trim())}");
            sb.AppendLine($"   3.2. Телефон: {Display(data.ClientPhone)}");
            sb.AppendLine($"   3.3. Email: {Display(data.ClientEmail)}");
            sb.AppendLine();

            sb.AppendLine("4. Пристрій");
            sb.AppendLine($"   4.1. Тип: {Display(data.DeviceTypeName)}");
            sb.AppendLine($"   4.2. Виробник: {Display(data.Manufacturer)}");
            sb.AppendLine($"   4.3. Модель: {Display(data.Model)}");
            sb.AppendLine($"   4.4. Серійний №: {Display(data.SerialNumber)}");
            sb.AppendLine();

            sb.AppendLine("5. Несправність / проблема");
            sb.AppendLine($"   5.1. Опис:\n{IndentMultiline(data.ProblemDescription, "        ")}");
            sb.AppendLine();

            sb.AppendLine("6. Що було зроблено");
            sb.AppendLine($"   6.1. Опис:\n{IndentMultiline(workDone, "        ")}");

            return sb.ToString().TrimEnd();
        }

        private static string BuildMarkdownWorkAct(WorkActReportData data, string workDone)
        {
            string clientName = Display($"{data.ClientFirstName} {data.ClientLastName}".Trim());
            string problem = (data.ProblemDescription ?? string.Empty).Trim();
            string done = (workDone ?? string.Empty).Trim();

            return "# Акт виконаних робіт\n\n" +
                   "## 1. Замовлення\n" +
                   $"- **№:** {data.OrderId}\n" +
                   $"- **Статус:** {Display(data.CurrentStatus)}\n" +
                   "\n" +
                   "## 2. Дати та відповідальні\n" +
                   $"- **Дата прийому:** {DisplayDate(data.DateReceived)}\n" +
                   $"- **Прийнято:** {Display(data.AcceptedBy)}\n" +
                   $"- **Дата призначення:** {DisplayDate(data.DateAssigned)}\n" +
                   $"- **Призначив:** {Display(data.AssignedBy)}\n" +
                   $"- **Виконавець:** {Display(data.Technician)}\n" +
                   $"- **Дата завершення:** {DisplayDate(data.DateCompleted)}\n" +
                   $"- **Завершив:** {Display(data.CompletedBy)}\n" +
                   "\n" +
                   "## 3. Клієнт\n" +
                   $"- **ПІБ:** {clientName}\n" +
                   $"- **Телефон:** {Display(data.ClientPhone)}\n" +
                   $"- **Email:** {Display(data.ClientEmail)}\n" +
                   "\n" +
                   "## 4. Пристрій\n" +
                   $"- **Тип:** {Display(data.DeviceTypeName)}\n" +
                   $"- **Виробник:** {Display(data.Manufacturer)}\n" +
                   $"- **Модель:** {Display(data.Model)}\n" +
                   $"- **Серійний №:** {Display(data.SerialNumber)}\n" +
                   "\n" +
                   "## 5. Несправність / проблема\n" +
                   $"{(string.IsNullOrWhiteSpace(problem) ? "-" : problem)}\n" +
                   "\n" +
                   "## 6. Що було зроблено\n" +
                   $"{(string.IsNullOrWhiteSpace(done) ? "-" : done)}\n";
        }

        private static string BuildCsvWorkAct(WorkActReportData data, string workDone)
        {
            static string Csv(string? value)
            {
                string v = value ?? string.Empty;
                v = v.Replace("\r\n", "\n").Replace("\r", "\n");
                v = v.Replace("\"", "\"\"");
                return $"\"{v}\"";
            }

            static string ExcelText(string? value)
            {
                string v = value ?? string.Empty;
                v = v.Replace("\r\n", "\n").Replace("\r", "\n");
                v = v.Replace("\"", "\"\"");
                return $"=\"{v}\"";
            }

            string clientName = Display($"{data.ClientFirstName} {data.ClientLastName}".Trim());
            string problem = string.IsNullOrWhiteSpace(data.ProblemDescription) ? "-" : data.ProblemDescription.Trim();
            string done = string.IsNullOrWhiteSpace(workDone) ? "-" : workDone.Trim();

            const string sep = ";";
            var sb = new StringBuilder();
            sb.AppendLine($"Поле{sep}Значення");

            sb.AppendLine($"{Csv("Замовлення")}{sep}{Csv(string.Empty)}");
            sb.AppendLine($"{Csv("№")}{sep}{Csv(ExcelText(data.OrderId.ToString()))}");
            sb.AppendLine($"{Csv("Статус")}{sep}{Csv(Display(data.CurrentStatus))}");
            sb.AppendLine($"{Csv(string.Empty)}{sep}{Csv(string.Empty)}");

            sb.AppendLine($"{Csv("Дати та відповідальні")}{sep}{Csv(string.Empty)}");
            sb.AppendLine($"{Csv("Дата прийому")}{sep}{Csv(ExcelText(DisplayDate(data.DateReceived)))}");
            sb.AppendLine($"{Csv("Прийнято")}{sep}{Csv(Display(data.AcceptedBy))}");
            sb.AppendLine($"{Csv("Дата призначення")}{sep}{Csv(ExcelText(DisplayDate(data.DateAssigned)))}");
            sb.AppendLine($"{Csv("Призначив")}{sep}{Csv(Display(data.AssignedBy))}");
            sb.AppendLine($"{Csv("Виконавець")}{sep}{Csv(Display(data.Technician))}");
            sb.AppendLine($"{Csv("Дата завершення")}{sep}{Csv(ExcelText(DisplayDate(data.DateCompleted)))}");
            sb.AppendLine($"{Csv("Завершив")}{sep}{Csv(Display(data.CompletedBy))}");
            sb.AppendLine($"{Csv(string.Empty)}{sep}{Csv(string.Empty)}");

            sb.AppendLine($"{Csv("Клієнт")}{sep}{Csv(string.Empty)}");
            sb.AppendLine($"{Csv("ПІБ")}{sep}{Csv(clientName)}");
            sb.AppendLine($"{Csv("Телефон")}{sep}{Csv(ExcelText(Display(data.ClientPhone)))}");
            sb.AppendLine($"{Csv("Email")}{sep}{Csv(Display(data.ClientEmail))}");
            sb.AppendLine($"{Csv(string.Empty)}{sep}{Csv(string.Empty)}");

            sb.AppendLine($"{Csv("Пристрій")}{sep}{Csv(string.Empty)}");
            sb.AppendLine($"{Csv("Тип")}{sep}{Csv(Display(data.DeviceTypeName))}");
            sb.AppendLine($"{Csv("Виробник")}{sep}{Csv(Display(data.Manufacturer))}");
            sb.AppendLine($"{Csv("Модель")}{sep}{Csv(Display(data.Model))}");
            sb.AppendLine($"{Csv("Серійний №")}{sep}{Csv(ExcelText(Display(data.SerialNumber)))}");
            sb.AppendLine($"{Csv(string.Empty)}{sep}{Csv(string.Empty)}");

            sb.AppendLine($"{Csv("Несправність / проблема")}{sep}{Csv(string.Empty)}");
            sb.AppendLine($"{Csv("Опис")}{sep}{Csv(problem)}");
            sb.AppendLine($"{Csv(string.Empty)}{sep}{Csv(string.Empty)}");

            sb.AppendLine($"{Csv("Що було зроблено")}{sep}{Csv(string.Empty)}");
            sb.AppendLine($"{Csv("Опис")}{sep}{Csv(done)}");

            return sb.ToString();
        }

        private static string BuildRtfWorkAct(string plainText)
        {
            var sb = new StringBuilder();
            sb.Append("{\\rtf1\\ansi\\deff0{\\fonttbl{\\f0 Calibri;}}\\viewkind4\\uc1\\pard\\f0\\fs22 ");
            sb.Append(EscapeRtf(plainText));
            sb.Append("}");
            return sb.ToString();
        }

        private static string EscapeRtf(string value)
        {
            var sb = new StringBuilder(value.Length);

            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];

                if (c == '\\')
                {
                    sb.Append("\\\\");
                    continue;
                }

                if (c == '{')
                {
                    sb.Append("\\{");
                    continue;
                }

                if (c == '}')
                {
                    sb.Append("\\}");
                    continue;
                }

                if (c == '\r')
                {
                    if (i + 1 < value.Length && value[i + 1] == '\n')
                        i++;
                    sb.Append("\\par\n");
                    continue;
                }

                if (c == '\n')
                {
                    sb.Append("\\par\n");
                    continue;
                }

                if (c <= 0x7f)
                {
                    sb.Append(c);
                }
                else
                {
                    sb.Append("\\u").Append((short)c).Append('?');
                }
            }

            return sb.ToString();
        }
    }
}
