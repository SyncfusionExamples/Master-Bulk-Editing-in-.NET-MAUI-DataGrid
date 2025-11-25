using Syncfusion.Maui.DataGrid;
using System.Globalization;

namespace DataGridBulkEditSample
{
    public partial class MainPage : ContentPage
    {
        private MainViewModel vm;

        public MainPage()
        {
            InitializeComponent();
            vm = new MainViewModel();
            BindingContext = vm;
            bulkEditPopup.BindingContext = vm;

#if WINDOWS || MACCATALYST
            dataGrid.CellRightTapped += (s, e) => OpenBulkEditPopupWithSelection(e.Column.MappingName);
#else
            dataGrid.CellTapped += (s, e) => OpenBulkEditPopupWithSelection(e.Column.MappingName);
#endif
        }

        private void OpenBulkEditPopupWithSelection(string mappingName)
        {
            CommitCurrentEditIfAny();
            vm.SelectedRows = GetSelectedOrderInfos();
            vm.TargetMappingName = mappingName;
            vm.IsBulkPopupOpen = true;
        }

        private void CommitCurrentEditIfAny()
        {
            try
            {
                dataGrid.EndEdit();
            }
            catch
            {
                // As a fallback, try moving focus to commit editor implicitly
                dataGrid.Focus();
            }
        }

        private IList<OrderInfo> GetSelectedOrderInfos()
        {
            var result = new List<OrderInfo>();

            // Prefer SelectedRows; items can be model or DataGridRowInfo depending on version
            var rows = dataGrid.SelectedRows;
            if (rows != null)
            {
                foreach (var item in rows)
                {
                    if (item is OrderInfo o1)
                        result.Add(o1);
                    else if (item is DataGridRowInfo rowInfo && rowInfo.RowData is OrderInfo o2)
                        result.Add(o2);
                }
            }


            // Fallback for versions exposing SelectedItems
            var items = dataGrid.SelectedRows;
            if (items != null && result.Count == 0)
            {
                foreach (var item in items)
                {
                    if (item is OrderInfo o3)
                        result.Add(o3);
                    else if (item is DataGridRowInfo rowInfo && rowInfo.RowData is OrderInfo o4)
                        result.Add(o4);
                }
            }

            return result;
        }
    }
    public class BoolInverseConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    => value is bool b ? !b : value;


        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => value is bool b ? !b : value;

    }
}