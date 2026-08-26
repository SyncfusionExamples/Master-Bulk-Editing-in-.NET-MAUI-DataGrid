using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Input;

namespace DataGridBulkEditSample
{
    /// <summary>
    /// Represents the MainViewModel for the DataGrid bulk edit.
    /// </summary>
    public class MainViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Collection of orders displayed in the DataGrid.
        /// </summary>
        public ObservableCollection<OrderInfo> Orders { get; set; }

        private IList<OrderInfo> selectedRows = new List<OrderInfo>();

        /// <summary>
        /// True when Cell mode is showing the generic text editor
        /// (i.e. not Status and not Date).
        /// </summary>
        public bool IsTextEditing => !IsStatusEditing && !IsDateEditing;

        /// <summary>
        /// List of rows selected in the DataGrid before opening the bulk edit popup.
        /// </summary>
        public IList<OrderInfo> SelectedRows
        {
            get => selectedRows;
            set
            {
                selectedRows = value ?? new List<OrderInfo>();
                OnPropertyChanged(nameof(SelectedRows));
            }
        }

        private bool isCellSelectionMode;

        /// <summary>
        /// True = Cell selection mode; False = Row selection mode.
        /// Bound indirectly via SelectedSelectionMode.
        /// </summary>
        public bool IsCellSelectionMode
        {
            get => isCellSelectionMode;
            set
            {
                if (isCellSelectionMode != value)
                {
                    isCellSelectionMode = value;
                    OnPropertyChanged(nameof(IsCellSelectionMode));
                }
            }
        }

        private string selectedSelectionMode = "Cell";

        /// <summary>
        /// Backing property bound to the Picker. When set, toggles IsCellSelectionMode.
        /// </summary>
        public string SelectedSelectionMode
        {
            get => selectedSelectionMode;
            set
            {
                if (selectedSelectionMode != value)
                {
                    selectedSelectionMode = value;
                    OnPropertyChanged(nameof(SelectedSelectionMode));
                    IsCellSelectionMode = string.Equals(value, "Cell", StringComparison.OrdinalIgnoreCase);
                }
            }
        }

        /// <summary>
        /// Command to apply edits.
        /// </summary>
        public ICommand ApplyBulkEditCommand { get; }

        /// <summary>
        /// Command to cancel bulk edit and close the popup.
        /// </summary>
        public ICommand CancelBulkEditCommand { get; }

        private string? bulkEditValue;

        /// <summary>
        /// Value entered by the user for bulk editing text-based columns (Cell mode).
        /// </summary>
        public string? BulkEditValue
        {
            get => bulkEditValue;
            set
            {
                bulkEditValue = value;
                OnPropertyChanged(nameof(BulkEditValue));
            }
        }

        private DateTime? bulkEditDate;

        /// <summary>
        /// Value entered by the user when editing the Date column (Cell mode).
        /// </summary>
        public DateTime? BulkEditDate
        {
            get => bulkEditDate;
            set
            {
                bulkEditDate = value;
                OnPropertyChanged(nameof(BulkEditDate));
            }
        }

        // Row mode inputs — one per editable column (empty entries are ignored when Apply is pressed).
        private string? rowEditCustomerName;
        public string? RowEditCustomerName
        {
            get => rowEditCustomerName;
            set { rowEditCustomerName = value; OnPropertyChanged(nameof(RowEditCustomerName)); }
        }

        private string? rowEditCountry;
        public string? RowEditCountry
        {
            get => rowEditCountry;
            set { rowEditCountry = value; OnPropertyChanged(nameof(RowEditCountry)); }
        }

        private string? rowEditStatus;
        public string? RowEditStatus
        {
            get => rowEditStatus;
            set { rowEditStatus = value; OnPropertyChanged(nameof(RowEditStatus)); }
        }

        private DateTime? rowEditDate;
        public DateTime? RowEditDate
        {
            get => rowEditDate;
            set { rowEditDate = value; OnPropertyChanged(nameof(RowEditDate)); }
        }

        private string? rowEditBranch;
        public string? RowEditBranch
        {
            get => rowEditBranch;
            set { rowEditBranch = value; OnPropertyChanged(nameof(RowEditBranch)); }
        }

        private bool isBulkPopupOpen;

        /// <summary>
        /// Indicates whether the bulk edit popup is currently open.
        /// </summary>
        public bool IsBulkPopupOpen
        {
            get => isBulkPopupOpen;
            set
            {
                isBulkPopupOpen = value;
                OnPropertyChanged(nameof(IsBulkPopupOpen));
            }
        }

        private string targetMappingName = "CustomerName";

        /// <summary>
        /// The column name (MappingName) being edited in bulk (Cell selection mode).
        /// In Row selection mode this is set to "Bulk Edit Your Values" and the
        /// row-mode editor uses <see cref="SelectedBulkColumn"/> instead.
        /// </summary>
        public string TargetMappingName
        {
            get => targetMappingName;
            set
            {
                targetMappingName = value;
                OnPropertyChanged(nameof(TargetMappingName));
                IsStatusEditing = string.Equals(targetMappingName, "Status", StringComparison.Ordinal);
                IsDateEditing = string.Equals(targetMappingName, "Date", StringComparison.Ordinal);
            }
        }

        private bool isStatusEditing;

        /// <summary>
        /// Indicates whether the current bulk edit is for the Status column (Cell selection mode).
        /// If true, UI shows a Picker instead of a text Entry.
        /// </summary>
        public bool IsStatusEditing
        {
            get => isStatusEditing;
            set
            {
                isStatusEditing = value;
                OnPropertyChanged(nameof(IsStatusEditing));
            }
        }

        private bool isDateEditing;

        /// <summary>
        /// True when editing Date column to show a DatePicker.
        /// </summary>
        public bool IsDateEditing
        {
            get => isDateEditing;
            set
            {
                isDateEditing = value;
                OnPropertyChanged(nameof(IsDateEditing));
            }
        }

        /// <summary>
        /// Available status options for bulk editing (used in Picker).
        /// </summary>
        public IList<string> StatusOptions { get; } = new List<string> { "Probation", "Confirmed" };

        /// <summary>
        /// Column options available for row-selection bulk edit. Excludes EmployeeID
        /// (read-only) and CustomerName (not bulk-editable in this view).
        /// </summary>
        public IList<string> BulkColumnOptions { get; } = new List<string> { "Country", "Status", "Date", "Branch" };

        private string selectedBulkColumn = "Country";

        /// <summary>
        /// Selected column to bulk-edit when in row selection mode.
        /// Drives which editor (text / picker / date picker) is shown.
        /// </summary>
        public string SelectedBulkColumn
        {
            get => selectedBulkColumn;
            set
            {
                if (selectedBulkColumn == value)
                {
                    return;
                }

                selectedBulkColumn = value ?? "Country";
                OnPropertyChanged(nameof(SelectedBulkColumn));
                OnPropertyChanged(nameof(IsRowEditStatusColumn));
                OnPropertyChanged(nameof(IsRowEditDateColumn));
                OnPropertyChanged(nameof(IsRowEditTextColumn));
            }
        }

        /// <summary>True when the row editor should show the Status picker.</summary>
        public bool IsRowEditStatusColumn => string.Equals(selectedBulkColumn, "Status", StringComparison.Ordinal);

        /// <summary>True when the row editor should show the Date picker.</summary>
        public bool IsRowEditDateColumn => string.Equals(selectedBulkColumn, "Date", StringComparison.Ordinal);

        /// <summary>True when the row editor should show the free-text Entry.</summary>
        public bool IsRowEditTextColumn => !IsRowEditStatusColumn && !IsRowEditDateColumn;

        private string? selectedStatus;

        /// <summary>
        /// Selected status value from the Picker for bulk editing (Cell selection mode).
        /// </summary>
        public string? SelectedStatus
        {
            get => selectedStatus;
            set
            {
                selectedStatus = value;
                OnPropertyChanged(nameof(SelectedStatus));
            }
        }

        /// <summary>
        /// Initializes the ViewModel with sample data and sets up commands.
        /// </summary>
        public MainViewModel()
        {
            Orders = new ObservableCollection<OrderInfo>
            {
                new OrderInfo { EmployeeID = 101, CustomerName = "John", Country = "USA", Date = DateTime.Now, Status = "Probation", Branch="Chennai" },
                new OrderInfo { EmployeeID = 102, CustomerName = "Mary", Country = "UK", Date = DateTime.Now,  Status = "Confirmed", Branch="Kenya"  },
                new OrderInfo { EmployeeID = 103, CustomerName = "Alex", Country = "India", Date = DateTime.Now, Status = "Probation", Branch="Chennai"},
                new OrderInfo { EmployeeID = 104, CustomerName = "Rose", Country = "USA", Date = DateTime.Now, Status = "Probation", Branch="Chennai" },
                new OrderInfo { EmployeeID = 105, CustomerName = "Jack", Country = "UK",   Date = DateTime.Now, Status = "Confirmed", Branch="Kenya" },
                new OrderInfo { EmployeeID = 106, CustomerName = "Nick", Country = "India",Date = DateTime.Now, Status = "Probation", Branch = "Chennai"},
                new OrderInfo { EmployeeID = 107, CustomerName = "Joseph", Country = "USA", Date = DateTime.Now, Status = "Probation", Branch = "Chennai"},
                new OrderInfo { EmployeeID = 108, CustomerName = "Zaber", Country = "UK",   Date = DateTime.Now, Status = "Confirmed", Branch="Kenya"},
                new OrderInfo { EmployeeID = 109, CustomerName = "Lilly", Country = "India",Date = DateTime.Now, Status = "Probation", Branch = "Chennai"},
                new OrderInfo { EmployeeID = 110, CustomerName = "Defa", Country = "USA", Date = DateTime.Now, Status = "Probation", Branch = "Chennai"},
                new OrderInfo { EmployeeID = 111, CustomerName = "Fazi", Country = "UK",   Date = DateTime.Now, Status = "Confirmed", Branch="Kenya"},
                new OrderInfo { EmployeeID = 112, CustomerName = "Zeba", Country = "India",Date = DateTime.Now, Status = "Probation", Branch = "Chennai"},
                new OrderInfo { EmployeeID = 113, CustomerName = "Igly", Country = "USA", Date = DateTime.Now, Status = "Probation", Branch = "Chennai"},
                new OrderInfo { EmployeeID = 114, CustomerName = "Mady", Country = "UK",   Date = DateTime.Now, Status = "Confirmed", Branch="Kenya"},
                new OrderInfo { EmployeeID = 115, CustomerName = "Fizzy", Country = "India",Date = DateTime.Now, Status = "Probation", Branch = "Chennai" }
            };

            // default to Cell selection mode
            IsCellSelectionMode = true;

            ApplyBulkEditCommand = new Command(ApplyBulkEdit);
            CancelBulkEditCommand = new Command(CancelBulkEdit);
        }

        /// <summary>
        /// Applies the edit based on selection mode.
        /// Cell mode edits a single column across selected cells' rows.
        /// Row mode edits the selected column (<see cref="SelectedBulkColumn"/>) across
        /// selected rows, only when the user provided a non-empty value.
        /// </summary>
        private void ApplyBulkEdit()
        {
            if (SelectedRows == null || SelectedRows.Count == 0)
            {
                ClosePopup();
                return;
            }

            if (IsCellSelectionMode)
            {
                ApplyCellModeBulkEdit();
            }
            else
            {
                ApplyRowModeBulkEdit();
            }

            ClosePopup();
        }

        /// <summary>
        /// Cell mode: applies a single-field change on the TargetMappingName column.
        /// Prevents bulk editing of EmployeeID and CustomerName.
        /// </summary>
        private void ApplyCellModeBulkEdit()
        {
            // EmployeeID is the row primary key — never bulk-edit it. CustomerName IS
            // bulk-editable here (Cell mode); users explicitly right-click the Name
            // column to retitle multiple rows.
            if (string.Equals(TargetMappingName, "EmployeeID", StringComparison.Ordinal))
            {
                return;
            }

            if (IsStatusEditing)
            {
                if (string.IsNullOrWhiteSpace(SelectedStatus))
                {
                    return;
                }

                foreach (var order in SelectedRows)
                {
                    order.Status = SelectedStatus;
                }
                return;
            }

            if (IsDateEditing)
            {
                if (BulkEditDate == null)
                {
                    return;
                }

                foreach (var order in SelectedRows)
                {
                    order.Date = BulkEditDate.Value;
                }
                return;
            }

            // Text editor: Country / CustomerName / Branch / anything else writable that isn't a known picker column.
            if (string.IsNullOrWhiteSpace(BulkEditValue))
            {
                return;
            }

            switch (TargetMappingName)
            {
                case "Country":
                    foreach (var order in SelectedRows) { order.Country = BulkEditValue; }
                    break;
                case "CustomerName":
                    foreach (var order in SelectedRows) { order.CustomerName = BulkEditValue; }
                    break;
                case "Branch":
                    foreach (var order in SelectedRows) { order.Branch = BulkEditValue; }
                    break;
                default:
                    // Defensive fallback for any future mapping name — only touches writable
                    // properties and never mutates EmployeeID.
                    var prop = typeof(OrderInfo).GetProperty(TargetMappingName);
                    if (prop == null || !prop.CanWrite ||
                        string.Equals(prop.Name, "EmployeeID", StringComparison.Ordinal))
                    {
                        return;
                    }

                    object? value = BulkEditValue;
                    if (prop.PropertyType == typeof(int) && int.TryParse(BulkEditValue, out var i))
                    {
                        value = i;
                    }
                    else if (prop.PropertyType == typeof(double) && double.TryParse(BulkEditValue, NumberStyles.Any, CultureInfo.CurrentCulture, out var d))
                    {
                        value = d;
                    }
                    else if (prop.PropertyType == typeof(DateTime) &&
                             DateTime.TryParse(BulkEditValue, CultureInfo.CurrentCulture, DateTimeStyles.None, out var dt))
                    {
                        value = dt;
                    }

                    foreach (var order in SelectedRows)
                    {
                        prop.SetValue(order, value);
                    }
                    break;
            }

            // Force INotifyPropertyChanged on every OrderInfo so the grid re-renders the
            // updated values for the bulk-edited rows. Each property setter already
            // raises a notification, but firing it here guarantees the grid picks up
            // changes even when the bind-side mirror lags behind model mutation.
            foreach (var order in SelectedRows)
            {
                order.RaisePropertyChanged(TargetMappingName);
            }
        }

        /// <summary>
        /// Row mode: applies updates from every per-column editor input the user
        /// touched (i.e. non-empty). Empty inputs are skipped so partial updates
        /// work correctly — the user only fills the fields they want to change.
        /// </summary>
        private void ApplyRowModeBulkEdit()
        {
            if (SelectedRows == null || SelectedRows.Count == 0)
            {
                return;
            }

            // CustomerName
            if (!string.IsNullOrWhiteSpace(RowEditCustomerName))
            {
                foreach (var order in SelectedRows) { order.CustomerName = RowEditCustomerName; }
                foreach (var order in SelectedRows) { order.RaisePropertyChanged(nameof(OrderInfo.CustomerName)); }
            }

            // Country
            if (!string.IsNullOrWhiteSpace(RowEditCountry))
            {
                foreach (var order in SelectedRows) { order.Country = RowEditCountry; }
                foreach (var order in SelectedRows) { order.RaisePropertyChanged(nameof(OrderInfo.Country)); }
            }

            // Status (picker — empty string is the legitimate "no change" sentinel)
            if (!string.IsNullOrWhiteSpace(RowEditStatus))
            {
                foreach (var order in SelectedRows) { order.Status = RowEditStatus; }
                foreach (var order in SelectedRows) { order.RaisePropertyChanged(nameof(OrderInfo.Status)); }
            }

            // Date
            if (RowEditDate != null)
            {
                foreach (var order in SelectedRows) { order.Date = RowEditDate.Value; }
                foreach (var order in SelectedRows) { order.RaisePropertyChanged(nameof(OrderInfo.Date)); }
            }

            // Branch
            if (!string.IsNullOrWhiteSpace(RowEditBranch))
            {
                foreach (var order in SelectedRows) { order.Branch = RowEditBranch; }
                foreach (var order in SelectedRows) { order.RaisePropertyChanged(nameof(OrderInfo.Branch)); }
            }
        }

        /// <summary>
        /// Cancels the bulk edit operation and closes the popup.
        /// </summary>
        private void CancelBulkEdit() => ClosePopup();

        /// <summary>
        /// Resets popup state and clears temporary values after bulk edit.
        /// </summary>
        private void ClosePopup()
        {
            IsBulkPopupOpen = false;
            BulkEditValue = string.Empty;
            SelectedStatus = null;
            RowEditCustomerName = null;
            RowEditCountry = null;
            RowEditStatus = null;
            RowEditDate = null;
            RowEditBranch = null;
            SelectedRows = new List<OrderInfo>();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Raises the PropertyChanged event for data binding updates.
        /// </summary>
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}