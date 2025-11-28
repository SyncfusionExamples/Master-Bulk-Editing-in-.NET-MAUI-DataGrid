using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Input;

namespace DataGridBulkEditSample
{
    public class OrderInfo : INotifyPropertyChanged
    {
        private int orderID;
        private string? customerName;
        private string? country;
        private string? status;
        private DateTime date;
        private string? branch;

        public int EmployeeID { get => orderID; set { orderID = value; OnPropertyChanged(nameof(EmployeeID)); } }
        public string? CustomerName { get => customerName; set { customerName = value; OnPropertyChanged(nameof(CustomerName)); } }
        public string? Country { get => country; set { country = value; OnPropertyChanged(nameof(Country)); } }
        public string? Status { get => status; set { status = value; OnPropertyChanged(nameof(Status)); } }
        public DateTime Date { get => date; set { date = value; OnPropertyChanged(nameof(Date));  }  }
        public string? Branch { get => branch; set { branch = value; OnPropertyChanged(nameof(Branch)); } }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    
}