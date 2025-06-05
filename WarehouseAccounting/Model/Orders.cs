using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace WarehouseAccounting.Model
{
    public class Orders : INotifyPropertyChanged
    {
        private int _order_id;
        private string _client_name;
        private DateTime _date;
        private string _order_product;
        private int _quantity;
        private decimal _fixedPrice;

        public int order_id
        {
            get => _order_id;
            set
            {
                _order_id = value;
                OnPropertyChanged();
            }
        }

        public string client_name
        {
            get => _client_name;
            set
            {
                _client_name = value;
                OnPropertyChanged();
            }
        }

        public DateTime date
        {
            get => _date;
            set
            {
                _date = value;
                OnPropertyChanged();
            }
        }

        public string order_product
        {
            get => _order_product;
            set
            {
                _order_product = value;
                OnPropertyChanged();
            }
        }

        public int quantity
        {
            get => _quantity;
            set
            {
                _quantity = value;
                OnPropertyChanged();
            }
        }

        public string ProductName
        {
            get
            {
                if (int.TryParse(order_product, out int productId))
                {
                    var product = ProductsDB.GetDb().SelectAll()
                        .FirstOrDefault(p => p.product_id.ToString() == order_product);
                    return product?.product_name ?? "Неизвестный товар";
                }
                return "Ошибка ID";
            }
        }

        public decimal ProductPrice => FixedPrice;

        public decimal FixedPrice
        {
            get => _fixedPrice;
            set
            {
                _fixedPrice = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public decimal TotalP => quantity * FixedPrice;
    }
}