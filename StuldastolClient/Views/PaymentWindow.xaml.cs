using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using QRCoder;
using System.Drawing;
using System.IO;
using StuldastolClient.MessageWindow;

namespace StuldastolClient.Views
{
    public partial class PaymentWindow : Window
    {
        private ObservableCollection<CartItemViewModel> cartItems;
        private string deliveryAddress;
        private bool isSelfPickup;
        private decimal deliveryCostPerItem = 700m;
        private decimal subtotal;
        private decimal deliveryCost;
        private decimal promoDiscount;
        private decimal paymentDiscount;
        private decimal total;
        private bool isFullPayment;

        public decimal FinalTotal { get; private set; }
        public int? PromoCodeId { get; private set; }

        public PaymentWindow(ObservableCollection<CartItemViewModel> items, string deliveryAddress, bool isSelfPickup)
        {
            InitializeComponent();
            cartItems = items ?? new ObservableCollection<CartItemViewModel>(); // Защита от null
            this.deliveryAddress = deliveryAddress ?? "Не указано"; // Защита от null
            this.isSelfPickup = isSelfPickup;
            isFullPayment = true;
            CalculateTotals();
            GenerateQrCode(); // Генерируем QR-код при открытии окна
        }

        private void CalculateTotals()
        {
            if (cartItems == null || !cartItems.Any())
            {
                subtotal = 0m;
                deliveryCost = 0m;
                total = 0m;
            }
            else
            {
                subtotal = cartItems.Sum(ci => ci.TotalPrice); // Предполагаем, что TotalPrice не null
                int totalItems = cartItems.Sum(ci => ci.Quantity); // Предполагаем, что Quantity не null
                deliveryCost = isSelfPickup ? 0m : totalItems * deliveryCostPerItem;
            }
            paymentDiscount = isFullPayment ? (subtotal + deliveryCost) * 0.02m : 0m;
            total = (subtotal + deliveryCost - promoDiscount - paymentDiscount) * (isFullPayment ? 1m : 0.5m);

            if (SubtotalTextBlock != null) SubtotalTextBlock.Text = $"Сумма за товары: {subtotal:N2}р";
            if (DeliveryCostTextBlock != null) DeliveryCostTextBlock.Text = $"Стоимость доставки: {deliveryCost:N2}р";
            if (PromoDiscountTextBlock != null) PromoDiscountTextBlock.Text = $"Скидка по промокоду: {promoDiscount:N2}р";
            if (PaymentDiscountTextBlock != null) PaymentDiscountTextBlock.Text = $"Скидка за полную оплату (2%): {paymentDiscount:N2}р";
            if (TotalTextBlock != null) TotalTextBlock.Text = $"Итоговая сумма: {(subtotal + deliveryCost - promoDiscount - paymentDiscount):N2}р";
            if (FinalTotalTextBlock != null) FinalTotalTextBlock.Text = $"К оплате: {total:N2}р";
            FinalTotal = total;
        }

        private void GenerateQrCode()
        {
            // Формируем строку для QR-кода в формате СБП
            string purpose = "Оплата заказа через StuldastolApp";
            decimal amountInKopecks = total * 100; // Сумма в копейках
            string qrData = $"ST00012|Name=ИП Иванов Иван Иванович|PersonalAcc=40802810100000000001|BIC=044525974|BankName=Т-Банк|Sum={amountInKopecks:F0}|Purpose={purpose}";

            // Генерируем QR-код с помощью QRCoder
            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            {
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrData, QRCodeGenerator.ECCLevel.Q);
                using (QRCode qrCode = new QRCode(qrCodeData))
                {
                    using (Bitmap qrCodeBitmap = qrCode.GetGraphic(20))
                    {
                        // Конвертируем Bitmap в BitmapImage для WPF
                        using (MemoryStream memory = new MemoryStream())
                        {
                            qrCodeBitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png); // Используем ImageFormat.Png
                            memory.Position = 0;

                            BitmapImage bitmapImage = new BitmapImage();
                            bitmapImage.BeginInit();
                            bitmapImage.StreamSource = memory;
                            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                            bitmapImage.EndInit();

                            // Устанавливаем QR-код в элемент Image
                            if (QrCodeImage != null)
                            {
                                QrCodeImage.Source = bitmapImage;
                            }
                        }
                    }
                }
            }
        }

        private void ApplyPromoCodeButton_Click(object sender, RoutedEventArgs e)
        {
            string promoCode = PromoCodeTextBox.Text;
            using (var context = new StuldastolEntities())
            {
                var promo = context.PromoCodes.FirstOrDefault(p => p.Code == promoCode && p.ValidUntil >= DateTime.Now);
                if (promo != null)
                {
                    decimal discountPercentage = promo.DiscountPercentage;
                    promoDiscount = (subtotal + deliveryCost) * (discountPercentage / 100m);
                    PromoCodeId = promo.Id;
                    DonePromoWindow donePromoWindow = new DonePromoWindow();
                    donePromoWindow.ShowDialog();
                }
                else
                {
                    promoDiscount = 0m;
                    PromoCodeId = null;
                    FalsePromoWindow falsePromoWindow = new FalsePromoWindow();
                    falsePromoWindow.ShowDialog();
                }
            }
            CalculateTotals();
            GenerateQrCode(); // Обновляем QR-код после изменения суммы
        }

        private void PaymentOption_Checked(object sender, RoutedEventArgs e)
        {
            isFullPayment = FullPaymentRadioButton.IsChecked == true;
            CalculateTotals();
            GenerateQrCode(); // Обновляем QR-код после изменения суммы
        }

        private void ConfirmPaymentButton_Click(object sender, RoutedEventArgs e)
        {
            DonePayWindow donePayWindow = new DonePayWindow();
            donePayWindow.ShowDialog();
            DialogResult = true;
            Close();
        }
        private void GoToCatalogButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}