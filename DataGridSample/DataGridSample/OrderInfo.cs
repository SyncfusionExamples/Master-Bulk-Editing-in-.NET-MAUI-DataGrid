using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Input;

namespace DataGridSample
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<OrderInfo> Orders { get; set; }

        // Selected rows captured before opening popup
        private IList<OrderInfo> selectedRows = new List<OrderInfo>();
        public IList<OrderInfo> SelectedRows
        {
            get => selectedRows;
            set { selectedRows = value ?? new List<OrderInfo>(); OnPropertyChanged(nameof(SelectedRows)); }
        }

        public ICommand ApplyBulkEditCommand { get; }
        public ICommand CancelBulkEditCommand { get; }

        // For text-based edit (e.g., CustomerName, Country)
        private string? bulkEditValue;
        public string? BulkEditValue
        {
            get => bulkEditValue;
            set { bulkEditValue = value; OnPropertyChanged(nameof(BulkEditValue)); }
        }

        // Popup state
        private bool isBulkPopupOpen;
        public bool IsBulkPopupOpen
        {
            get => isBulkPopupOpen;
            set { isBulkPopupOpen = value; OnPropertyChanged(nameof(IsBulkPopupOpen)); }
        }

        // Which column are we editing?
        private string targetMappingName = "CustomerName";
        public string TargetMappingName
        {
            get => targetMappingName;
            set
            {
                targetMappingName = value;
                OnPropertyChanged(nameof(TargetMappingName));
                IsStatusEditing = string.Equals(targetMappingName, "Status");
            }
        }

        // Toggle UI between Entry vs Picker
        private bool isStatusEditing;
        public bool IsStatusEditing
        {
            get => isStatusEditing;
            set { isStatusEditing = value; OnPropertyChanged(nameof(IsStatusEditing)); }
        }

        // Picker data for Status
        public IList<string> StatusOptions { get; } = new List<string> { "Probation", "Confirmed" };

        private string? selectedStatus;
        public string? SelectedStatus
        {
            get => selectedStatus;
            set { selectedStatus = value; OnPropertyChanged(nameof(SelectedStatus)); }
        }

        public MainViewModel()
        {
            Orders = new ObservableCollection<OrderInfo>
            {
                new OrderInfo { OrderID = 101, CustomerName = "John", Country = "USA", Date = DateTime.Now, Status = "Probation" },
                new OrderInfo { OrderID = 102, CustomerName = "Mary", Country = "UK", Date = DateTime.Now,  Status = "Confirmed",   },
                new OrderInfo { OrderID = 103, CustomerName = "Alex", Country = "India", Date = DateTime.Now, Status = "Probation" },
                new OrderInfo { OrderID = 104, CustomerName = "Rose", Country = "USA", Date = DateTime.Now, Status = "Probation" },
                new OrderInfo { OrderID = 105, CustomerName = "Jack", Country = "UK",   Date = DateTime.Now, Status = "Confirmed" },
                new OrderInfo { OrderID = 106, CustomerName = "Hick", Country = "India",Date = DateTime.Now, Status = "Probation"},
                new OrderInfo { OrderID = 107, CustomerName = "Joseph", Country = "USA", Date = DateTime.Now, Status = "Probation"},
                new OrderInfo { OrderID = 108, CustomerName = "Live", Country = "UK",   Date = DateTime.Now, Status = "Confirmed"},
                new OrderInfo { OrderID = 109, CustomerName = "Lilly", Country = "India",Date = DateTime.Now, Status = "Probation"},
                new OrderInfo { OrderID = 110, CustomerName = "Defa", Country = "USA", Date = DateTime.Now, Status = "Probation"},
                new OrderInfo { OrderID = 111, CustomerName = "Fazi", Country = "UK",   Date = DateTime.Now, Status = "Confirmed"},
                new OrderInfo { OrderID = 112, CustomerName = "Zeba", Country = "India",Date = DateTime.Now, Status = "Probation"},
                new OrderInfo { OrderID = 113, CustomerName = "Igly", Country = "USA", Date = DateTime.Now, Status = "Probation"},
                new OrderInfo { OrderID = 114, CustomerName = "Mady", Country = "UK",   Date = DateTime.Now, Status = "Confirmed"},
                new OrderInfo { OrderID = 115, CustomerName = "Pizo", Country = "India",Date = DateTime.Now, Status = "Probation"}
            };

            ApplyBulkEditCommand = new Command(ApplyBulkEdit);
            CancelBulkEditCommand = new Command(CancelBulkEdit);
        }

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

                // Use the target column
                foreach (var order in SelectedRows)
                {
                    switch (TargetMappingName)
                    {
                        case "CustomerName":
                            order.CustomerName = BulkEditValue;
                            break;
                        case "Country":
                            order.Country = BulkEditValue;
                            break;
                        default:
                            // Generic fallback via reflection
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
                                    value = dt;

                                prop.SetValue(order, value);
                            }
                            break;
                    }
                }
            }

            ClosePopup();
        }

        private void CancelBulkEdit() => ClosePopup();

        private void ClosePopup()
        {
            IsBulkPopupOpen = false;
            BulkEditValue = string.Empty;
            SelectedStatus = null;
            SelectedRows = new List<OrderInfo>();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public class OrderInfo : INotifyPropertyChanged
    {
        private int orderID;
        private string? customerName;
        private string? country;
        private string? status;
        private DateTime date;

        public int OrderID { get => orderID; set { orderID = value; OnPropertyChanged(nameof(OrderID)); } }
        public string? CustomerName { get => customerName; set { customerName = value; OnPropertyChanged(nameof(CustomerName)); } }
        public string? Country { get => country; set { country = value; OnPropertyChanged(nameof(Country)); } }
        public string? Status { get => status; set { status = value; OnPropertyChanged(nameof(Status)); } }
        public DateTime Date { get => date; set { date = value; OnPropertyChanged(nameof(Date));  }  }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    
}