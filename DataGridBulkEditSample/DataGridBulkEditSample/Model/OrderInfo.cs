using System.ComponentModel;

namespace DataGridBulkEditSample
{
    /// <summary>
    /// Represents an order record displayed in the DataGrid.
    /// Implements <see cref="INotifyPropertyChanged"/> for UI data binding.
    /// </summary>
    public class OrderInfo : INotifyPropertyChanged
    {
        private int orderID;
        private string? customerName;
        private string? country;
        private string? status;
        private DateTime date;
        private string? branch;

        /// <summary>
        /// Gets or sets the unique employee ID associated with the order.
        /// </summary>
        public int EmployeeID
        {
            get => orderID;
            set
            {
                orderID = value;
                OnPropertyChanged(nameof(EmployeeID));
            }
        }

        /// <summary>
        /// Gets or sets the name of the customer.
        /// </summary>
        public string? CustomerName
        {
            get => customerName;
            set
            {
                customerName = value;
                OnPropertyChanged(nameof(CustomerName));
            }
        }

        /// <summary>
        /// Gets or sets the country associated with the order.
        /// </summary>
        public string? Country
        {
            get => country;
            set
            {
                country = value;
                OnPropertyChanged(nameof(Country));
            }
        }

        /// <summary>
        /// Gets or sets the employment status (e.g., Probation, Confirmed).
        /// </summary>
        public string? Status
        {
            get => status;
            set
            {
                status = value;
                OnPropertyChanged(nameof(Status));
            }
        }

        /// <summary>
        /// Gets or sets the date of the order.
        /// </summary>
        public DateTime Date
        {
            get => date;
            set
            {
                date = value;
                OnPropertyChanged(nameof(Date));
            }
        }

        /// <summary>
        /// Gets or sets the branch location associated with the order.
        /// </summary>
        public string? Branch
        {
            get => branch;
            set
            {
                branch = value;
                OnPropertyChanged(nameof(Branch));
            }
        }

        /// <summary>
        /// Event raised when a property value changes.
        /// Used by data binding to update the UI.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event for the specified property.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        public void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        /// <summary>
        /// Forces a <see cref="PropertyChanged"/> notification for the supplied
        /// property name. Useful when the property was set via reflection (e.g.
        /// by reflection-based bulk-edit paths) and the setter was bypassed.
        /// </summary>
        /// <param name="propertyName">The mapping/property name to refresh.</param>
        public void RaisePropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
