using StuldastolClient.MessageWindow;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace StuldastolClient.Views
{
    public partial class CheckoutWindow : Window
    {
        private ObservableCollection<CartItemViewModel> cartItems;
        private decimal deliveryCostPerItem = 700m;
        private decimal subtotal;
        private decimal deliveryCost;
        private decimal totalWithDelivery;

        public string DeliveryAddress { get; private set; }
        public bool IsSelfPickup { get; private set; }

        public CheckoutWindow(ObservableCollection<CartItemViewModel> items)
        {
            InitializeComponent();
            cartItems = items ?? new ObservableCollection<CartItemViewModel>(); // Защита от null
            OrderItemsControl.ItemsSource = cartItems; // Устанавливаем источник данных
            LoadUserData();
            CalculateTotals();
        }

        private void LoadUserData()
        {
            if (CurrentUser.UserId == 0) // Проверка на неавторизованного пользователя
            {
                FullNameTextBox.Text = "Не указано";
                PhoneNumberTextBox.Text = "Не указано";
                return;
            }

            using (var context = new StuldastolEntities())
            {
                var user = context.Users.FirstOrDefault(u => u.Id == CurrentUser.UserId);
                if (user != null)
                {
                    string fullName = $"{user.FirstName ?? "Не указано"} {user.LastName ?? ""}".Trim();
                    FullNameTextBox.Text = string.IsNullOrEmpty(fullName) ? "Не указано" : fullName;
                    PhoneNumberTextBox.Text = user.Phone ?? "Не указано";
                }
                else
                {
                    FullNameTextBox.Text = "Не указано";
                    PhoneNumberTextBox.Text = "Не указано";
                }
            }
        }

        private void CalculateTotals()
        {
            if (cartItems == null || !cartItems.Any())
            {
                subtotal = 0m;
                deliveryCost = 0m;
                totalWithDelivery = 0m;
            }
            else
            {
                subtotal = cartItems.Sum(ci => ci.TotalPrice);
                int totalItems = cartItems.Sum(ci => ci.Quantity); 
                deliveryCost = IsSelfPickup ? 0m : totalItems * deliveryCostPerItem;
                totalWithDelivery = subtotal + deliveryCost;
            }

            if (SubtotalTextBlock != null) SubtotalTextBlock.Text = $"Сумма за товары: {subtotal:N2}р";
            if (DeliveryCostTextBlock != null) DeliveryCostTextBlock.Text = $"Стоимость доставки: {deliveryCost:N2}р";
            if (TotalWithDeliveryTextBlock != null) TotalWithDeliveryTextBlock.Text = $"Общая сумма заказа: {totalWithDelivery:N2}р";
        }

        private void SelfPickupRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            IsSelfPickup = true;
            if (SelfPickupAddressTextBlock != null) SelfPickupAddressTextBlock.Visibility = Visibility.Visible;
            if (DeliveryAddressLabel != null) DeliveryAddressLabel.Visibility = Visibility.Collapsed;
            if (DeliveryAddressTextBox != null) DeliveryAddressTextBox.Visibility = Visibility.Collapsed;
            if (DeliveryCostTextBlock != null) DeliveryCostTextBlock.Visibility = Visibility.Collapsed;
            CalculateTotals();
        }

        private void DeliveryRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            IsSelfPickup = false;
            if (SelfPickupAddressTextBlock != null) SelfPickupAddressTextBlock.Visibility = Visibility.Collapsed;
            if (DeliveryAddressLabel != null) DeliveryAddressLabel.Visibility = Visibility.Visible;
            if (DeliveryAddressTextBox != null) DeliveryAddressTextBox.Visibility = Visibility.Visible;
            if (DeliveryCostTextBlock != null) DeliveryCostTextBlock.Visibility = Visibility.Visible;
            CalculateTotals();
        }

        private void ProceedToPaymentButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(FullNameTextBox?.Text))
            {
                FalseFIOWindow falseFIOWindow = new FalseFIOWindow();
                falseFIOWindow.ShowDialog();
                return;
            }
            if (string.IsNullOrWhiteSpace(PhoneNumberTextBox?.Text))
            {
                FalseNumberPhoneWindow falseNumberPhoneWindow = new FalseNumberPhoneWindow();
                falseNumberPhoneWindow.ShowDialog();
                return;
            }
            if (!IsSelfPickup && string.IsNullOrWhiteSpace(DeliveryAddressTextBox?.Text))
            {
                FalseAddressWindow falseAddressWindow = new FalseAddressWindow();
                falseAddressWindow.ShowDialog();
                return;
            }

            DeliveryAddress = IsSelfPickup ? "Самовывоз" : DeliveryAddressTextBox.Text;
            DialogResult = true;
            Close();
        }
        private void GoToCatalogButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}