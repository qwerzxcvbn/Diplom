using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using StuldastolClient.Services;

namespace StuldastolClient.Views
{
    public partial class CartWindow : Window
    {
        private ObservableCollection<CartItemViewModel> cartItems;
        private readonly ICartService _cartService;
        private readonly IOrderService _orderService;

        public CartWindow(ICartService cartService, IOrderService orderService)
        {
            InitializeComponent();
            _cartService = cartService;
            _orderService = orderService;
            cartItems = new ObservableCollection<CartItemViewModel>();
            CartItemsControl.ItemsSource = cartItems;
            LoadCartItems();
        }

        private void LoadCartItems()
        {
            cartItems.Clear();
            var items = _cartService.GetCartItems(CurrentUser.UserId);
            foreach (var item in items)
            {
                cartItems.Add(item);
            }

            if (cartItems.Any())
            {
                CheckSP.Visibility = Visibility.Visible;
                ImageCart.Visibility = Visibility.Collapsed;
                EmptyCartText.Visibility = Visibility.Collapsed;
                TotalAmountText.Text = $"{cartItems.Count} товара на сумму: {cartItems.Sum(ci => ci.TotalPrice):N2}р";
            }
            else
            {
                CheckSP.Visibility = Visibility.Collapsed;
                ImageCart.Visibility = Visibility.Visible;
                EmptyCartText.Visibility = Visibility.Visible;
                TotalAmountText.Text = "0 товаров на сумму: 0р";
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is CartItemViewModel cartItem)
            {
                _cartService.RemoveFromCart(cartItem.CartItemId);
                cartItems.Remove(cartItem);
                UpdateTotalAmount();
                if (!cartItems.Any())
                {
                    EmptyCartText.Visibility = Visibility.Visible;
                }
            }
        }

        private void UpdateTotalAmount()
        {
            TotalAmountText.Text = $"{cartItems.Count} товара на сумму: {cartItems.Sum(ci => ci.TotalPrice):N2}р";
        }

        private void CheckoutButton_Click(object sender, RoutedEventArgs e)
        {
            int userId = CurrentUser.UserId;
            if (userId == 0)
            {
                return;
            }

            if (!cartItems.Any())
            {
                MessageBox.Show("Корзина пуста. Добавьте товары для оформления заказа.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var checkoutWindow = new CheckoutWindow(cartItems);
            if (checkoutWindow.ShowDialog() == true)
            {
                string deliveryAddress = checkoutWindow.DeliveryAddress;
                bool isSelfPickup = checkoutWindow.IsSelfPickup;
                _orderService.CreateOrder(cartItems, deliveryAddress, isSelfPickup);

                cartItems.Clear();
                EmptyCartText.Visibility = Visibility.Visible;
                TotalAmountText.Text = "0 товаров на сумму: 0р";
            }
        }

        private void GoToCatalogButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}