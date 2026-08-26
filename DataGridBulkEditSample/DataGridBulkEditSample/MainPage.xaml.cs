using Syncfusion.Maui.DataGrid;
using System.ComponentModel;

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
            dataGrid.CellRightTapped += OnCellRightTapped;
#else
            // Mobile (Android/iOS): long press opens the popup
            dataGrid.CellLongPress += OnCellLongPress;
#endif
        }

#if WINDOWS || MACCATALYST
        /// <summary>
        /// Opens the bulk edit popup on right-click (desktop platforms).
        /// In row-selection mode the mapping name is irrelevant — the entire
        /// right-clicked row plus any other already-selected rows form the bulk set.
        /// </summary>
        private void OnCellRightTapped(object? sender, DataGridCellRightTappedEventArgs e)
        {
            // Commit any in-progress edit BEFORE we capture selection, otherwise the
            // pop-up's bindings may snapshot a stale value.
            CommitCurrentEditIfAny();
            DataGridCellRightTappedEventArgs longPress = e;

            if (viewModel.IsCellSelectionMode)
            {
                OpenPopupForCellMode(longPress, null);
            }
            else
            {
                OpenPopupForRowMode(longPress, null);
            }
        }
#else
        /// <summary>
        /// Opens the bulk edit popup on long press (mobile platforms).
        /// </summary>
        private void OnCellLongPress(object? sender, DataGridCellLongPressEventArgs e)
        {
            CommitCurrentEditIfAny();

            DataGridCellLongPressEventArgs longPress = e;
            if (viewModel.IsCellSelectionMode)
            {
                OpenPopupForCellMode(null, longPress);
            }
            else
            {
                OpenPopupForRowMode(null, longPress);
            }
        }
#endif

        /// <summary>
        /// Cell mode: target column is whatever cell was tapped.
        /// Selected rows are the rows of the selected cells (deduped). If the user
        /// tapped a cell outside the existing selection, only the tapped row counts.
        /// </summary>
        private void OpenPopupForCellMode(DataGridCellRightTappedEventArgs? rightTapped,
                                          DataGridCellLongPressEventArgs? longPress)
        {
            string? mapping = rightTapped?.Column?.MappingName
                              ?? longPress?.Column?.MappingName;

            // If the user tapped outside the current selection, fall back to the
            // first currently-selected cell's mapping name.
            if (string.IsNullOrWhiteSpace(mapping) ||
                (dataGrid.GetSelectedCells()?.Count > 0 &&
                 !dataGrid.GetSelectedCells()!.Any(c => c.Column?.MappingName == mapping)))
            {
                mapping = GetFirstSelectedMappingName();
            }

            if (string.IsNullOrWhiteSpace(mapping))
            {
                mapping = viewModel.TargetMappingName;
            }

            if (string.IsNullOrWhiteSpace(mapping))
            {
                return;
            }

            viewModel.TargetMappingName = mapping;

            var selectedFromCells = GetSelectedOrderInfosFromSelectedCells(mapping);
            if (selectedFromCells.Count == 0)
            {
                // Fall back to whatever row the user just clicked/long-pressed.
                selectedFromCells = rightTapped != null
                    ? GetOrderInfosFromRowData(rightTapped.RowData)
                    : GetOrderInfosFromRowData(longPress?.RowData);

                if (selectedFromCells.Count == 0)
                {
                    return;
                }
            }

            viewModel.SelectedRows = selectedFromCells;
            viewModel.IsBulkPopupOpen = true;
        }

        /// <summary>
        /// Row mode: capture every selected row (SelectedRows on the grid).
        /// Snapshots selection BEFORE the popup opens so the Apply path has a
        /// stable list even if the grid selection is cleared while the popup is up.
        /// Also seeds the row-mode editor inputs from the first selected row so the
        /// editor is "loaded" with the user's existing data.
        /// </summary>
        private void OpenPopupForRowMode(DataGridCellRightTappedEventArgs? rightTapped,
                                         DataGridCellLongPressEventArgs? longPress)
        {
            // Snapshot the selected rows from the grid IMMEDIATELY. After this point
            // we never re-read dataGrid.SelectedRows — that list may be mutated by
            // the popup's open/close cycle or by selection-mode toggling.
            var rows = GetSelectedOrderInfos();

            // Make sure the row that was tapped (right-click / long-press) is
            // included even if the user hadn't multi-selected it yet. We pull it
            // straight out of the event args (this is the same row the user is
            // currently pointing at).
            var clicked = rightTapped != null
                ? GetOrderInfosFromRowData(rightTapped.RowData)
                : GetOrderInfosFromRowData(longPress?.RowData);
            foreach (var order in clicked)
            {
                if (!rows.Contains(order))
                {
                    rows.Add(order);
                }
            }

            if (rows.Count == 0)
            {
                return;
            }

            viewModel.SelectedRows = rows;
            // Seed the row-mode editors with the first selected row's values so the
            // editor surfaces the user's existing data instead of opening blank.
            SeedRowModeEditorsFromFirstRow(rows[0]);
            viewModel.TargetMappingName = "Bulk Edit Your Values";
            viewModel.IsBulkPopupOpen = true;
        }

        /// <summary>
        /// Seeds every row-mode editor input from the supplied row's values so the
        /// popup is "loaded" with the user's existing data. EmployeeID is the
        /// primary key and is not editable from row mode.
        /// </summary>
        private void SeedRowModeEditorsFromFirstRow(OrderInfo firstRow)
        {
            viewModel.RowEditCustomerName = firstRow.CustomerName;
            viewModel.RowEditCountry = firstRow.Country;
            viewModel.RowEditStatus = firstRow.Status;
            viewModel.RowEditDate = firstRow.Date;
            viewModel.RowEditBranch = firstRow.Branch;
        }

        /// <summary>
        /// Wraps an arbitrary <see cref="object"/> row-data into an OrderInfo list.
        /// </summary>
        private static IList<OrderInfo> GetOrderInfosFromRowData(object? rowData)
        {
            if (rowData is OrderInfo order)
            {
                return new List<OrderInfo> { order };
            }
            return new List<OrderInfo>();
        }

        /// <summary>
        /// Gets the mapping name (column) of the first selected cell in the data grid.
        /// </summary>
        private string? GetFirstSelectedMappingName()
        {
            var selectedCells = dataGrid.GetSelectedCells();
            if (selectedCells == null || selectedCells.Count == 0)
            {
                return null;
            }

            foreach (var info in selectedCells)
            {
                var map = info.Column?.MappingName;
                if (!string.IsNullOrWhiteSpace(map))
                {
                    return map;
                }
            }

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
        /// </summary>
        private IList<OrderInfo> GetSelectedOrderInfos()
        {
            var result = new List<OrderInfo>();
            var rows = dataGrid.SelectedRows;
            if (rows == null)
            {
                return result;
            }

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

            return result;
        }

        /// <summary>
        /// Gets the selected OrderInfo objects from the currently selected cells
        /// whose column matches the supplied mapping name.
        /// </summary>
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
                if (rowData is OrderInfo order && column?.MappingName == mappingName && !result.Contains(order))
                {
                    result.Add(order);
                }
            }

            return result;
        }

        /// <summary>
        /// ViewModel property changed handler to respond to changes in selection mode.
        /// </summary>
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
        private void UpdateGridSelectionUnit(bool isCellMode)
        {
            ClearGridSelection();
            // SelectionMode stays Multiple and is bound from the VM so it has
            // a single source of truth. The picker only toggles the *unit*
            // (Cell vs Row), not the *count*.
            dataGrid.SelectionUnit = isCellMode ? DataGridSelectionUnit.Cell : DataGridSelectionUnit.Row;
        }

        /// <summary>
        /// To clear the selection in the DataGrid.
        /// </summary>
        private void ClearGridSelection()
        {
            dataGrid.ClearSelection();
            dataGrid.SelectedRows?.Clear();
            dataGrid.CurrentCell = default;

            var selectedCells = dataGrid.GetSelectedCells();
            selectedCells?.Clear();

            dataGrid.Refresh();
        }
    }
}