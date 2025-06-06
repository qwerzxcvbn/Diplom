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
    public partial class CreatePromoCodeWindow : Window
    {
        public CreatePromoCodeWindow()
        {
            InitializeComponent();
        }
        private void GoToCatalogButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Валидация
            if (string.IsNullOrWhiteSpace(CodeTextBox.Text) ||
                string.IsNullOrWhiteSpace(DiscountTextBox.Text) ||
                ValidUntilDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Заполните все поля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!decimal.TryParse(DiscountTextBox.Text, out decimal discount) || discount <= 0 || discount > 100)
            {
                MessageBox.Show("Введите корректную скидку (0-100%).", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            DateTime validUntil = ValidUntilDatePicker.SelectedDate.Value;
            if (validUntil < DateTime.Now)
            {
                MessageBox.Show("Срок действия должен быть в будущем.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            using (var context = new StuldastolEntities())
            {
                var promoCode = new PromoCodes
                {
                    Code = CodeTextBox.Text,
                    DiscountPercentage = discount,
                    ValidUntil = validUntil,
                    CreatedBy = CurrentUser.UserId,
                    CreatedAt = DateTime.Now
                };

                context.PromoCodes.Add(promoCode);
                try
                {
                    context.SaveChanges();
                    MessageBox.Show("Промокод успешно создан.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при создании промокода: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}