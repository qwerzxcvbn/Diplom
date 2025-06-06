using System;
using System.Collections.Generic;
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
using StuldastolClient.Views.WindowAdmin;

namespace StuldastolClient.Views.WindowAdmin
{
    public partial class OrdersAdminWindow : Window
    {
        public OrdersAdminWindow()
        {
            InitializeComponent();
            LoadOrders();
            Resources["BooleanToVisibilityConverter"] = new BooleanToVisibilityConverter();
        }

        private void GoToCatalogButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void LoadOrders()
        {
            using (var context = new StuldastolEntities())
            {
                var query = from order in context.Orders
                            join productionOrder in context.ProductionOrders on order.Id equals productionOrder.OrderId
                            join product in context.Products on productionOrder.ProductId equals product.Id
                            join user in context.Users on order.UserId equals user.Id
                            join orderStatus in context.OrderStatuses on order.StatusId equals orderStatus.Id
                            select new
                            {
                                OrderId = order.Id,
                                Product = product,
                                User = user,
                                IsSelfPickup = order.IsSelfPickup,
                                DeliveryAddress = order.DeliveryAddress,
                                OrderStatusId = order.StatusId,
                                OrderStatusName = orderStatus.Name,
                                ProductionStatusId = productionOrder.CurrentStatusId
                            };

                var ordersList = query.ToList()
                    .Select(o =>
                    {
                        string deliveryType = o.IsSelfPickup ? "Самовывоз" : "Доставка (" + (o.DeliveryAddress ?? "Не указан") + ")";
                        return new
                        {
                            OrderId = o.OrderId,
                            Product = o.Product,
                            User = o.User,
                            DeliveryType = deliveryType,
                            OrderStatusId = o.OrderStatusId,
                            OrderStatusName = o.OrderStatusName,
                            ProductionStatusId = o.ProductionStatusId
                        };
                    })
                    .GroupBy(o => o.OrderId)
                    .Select(g => new
                    {
                        Key = g.Key,
                        Products = g.ToList(),
                        IsCancelable = g.All(o => o.OrderStatusId != 5), // Можно отменить, если статус не "Отменен"
                        First = g.First() // Для доступа к первым значениям
                    })
                    .ToList();

                OrdersItemsControl.ItemsSource = ordersList;
            }
        }

        private void CancelOrderButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                var dataContext = button.DataContext as dynamic;
                if (dataContext != null)
                {
                    int orderId = dataContext.Key; // Доступ к Key через dynamic
                    var result = MessageBox.Show("Вы уверены, что хотите отменить этот заказ?", "Подтверждение отмены", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        using (var context = new StuldastolEntities())
                        {
                            var order = context.Orders.FirstOrDefault(o => o.Id == orderId);
                            if (order != null)
                            {
                                order.StatusId = 5; // Устанавливаем статус "Отменен"
                                var productionOrders = context.ProductionOrders.Where(po => po.OrderId == orderId);
                                foreach (var po in productionOrders)
                                {
                                    po.CurrentStatusId = 6; // Устанавливаем статус "Отменен"
                                }
                                context.SaveChanges();
                                LoadOrders();
                            }
                        }
                    }
                }
            }
        }

        private void OrderBorder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border)
            {
                var dataContext = border.DataContext as dynamic;
                if (dataContext != null)
                {
                    int orderId = dataContext.Key; // Доступ к Key через dynamic
                    var orderDetailsWindow = new OrderDetailsWindow(orderId);
                    orderDetailsWindow.ShowDialog();
                }
            }
        }
    }

    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool isVisible)
            {
                return isVisible ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}