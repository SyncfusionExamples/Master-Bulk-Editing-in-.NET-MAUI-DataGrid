using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Input;

namespace DataGridBulkEditSample
{
    public class MainViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Collection of orders displayed in the DataGrid.
        /// </summary>
        public ObservableCollection<OrderInfo> Orders { get; set; }

        private IList<OrderInfo> selectedRows = new List<OrderInfo>();

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

        /// <summary>
        /// Command to apply bulk edits to selected rows.
        /// </summary>
        public ICommand ApplyBulkEditCommand { get; }

        /// <summary>
        /// Command to cancel bulk edit and close the popup.
        /// </summary>
        public ICommand CancelBulkEditCommand { get; }

        private string? bulkEditValue;

        /// <summary>
        /// Value entered by the user for bulk editing text-based columns (e.g., CustomerName, Country).
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
        /// The column name (MappingName) being edited in bulk.
        /// Used to determine which property of OrderInfo to update.
        /// </summary>
        public string TargetMappingName
        {
            get => targetMappingName;
            set
            {
                targetMappingName = value;
                OnPropertyChanged(nameof(TargetMappingName));

                // Toggle UI between Entry and Picker based on column type
                IsStatusEditing = string.Equals(targetMappingName, "Status");
            }
        }

        private bool isStatusEditing;

        /// <summary>
        /// Indicates whether the current bulk edit is for the Status column.
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

        /// <summary>
        /// Available status options for bulk editing (used in Picker).
        /// </summary>
        public IList<string> StatusOptions { get; } = new List<string> { "Probation", "Confirmed" };

        private string? selectedStatus;

        /// <summary>
        /// Selected status value from the Picker for bulk editing.
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
                new OrderInfo { EmployeeID = 115, CustomerName = "Fizzy", Country = "India",Date = DateTime.Now, Status = "Probation", Branch = "Chennai"}
            };

            ApplyBulkEditCommand = new Command(ApplyBulkEdit);
            CancelBulkEditCommand = new Command(CancelBulkEdit);
        }

        /// <summary>
        /// Applies the bulk edit to all selected rows based on the target column.
        /// Handles both text-based edits and status updates.
        /// </summary>
        private void ApplyBulkEdit()
        {
            if (SelectedRows == null || SelectedRows.Count == 0)
            {
                ClosePopup();
                return;
            }

            if (IsStatusEditing)
            {
                if (string.IsNullOrWhiteSpace(SelectedStatus))
                {
                    ClosePopup();
                    return;
                }

                foreach (var order in SelectedRows)
                    order.Status = SelectedStatus;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(BulkEditValue))
                {
                    ClosePopup();
                    return;
                }

                foreach (var order in SelectedRows)
                {
                    if (TargetMappingName == "CustomerName")
                    {
                        order.CustomerName = BulkEditValue;
                    }

                    else if (TargetMappingName == "Country")
                    {
                        order.Country = BulkEditValue;
                    }

                    else
                    {
                        var prop = typeof(OrderInfo).GetProperty(TargetMappingName);
                        if (prop != null && prop.CanWrite)
                        {
                            object value = BulkEditValue;
                            if (prop.PropertyType == typeof(int) && int.TryParse(BulkEditValue, out var i))
                                value = i;

                            else if (prop.PropertyType == typeof(double) && double.TryParse(BulkEditValue, out var d))
                                value = d;

                            else if (prop.PropertyType == typeof(DateTime) &&
                            DateTime.TryParse(BulkEditValue, CultureInfo.CurrentCulture, DateTimeStyles.None, out var dt))
                                value = value = dt;
                            prop.SetValue(order, value);

                        }
                        break;
                    }
                }
            }

            ClosePopup();
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
