using Syncfusion.Maui.DataGrid;
using System.Globalization;

namespace DataGridBulkEditSample
{
    public partial class MainPage : ContentPage
    {
        private MainViewModel vm;

        /// <summary>
        /// Initializes the page, sets up bindings, and attaches gesture handlers
        /// based on the current platform.
        /// </summary>
        public MainPage()
        {
            InitializeComponent();

            // Bind page & popup to the same ViewModel to share selection and edit context
            vm = new MainViewModel();
            BindingContext = vm;
            bulkEditPopup.BindingContext = vm;

#if WINDOWS || MACCATALYST
            // Desktop: right-tap opens the bulk edit popup for the tapped column
            dataGrid.CellRightTapped += (s, e) => OpenBulkEditPopupWithSelection(e.Column.MappingName);
#else
            // Mobile (Android/iOS): long press opens the bulk edit popup for the pressed column
            dataGrid.CellLongPress += (s, e) => OpenBulkEditPopupWithSelection(e.Column.MappingName);
#endif
        }

        /// <summary>
        /// Commits any active in-cell edit, captures current selection,
        /// sets target column mapping, and opens the bulk edit popup.
        /// </summary>
        /// <param name="mappingName">The data column mapping name (e.g., property name) that was tapped/pressed.</param>
        private void OpenBulkEditPopupWithSelection(string mappingName)
        {
            CommitCurrentEditIfAny();
            vm.SelectedRows = GetSelectedOrderInfos();
            vm.TargetMappingName = mappingName;
            vm.IsBulkPopupOpen = true;
        }

        /// <summary>
        /// Attempts to finalize any current grid cell edit so the underlying model
        /// reflects changes before bulk edits are applied.
        /// </summary>
        private void CommitCurrentEditIfAny()
        {
            try
            {
                dataGrid.EndEdit();
            }
            catch
            {
                dataGrid.Focus();
            }
        }

        /// <summary>
        /// Returns a strongly-typed list of selected <see cref="OrderInfo"/> models.
        /// Handles differences in Syncfusion DataGrid versions where selection may expose
        /// model instances directly or via <c>DataGridRowInfo</c>.
        /// </summary>
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

            if (result.Count == 0)
            {
                var items = dataGrid.SelectedRows;
                if (items != null)
                {
                    foreach (var item in items)
                    {
                        if (item is OrderInfo o3)
                            result.Add(o3);
                        else if (item is DataGridRowInfo rowInfo && rowInfo.RowData is OrderInfo o4)
                            result.Add(o4);
                    }
                }
            }

            return result;
        }
    }

    /// <summary>
    /// Inverts boolean values for XAML bindings.
    /// Useful for toggling visibility or enabled states (e.g., show label when condition is false).
    /// </summary>
    public class BoolInverseConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => value is bool b ? !b : value;

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => value is bool b ? !b : value;
    }
}