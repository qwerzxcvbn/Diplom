using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace StuldastolProduction.Views
{
    public partial class AdminCompletedOrdersWindow : Window
    {
        private ObservableCollection<ProductionOrderViewModel> orders;
        private ObservableCollection<UserViewModel> employees;

        public AdminCompletedOrdersWindow()
        {
            InitializeComponent();
            orders = new ObservableCollection<ProductionOrderViewModel>();
            employees = new ObservableCollection<UserViewModel>();
            OrdersItemsControl.ItemsSource = orders;
            EmployeeFilterComboBox.ItemsSource = employees;
            LoadEmployees();
            LoadOrders();

            // Отображаем информацию о пользователе
            using (var context = new StuldastolEntities())
            {
                var user = context.Users.FirstOrDefault(u => u.Id == CurrentUser.UserId);
                if (user != null)
                {
                    UserInfoTextBlock.Text = $"ID: {user.Id} | {user.LastName} {user.FirstName}";
                }
            }
        }

        private void LoadEmployees()
        {
            using (var context = new StuldastolEntities())
            {
                var employeeList = context.Users
                    .Where(u => u.RoleId == 4) // Рабочие цеха
                    .Select(u => new UserViewModel
                    {
                        Id = u.Id,
                        FullName = u.LastName + " " + u.FirstName
                    }).ToList();

                employees.Clear();
                employees.Add(new UserViewModel { Id = 0, FullName = "Все сотрудники" });
                foreach (var employee in employeeList)
                {
                    employees.Add(employee);
                }
            }
        }

        private void LoadOrders()
        {
            using (var context = new StuldastolEntities())
            {
                var query = context.ProductionOrders
                    .Where(po => po.CurrentStatusId == 5) // Завершенные заказы
                    .Join(context.Users,
                        po => po.AssignedTo,
                        u => u.Id,
                        (po, u) => new { po, u });

                // Фильтр по сотруднику
                if (EmployeeFilterComboBox.SelectedItem is UserViewModel selectedEmployee && selectedEmployee.Id != 0)
                {
                    query = query.Where(x => x.po.AssignedTo == selectedEmployee.Id);
                }

                // Фильтр по периоду
                if (PeriodFilterComboBox.SelectedItem is ComboBoxItem selectedPeriod)
                {
                    DateTime startDate = DateTime.Now;
                    switch (selectedPeriod.Content.ToString())
                    {
                        case "За день":
                            startDate = DateTime.Now.Date;
                            break;
                        case "За неделю":
                            startDate = DateTime.Now.Date.AddDays(-7);
                            break;
                        case "За месяц":
                            startDate = DateTime.Now.Date.AddMonths(-1);
                            break;
                    }
                    query = query.Where(x => x.po.ProductionStatusLogs
                        .Any(log => log.StatusId == 5 && log.ChangedAt >= startDate));
                }

                var completedOrders = query
                    .Select(x => new ProductionOrderViewModel
                    {
                        Id = x.po.Id,
                        OrderId = x.po.OrderId,
                        Product = x.po.Products,
                        Quantity = x.po.Quantity,
                        WoodType = x.po.WoodType,
                        OilColor = x.po.OilColor,
                        FabricType = x.po.FabricType,
                        Deadline = x.po.Deadline,
                        IsSelfPickup = x.po.IsSelfPickup,
                        AssignedToUser = new UserViewModel { Id = x.u.Id, FullName = x.u.LastName + " " + x.u.FirstName }
                    }).ToList();

                orders.Clear();
                foreach (var order in completedOrders)
                {
                    orders.Add(order);
                }
            }
        }

        private void FilterChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadOrders();
        }

        private void GenerateReportButton_Click(object sender, RoutedEventArgs e)
        {
            // Создаем диалоговое окно для выбора пути сохранения файла
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Текстовые файлы (*.txt)|*.txt",
                DefaultExt = "txt",
                FileName = $"CompletedOrdersReport_{DateTime.Now:yyyyMMdd_HHmmss}",
                Title = "Сохранить отчет"
            };

            // Показываем диалог и проверяем, выбрал ли пользователь файл
            if (saveFileDialog.ShowDialog() == true)
            {
                string filePath = saveFileDialog.FileName;
                string period = (PeriodFilterComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Не выбран";
                string employee = (EmployeeFilterComboBox.SelectedItem as UserViewModel)?.FullName ?? "Все сотрудники";

                // Формируем содержимое отчета
                StringBuilder reportContent = new StringBuilder();
                reportContent.AppendLine(new string('=', 60));
                reportContent.AppendLine($"Отчет по выполненным заказам".PadLeft(30 + "Отчет по выполненным заказам".Length / 2));
                reportContent.AppendLine($"Дата создания: {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
                reportContent.AppendLine($"Период: {period}");
                reportContent.AppendLine($"Сотрудник: {employee}");
                reportContent.AppendLine(new string('=', 60));
                reportContent.AppendLine();

                // Заголовки таблицы
                reportContent.AppendLine($"{"№",-8} {"Продукт",-70} {"Количество",-15} {"Сотрудник"}"); ;
                reportContent.AppendLine(new string('-', 120));

                // Данные о заказах
                foreach (var order in orders)
                {
                    reportContent.AppendLine($"{order.OrderId,-8} {order.Product.Name,-75} {order.Quantity,-10} {order.AssignedToUser.FullName}");
                }

                reportContent.AppendLine(new string('-', 120));
                reportContent.AppendLine();
                reportContent.AppendLine($"Итог: {orders.Count} заказов выполнено.");
                reportContent.AppendLine(new string('=', 120));

                // Сохраняем отчет
                try
                {
                    File.WriteAllText(filePath, reportContent.ToString());
                    MessageBox.Show($"Отчет успешно сохранен как {filePath}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сохранении отчета: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Сохранение отчета отменено.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var orderListWindow = new OrderListWindow();
            orderListWindow.Show();
            this.Close();
        }
    }

    public class UserViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; }
    }
}