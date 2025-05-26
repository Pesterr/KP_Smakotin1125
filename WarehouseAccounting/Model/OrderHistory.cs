using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace WarehouseAccounting.Model
{
    public class OrderHistory : INotifyPropertyChanged
    {
        public int id { get; set; }
        public string client_name { get; set; }
        public DateTime date { get; set; }
        public string order_product { get; set; }
        public int quantity { get; set; }
        public decimal fixed_price { get; set; }
        public DateTime deleted_at { get; set; }

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

        public decimal Total => quantity * fixed_price;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
