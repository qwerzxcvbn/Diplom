using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace StuldastolProduction.Views
{
    public partial class OrderListWindow : Window
    {
        private ObservableCollection<ProductionOrderViewModel> orders;
        public bool IsNotAdmin => GetCurrentUserRole() != 2; // True, если пользователь не администратор

        public OrderListWindow()
        {
            InitializeComponent();
            orders = new ObservableCollection<ProductionOrderViewModel>();
            OrdersItemsControl.ItemsSource = orders;

            // Отображаем информацию о пользователе
            using (var context = new StuldastolEntities())
            {
                var user = context.Users.FirstOrDefault(u => u.Id == CurrentUser.UserId);
                if (user != null)
                {
                    UserInfoTextBlock.Text = $"ID {user.Id} | {user.LastName} {user.FirstName}";
                    Console.WriteLine($"User Role: {user.RoleId}, IsNotAdmin: {IsNotAdmin}"); // Отладка
                }
            }

            LoadOrders();
        }

        private void LoadOrders()
        {
            using (var context = new StuldastolEntities())
            {
                var availableOrders = context.ProductionOrders
                    .Where(po => po.CurrentStatusId == 1) // Только заказы со статусом "Принял"
                    .Join(context.Users,
                        po => po.AssignedTo,
                        u => u.Id,
                        (po, u) => new { po, u })
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
                        AssignedToUser = new UserViewModel
                        {
                            Id = x.u.Id,
                            FullName = x.u.LastName + " " + x.u.FirstName
                        }
                    }).ToList();

                orders.Clear();
                foreach (var order in availableOrders)
                {
                    orders.Add(order);
                }
            }
        }

        private int GetCurrentUserRole()
        {
            using (var context = new StuldastolEntities())
            {
                return context.Users.FirstOrDefault(u => u.Id == CurrentUser.UserId)?.RoleId ?? 0;
            }
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is ProductionOrderViewModel order)
            {
                using (var context = new StuldastolEntities())
                {
                    var productionOrder = context.ProductionOrders.FirstOrDefault(po => po.Id == order.Id);
                    if (productionOrder != null)
                    {
                        productionOrder.CurrentStatusId = 7; // Склейка каркасов
                        productionOrder.AssignedTo = CurrentUser.UserId;
                        context.SaveChanges();
                    }
                }

                var processWindow = new ProcessOrderWindow(order);
                processWindow.Show();
                this.Close();
            }
        }

        private void CompletedOrdersButton_Click(object sender, RoutedEventArgs e)
        {
            if (GetCurrentUserRole() == 4) // Рабочий цеха
            {
                var completedOrdersWindow = new CompletedOrdersWindow();
                completedOrdersWindow.Show();
            }
            else if (GetCurrentUserRole() == 2) // Администратор
            {
                var adminCompletedOrdersWindow = new AdminCompletedOrdersWindow();
                adminCompletedOrdersWindow.Show();
            }
            this.Close();
        }

        private void ActiveOrdersButton_Click(object sender, RoutedEventArgs e)
        {
            if (GetCurrentUserRole() == 4 || GetCurrentUserRole() == 2) // Для рабочего цеха и администратора
            {
                var activeOrdersWindow = new ActiveOrdersWindow();
                activeOrdersWindow.Show();
                this.Close();
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }
    }
}