using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Shapes;
using WarehouseAccounting.Model;

namespace WarehouseAccounting.View
{
    /// <summary>
    /// Логика взаимодействия для OrderHistoryWindow.xaml
    /// </summary>
    public partial class OrderHistoryWindow : Window
    {
        public ObservableCollection<OrderHistory> HistoryOrders { get; set; }

        public OrderHistoryWindow()
        {
            InitializeComponent();
            LoadHistory();
        }

        private void LoadHistory()
        {
            var history = OrderHistoryDB.GetDb().SelectAll();
            HistoryOrders = new ObservableCollection<OrderHistory>(history);
            DataContext = this;
        }
    }
}
