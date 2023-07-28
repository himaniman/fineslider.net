using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Example
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private double valueDouble = 1;

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
