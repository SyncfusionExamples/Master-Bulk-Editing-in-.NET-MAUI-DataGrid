using Syncfusion.Maui.DataGrid;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.ComponentModel;
using System.Collections;

namespace DataGridBulkEditSample
{
    public partial class MainPage : ContentPage
    {
        private MainViewModel viewModel;

        public MainPage()
        {
            InitializeComponent();
            viewModel = new MainViewModel();
            BindingContext = viewModel;
            bulkEditPopup.BindingContext = viewModel;
            viewModel.PropertyChanged += ViewModelOnPropertyChanged;
            UpdateGridSelectionUnit(viewModel.IsCellSelectionMode);

#if WINDOWS || MACCATALYST
// Desktop: right-click opens the bulk edit popup.
dataGrid.CellRightTapped += (s, e) =>
{
    var mapping = e.Column?.MappingName
                  ?? ResolveMappingNameFromEventArgs(e)
                  ?? GetFirstSelectedMappingName()
                  ?? "Status";

    var rows = GetSelectedOrderInfosFromSelectedCells(mapping);
    if (rows.Count == 0)
    {
        rows = GetSelectedOrderInfos().ToList();
    }

    viewModel.SelectedRows = rows;
    OpenPopupBasedOnMode(mapping);
};
#else
            // Mobile (Android/iOS): long press opens the popup
            dataGrid.CellLongPress += (s, e) => OpenPopupBasedOnMode(e.Column?.MappingName);
#endif
        }

        /// <summary>
        /// Opens the popup depending on the chosen selection mode.
        /// Cell mode: uses mapping name for single-column edit across selected rows.
        /// Row mode: shows dialog to edit entire row, seeded from the first selected row.
        /// </summary>
        private void OpenPopupBasedOnMode(string? mappingName)
        {
            CommitCurrentEditIfAny();

            if (viewModel.IsCellSelectionMode)
            {
                if (string.IsNullOrEmpty(mappingName))
                {
                    mappingName = GetFirstSelectedMappingName() ?? viewModel.TargetMappingName;
                }

                if (string.IsNullOrEmpty(mappingName))
                {
                    return;
                }

                viewModel.TargetMappingName = mappingName;
                viewModel.SelectedRows = GetSelectedOrderInfosFromSelectedCells(mappingName);
                if (viewModel.SelectedRows.Count > 0)
                {
                    viewModel.IsBulkPopupOpen = true;
                }
            }
            else
            {
                viewModel.SelectedRows = GetSelectedOrderInfos();
                var first = viewModel.SelectedRows.FirstOrDefault();
                if (first != null)
                {
                    viewModel.EditableOrder = new OrderInfo
                    {
                        EmployeeID = first.EmployeeID,
                        CustomerName = first.CustomerName,
                        Country = first.Country,
                        Status = first.Status,
                        Date = first.Date,
                        Branch = first.Branch
                    };
                    viewModel.IsBulkPopupOpen = true;
                }
            }
        }

        /// <summary>
        /// Resolves the mapping name of the column from the event arguments.
        /// </summary>
        /// <param name="eventArgs"></param>
        /// <returns></returns>
        private string? ResolveMappingNameFromEventArgs(object eventArgs)
        {
            try
            {
                var argType = eventArgs.GetType();
                var rciProp = argType.GetProperty("RowColumnIndex");
                var rci = rciProp?.GetValue(eventArgs);
                if (rci == null)
                {
                    return null;
                }

                var colIndexProp = rci.GetType().GetProperty("ColumnIndex");
                if (colIndexProp == null)
                {
                    return null;
                }

                var colIndexObj = colIndexProp.GetValue(rci);
                if (colIndexObj is int colIndex && colIndex >= 0)
                {
                    var columnsProp = dataGrid.GetType().GetProperty("Columns");
                    var columns = columnsProp?.GetValue(dataGrid) as System.Collections.IList;
                    if (columns != null && colIndex < columns.Count)
                    {
                        var column = columns[colIndex];
                        var mapProp = column?.GetType().GetProperty("MappingName");
                        return mapProp?.GetValue(column) as string;
                    }
                }
            }
            catch { }
            return null;
        }

        /// <summary>
        /// Gets the mapping name of the first selected cell in the data grid.
        /// </summary>
        /// <returns></returns>
        private string? GetFirstSelectedMappingName()
        {
            try
            {
                var gridType = dataGrid.GetType();
                var selectedCellsProp = gridType.GetProperty("SelectedCells", BindingFlags.Public | BindingFlags.Instance);
                var selectedCells = selectedCellsProp?.GetValue(dataGrid) as System.Collections.IEnumerable;
                if (selectedCells == null)
                {
                    return null;
                }

                foreach (var cellObj in selectedCells)
                {
                    var cellType = cellObj.GetType();
                    var columnProp = cellType.GetProperty("Column");
                    var column = columnProp?.GetValue(cellObj);
                    var mappingNameProp = column?.GetType().GetProperty("MappingName");
                    var map = mappingNameProp?.GetValue(column) as string;
                    if (!string.IsNullOrWhiteSpace(map))
                        return map;
                }
            }
            catch { }
            return null;
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
            var rows = dataGrid.SelectedRows;
            if (rows != null)
            {
                foreach (var item in rows)
                {
                    if (item is OrderInfo o1)
                    {
                        result.Add(o1);
                    }
                    else if (item is DataGridRowInfo rowInfo && rowInfo.RowData is OrderInfo o2)
                    {
                        result.Add(o2);
                    }
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
                        {
                            result.Add(o3);
                        }
                        else if (item is DataGridRowInfo rowInfo && rowInfo.RowData is OrderInfo o4)
                        {
                            result.Add(o4);
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the selected OrderInfo objects from the currently selected cells.
        /// </summary>
        /// <param name="mappingName"></param>
        /// <returns></returns>
        private IList<OrderInfo> GetSelectedOrderInfosFromSelectedCells(string mappingName)
        {
            var result = new List<OrderInfo>();
            var selectedCells = dataGrid.GetSelectedCells();
            if (selectedCells == null || selectedCells.Count == 0)
            {
                return result;
            }

            foreach (var info in selectedCells)
            {
                var column = info.Column;
                var rowData = info.RowData;
                if (rowData is OrderInfo order && column?.MappingName == mappingName)
                {
                    if (!result.Contains(order))
                    {
                        result.Add(order);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// ViewModel property changed handler to respond to changes in selection mode.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainViewModel.IsCellSelectionMode))
            {
                UpdateGridSelectionUnit(viewModel.IsCellSelectionMode);
                ClearGridSelection();
            }

            if (e.PropertyName == nameof(MainViewModel.IsBulkPopupOpen) && !viewModel.IsBulkPopupOpen)
            {
                ClearGridSelection();
            }
        }

        /// <summary>
        /// Updates the selection and navigation modes of the associated data grid to use either cell-based or row-based
        /// selection.
        /// </summary>
        /// <param name="isCellMode">true to set the data grid to cell selection mode; false to set it to row selection mode.</param>
        private void UpdateGridSelectionUnit(bool isCellMode)
        {
            var suProp = dataGrid.GetType().GetProperty("SelectionUnit", BindingFlags.Public | BindingFlags.Instance);
            if (suProp != null)
            {
                var enumType = suProp.PropertyType;
                var enumValue = Enum.Parse(enumType, isCellMode ? "Cell" : "Row");
                suProp.SetValue(dataGrid, enumValue);
            }

            var navProp = dataGrid.GetType().GetProperty("NavigationMode", BindingFlags.Public | BindingFlags.Instance);
            if (navProp != null)
            {
                var enumType = navProp.PropertyType;
                var enumValue = Enum.Parse(enumType, isCellMode ? "Cell" : "Row");
                navProp.SetValue(dataGrid, enumValue);
            }

            var selModeProp = dataGrid.GetType().GetProperty("SelectionMode", BindingFlags.Public | BindingFlags.Instance);
            if (selModeProp != null)
            {
                var enumType = selModeProp.PropertyType;
                var enumValue = Enum.Parse(enumType, "Multiple");
                selModeProp.SetValue(dataGrid, enumValue);
            }
        }

        /// <summary>
        /// To clear the selection in the DataGrid.
        /// </summary>
        private void ClearGridSelection()
        {
            var clearSel = dataGrid.GetType().GetMethod("ClearSelection", BindingFlags.Public | BindingFlags.Instance);
            clearSel?.Invoke(dataGrid, null);

            dataGrid.SelectedRows?.Clear();
            var getSelectedCells = dataGrid.GetType().GetMethod("GetSelectedCells", BindingFlags.Public | BindingFlags.Instance);
            var cellsObj = getSelectedCells?.Invoke(dataGrid, null) as System.Collections.IList;
            cellsObj?.Clear();

            var selectedCellsProp = dataGrid.GetType().GetProperty("SelectedCells", BindingFlags.Public | BindingFlags.Instance);
            var selectedCells = selectedCellsProp?.GetValue(dataGrid) as System.Collections.IList;
            selectedCells?.Clear();

            var currentCellProp = dataGrid.GetType().GetProperty("CurrentCell", BindingFlags.Public | BindingFlags.Instance);
            if (currentCellProp != null)
            {
                currentCellProp.SetValue(dataGrid, null);
            }

            var refresh = dataGrid.GetType().GetMethod("Refresh", BindingFlags.Public | BindingFlags.Instance);
            refresh?.Invoke(dataGrid, null);
        }
    }
}
