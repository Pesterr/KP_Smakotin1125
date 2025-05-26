using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WarehouseAccounting.Model
{
    internal class OrderHistoryDB
    {
        DbConnection connection;

        private OrderHistoryDB(DbConnection db)
        {
            this.connection = db;
        }

        public static OrderHistoryDB GetDb()
        {
            return new OrderHistoryDB(DbConnection.GetDbConnection());
        }

        public bool Insert(OrderHistory order)
        {
            if (connection == null || !connection.OpenConnection())
                return false;

            try
            {
                var cmd = connection.CreateCommand(
                    "INSERT INTO OrderHistory (client_name, date, order_product, quantity, fixed_price, deleted_at) VALUES (@client_name, @date, @order_product, @quantity, @fixed_price, @deleted_at)");
                cmd.Parameters.Add(new MySqlParameter("client_name", order.client_name));
                cmd.Parameters.Add(new MySqlParameter("date", order.date));
                cmd.Parameters.Add(new MySqlParameter("order_product", order.order_product));
                cmd.Parameters.Add(new MySqlParameter("quantity", order.quantity));
                cmd.Parameters.Add(new MySqlParameter("fixed_price", order.fixed_price));
                cmd.Parameters.Add(new MySqlParameter("deleted_at", DateTime.Now));
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
            finally
            {
                connection.CloseConnection();
            }
        }

        public List<OrderHistory> SelectAll()
        {
            List<OrderHistory> orders = new List<OrderHistory>();
            if (connection == null || !connection.OpenConnection())
                return orders;

            try
            {
                var command = connection.CreateCommand(
                    "SELECT id, client_name, date, order_product, quantity, fixed_price, deleted_at FROM OrderHistory");
                using (var dr = command.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        orders.Add(new OrderHistory
                        {
                            id = dr.GetInt32(0),
                            client_name = dr.GetString(1),
                            date = dr.GetDateTime(2),
                            order_product = dr.GetString(3),
                            quantity = dr.GetInt32(4),
                            fixed_price = dr.GetDecimal(5),
                            deleted_at = dr.GetDateTime(6)
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                connection.CloseConnection();
            }

            return orders;
        }
    }
}
