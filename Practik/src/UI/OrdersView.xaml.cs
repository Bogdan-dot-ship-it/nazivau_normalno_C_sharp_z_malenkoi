namespace UI
{
    public partial class OrdersView
    {
        public OrdersView()
        {
            InitializeComponent();
        }

        private void DataGrid_LoadingRow(object sender, System.Windows.Controls.DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }

        private void OrdersGrid_RowEditEnding(object sender, System.Windows.Controls.DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction != System.Windows.Controls.DataGridEditAction.Commit)
                return;

            if (DataContext is OrdersViewModel viewModel && e.Row?.Item is Core.RepairOrder order)
            {
                Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    viewModel.SaveProblemDescription(order);
                }), System.Windows.Threading.DispatcherPriority.Background);
            }
        }
    }
}
