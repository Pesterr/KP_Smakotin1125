using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WarehouseAccounting.Model;

namespace WarehouseAccounting.ViewModel
{
    public class AnalyticsViewModel : INotifyPropertyChanged
    {
        public ICommand GenerateReportCommand { get; }

        private DateTime? _startDate;
        private DateTime? _endDate;
        private ObservableCollection<ReportItem> _reportItems;

        public ObservableCollection<ReportItem> ReportItems
        {
            get => _reportItems;
            set
            {
                _reportItems = value;
                OnPropertyChanged();
            }
        }

        public DateTime? StartDate
        {
            get => _startDate;
            set
            {
                _startDate = value;
                OnPropertyChanged();
            }
        }

        public DateTime? EndDate
        {
            get => _endDate;
            set
            {
                _endDate = value;
                OnPropertyChanged();
            }
        }
        private void ExportToExcel()
        {
            if (ReportItems == null || !ReportItems.Any())
            {
                MessageBox.Show("Нет данных для выгрузки.");
                return;
            }

            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Отчет по заказам");

            // Заголовки
            worksheet.Cell(1, 1).Value = "Дата";
            worksheet.Cell(1, 2).Value = "Клиент";
            worksheet.Cell(1, 3).Value = "Товар";
            worksheet.Cell(1, 4).Value = "Количество";
            worksheet.Cell(1, 5).Value = "Цена";
            worksheet.Cell(1, 6).Value = "Сумма";

            // Стиль заголовков
            worksheet.Row(1).Style.Font.Bold = true;
            worksheet.Row(1).Style.Fill.BackgroundColor = XLColor.LightGray;

            // Данные
            int row = 2;
            foreach (var item in ReportItems)
            {
                worksheet.Cell(row, 1).Value = item.Date.ToString("dd.MM.yyyy");
                worksheet.Cell(row, 2).Value = item.ClientName;
                worksheet.Cell(row, 3).Value = item.ProductName;
                worksheet.Cell(row, 4).Value = item.Quantity;
                worksheet.Cell(row, 5).Value = item.Price;
                worksheet.Cell(row, 6).Value = item.Total;
                row++;
            }

            // Итоговая строка
            worksheet.Cell(row + 1, 1).Value = "Итого:";
            worksheet.Cell(row + 1, 4).Value = TotalOrders;
            worksheet.Cell(row + 1, 6).Value = TotalAmount;
            worksheet.Row(row + 1).Style.Font.Bold = true;
            worksheet.Row(row + 1).Style.Fill.BackgroundColor = XLColor.LightBlue;

            // Подписи к итогам
            worksheet.Cell(row + 1, 3).Value = "Количество заказов:";
            worksheet.Cell(row + 1, 5).Value = "Общая сумма:";

            // Автоширина
            worksheet.Columns().AdjustToContents();

            // Сохранение файла
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                FileName = "Отчет_по_заказам",
                DefaultExt = ".xlsx",
                Filter = "Excel Files (*.xlsx)|*.xlsx"
            };

            if (dialog.ShowDialog() == true)
            {
                workbook.SaveAs(dialog.FileName);
                MessageBox.Show("Файл успешно сохранен!");
            }
        }
        public ICommand ExportToExcelCommand { get; }
        public AnalyticsViewModel()
        {
            GenerateReportCommand = new RelayCommand(GenerateReport);
            ExportToExcelCommand = new RelayCommand(ExportToExcel);
            ReportItems = new ObservableCollection<ReportItem>();
        }

        private void GenerateReport()
        {
            var allOrders = OrderHistoryDB.GetDb().SelectAll();

            var filtered = allOrders.AsEnumerable();

            if (StartDate.HasValue)
                filtered = filtered.Where(o => o.date >= StartDate.Value.Date);

            if (EndDate.HasValue)
                filtered = filtered.Where(o => o.date <= EndDate.Value.Date.AddDays(1));

            var items = filtered.Select(o =>
            {
                var product = ProductsDB.GetDb().SelectAll().FirstOrDefault(p => p.product_id.ToString() == o.order_product);
                return new ReportItem
                {
                    Date = o.date,
                    ClientName = o.client_name,
                    ProductName = product?.product_name ?? "Неизвестный",
                    Quantity = o.quantity,
                    Price = o.fixed_price
                };
            });

            ReportItems = new ObservableCollection<ReportItem>(items);
            TotalOrders = ReportItems.Count;
            TotalAmount = ReportItems.Sum(i => i.Total);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        private int _totalOrders;
        private decimal _totalAmount;

        public int TotalOrders
        {
            get => _totalOrders;
            set
            {
                _totalOrders = value;
                OnPropertyChanged();
            }
        }

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
}
