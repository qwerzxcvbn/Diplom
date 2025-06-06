using System;
using System.Collections.ObjectModel;
using System.Windows;
using StuldastolClient.Services;

namespace StuldastolClient.Views
{
    public partial class OrdersWindow : Window
    {
        private ObservableCollection<OrderViewModel> orders;
        private readonly IOrderService _orderService;

        public OrdersWindow(IOrderService orderService)
        {
            InitializeComponent();
            _orderService = orderService;
            orders = new ObservableCollection<OrderViewModel>();
            OrdersItemsControl.ItemsSource = orders;
            LoadOrders();
        }

        private void LoadOrders()
        {
            orders.Clear();
            var userOrders = _orderService.GetOrders(CurrentUser.UserId);
            foreach (var order in userOrders)
            {
                orders.Add(order);
            }
        }

        private void GoToCatalogButton_Click_1(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

    public class OrderViewModel
    {
        public int OrderId { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal TotalAmount { get; set; }
        public string StatusName { get; set; }
        public System.Collections.Generic.List<ProductionOrderViewModel> ProductionOrders { get; set; }
    }

    public class ProductionOrderViewModel
    {
        public Products Product { get; set; }
        public int Quantity { get; set; }
        public string CurrentStatusName { get; set; }
    }
}