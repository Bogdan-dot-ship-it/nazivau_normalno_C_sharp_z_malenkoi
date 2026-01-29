using System.Windows.Controls;

namespace UI
{
    public partial class MasterView : UserControl
    {
        public MasterView()
        {
            InitializeComponent();
        }

        private void DataGrid_LoadingRow(object sender, System.Windows.Controls.DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }
    }
}
