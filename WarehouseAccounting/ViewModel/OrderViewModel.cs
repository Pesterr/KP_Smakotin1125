using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows;
using WarehouseAccounting.Model;
using WarehouseAccounting.View;
using WarehouseAccounting.ViewModel;

public class OrderViewModel : INotifyPropertyChanged
{
    private ObservableCollection<Orders> _orders;
    private Orders _selectedOrder;

    public ObservableCollection<Orders> Orders
    {
        get => _orders;
        set
        {
            _orders = value;
            OnPropertyChanged();
        }
    }

    public Orders SelectedOrder
    {
        get => _selectedOrder;
        set
        {
            _selectedOrder = value;
            OnPropertyChanged();
        }
    }

    public ICommand EditOrderCommand { get; }
    public ICommand DeleteOrderCommand { get; }
    public ICommand RefreshOrderCommand { get; }
    public OrderViewModel()
    {
        RefreshOrderCommand = new RelayCommand(RefreshOrders);
        EditOrderCommand = new RelayCommand<object>(EditOrder);
        DeleteOrderCommand = new RelayCommand<object>(DeleteOrder);

        LoadOrders();
    }

    public void RefreshOrders()
    {
        LoadOrders();
    }

    public void LoadOrders()
    {
        var ordersFromDb = OrdersDB.GetDb().SelectAll();
        foreach (var order in ordersFromDb)
        {
            order.FixedPrice = ProductsDB.GetDb().SelectAll()
                .FirstOrDefault(p => p.product_id.ToString() == order.order_product)?.price ?? 0;
        }
        Orders = new ObservableCollection<Orders>(ordersFromDb);
        FilterOrders();
        TotalOrders = Orders.Count;
        TotalAmount = Orders.Sum(o => o.ProductPrice * o.quantity);
    }

    private void EditOrder(object param)
    {
        if (param is Orders selected)
        {
            var editWindow = new EditOrderWindow(selected);
            if (editWindow.ShowDialog() == true)
            {
                OrdersDB.GetDb().Update(editWindow.EditedOrder);
                RefreshOrders();
            }
        }
    }

    private void DeleteOrder(object param)
    {
        if (param is Orders selected)
        {
            var result = MessageBox.Show(
                "Как удалить заказ?\n\n" +
                "«Да» — Отменить заказ (товары вернутся на склад)\n" +
                "«Нет» — Заказ завершен",
                "Подтверждение удаления",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question);

            switch (result)
            {
                case MessageBoxResult.Yes:
                    CancelOrder(selected);
                    break;

                case MessageBoxResult.No:
                    RemoveOrder(selected);
                    break;

                case MessageBoxResult.Cancel:
                    return;
            }

            LoadOrders();
        }
    }

    private void RemoveOrder(Orders order)
    {
        if (MessageBox.Show("Вы уверены, что хотите удалить этот заказ?", "Подтверждение",
                MessageBoxButton.YesNo) == MessageBoxResult.Yes)
        {
            // Переносим в историю
            var historyOrder = new OrderHistory
            {
                client_name = order.client_name,
                date = order.date,
                order_product = order.order_product,
                quantity = order.quantity,
                fixed_price = order.FixedPrice
            };

            OrderHistoryDB.GetDb().Insert(historyOrder);

            // Удаляем из основной таблицы
            OrdersDB.GetDb().Remove(order);

            RefreshOrders();
            MessageBox.Show("Заказ успешно удалён и добавлен в историю.");
        }
    }

    private void CancelOrder(Orders order)
    {
        int productId;
        if (!int.TryParse(order.order_product, out productId))
        {
            MessageBox.Show("Ошибка: некорректный ID товара.");
            return;
        }

        var product = ProductsDB.GetDb().SelectAll().FirstOrDefault(p => p.product_id == productId);
        if (product == null)
        {
            MessageBox.Show("Товар не найден.");
            return;
        }

        if (!int.TryParse(product.unit, out int currentStock))
        {
            MessageBox.Show("Не удалось прочитать количество товара.");
            return;
        }

        product.unit = (currentStock + order.quantity).ToString();

        if (!ProductsDB.GetDb().Update(product))
        {
            MessageBox.Show("Ошибка при обновлении остатков на складе.");
            return;
        }

        OrdersDB.GetDb().Remove(order);
        MessageBox.Show("Заказ отменён. Товары возвращены на склад.");

        var mainWindow = Application.Current.MainWindow as Main;
        var productViewModel = mainWindow?.ProductsTabItem.DataContext as ProductViewModel;
        productViewModel?.LoadData();
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
    private string _searchText;
    public string SearchText
    {
        get => _searchText;
        set
        {
            _searchText = value;
            OnPropertyChanged();
            FilterOrders(); 
        }
    }

    private ObservableCollection<Orders> _filteredOrders;
    public ObservableCollection<Orders> FilteredOrders
    {
        get => _filteredOrders;
        set
        {
            _filteredOrders = value;
            OnPropertyChanged();
        }
    }

    private DateTime? _selectedDate;
    public DateTime? SelectedDate
    {
        get => _selectedDate;
        set
        {
            _selectedDate = value;
            OnPropertyChanged();
            FilterOrders();
        }
    }
    private void FilterOrders()
    {
        if (Orders == null)
        {
            FilteredOrders = new ObservableCollection<Orders>();
            return;
        }

        var filtered = Orders.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            filtered = filtered.Where(o =>
                o.client_name.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                o.ProductName.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        if (SelectedDate.HasValue)
        {
            filtered = filtered.Where(o => o.date.Date == SelectedDate.Value.Date);
        }

        FilteredOrders = new ObservableCollection<Orders>(filtered);
    }
    private int _totalOrders;
    public int TotalOrders
    {
        get => _totalOrders;
        set
        {
            _totalOrders = value;
            OnPropertyChanged();
        }
    }

    private decimal _totalAmount;
    public decimal TotalAmount
    {
        get => _totalAmount;
        set
        {
            _totalAmount = value;
            OnPropertyChanged();
        }
    }
}