using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace StuldastolProduction.Views
{
    public partial class CompletedOrdersWindow : Window
    {
        private ObservableCollection<ProductionOrderViewModel> orders;

        public CompletedOrdersWindow()
        {
            InitializeComponent();
            orders = new ObservableCollection<ProductionOrderViewModel>();
            OrdersItemsControl.ItemsSource = orders;
            LoadOrders();

            // Отображаем информацию о пользователе
            using (var context = new StuldastolEntities())
            {
                var user = context.Users.FirstOrDefault(u => u.Id == CurrentUser.UserId);
                if (user != null)
                {
                    UserInfoTextBlock.Text = $"ID {user.Id} | {user.LastName} {user.FirstName}";
                }
            }
        }

        private void LoadOrders()
        {
            using (var context = new StuldastolEntities())
            {
                var completedOrders = context.ProductionOrders
                    .Where(po => po.AssignedTo == CurrentUser.UserId && po.CurrentStatusId == 5) // Завершенные заказы
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
                foreach (var order in completedOrders)
                {
                    orders.Add(order);
                }
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var orderListWindow = new OrderListWindow();
            orderListWindow.Show();
            this.Close();
        }
    }
}