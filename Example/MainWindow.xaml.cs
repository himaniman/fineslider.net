using System.ComponentModel;
using System.Windows;

namespace Example
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private double valueDouble = 1.05;

        public double ValueDouble
        {
            get { return valueDouble; }
            set 
            { 
                valueDouble = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ValueDouble)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public MainWindow()
        {
            DataContext = this;
            InitializeComponent();
        }
    }
}
