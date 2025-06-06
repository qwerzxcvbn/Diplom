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
    public partial class PromoCodesWindow : Window
    {
        public PromoCodesWindow()
        {
            InitializeComponent();
            LoadPromoCodes();
        }

        private void LoadPromoCodes()
        {
            using (var context = new StuldastolEntities())
            {
                var promoCodes = context.PromoCodes.ToList();
                PromoCodesItemsControl.ItemsSource = promoCodes;
            }
        }

        private void GoToCatalogButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void CreatePromoCodeButton_Click(object sender, RoutedEventArgs e)
        {
            var createPromoCodeWindow = new CreatePromoCodeWindow();
            createPromoCodeWindow.ShowDialog();
            LoadPromoCodes();
        }

        private void DeletePromoCodeButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is PromoCodes promoCode)
            {
                var result = MessageBox.Show($"Вы уверены, что хотите удалить промокод '{promoCode.Code}'?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    using (var context = new StuldastolEntities())
                    {
                        var promoCodeToDelete = context.PromoCodes.FirstOrDefault(p => p.Id == promoCode.Id);
                        if (promoCodeToDelete != null)
                        {
                            context.PromoCodes.Remove(promoCodeToDelete);
                            context.SaveChanges();
                            LoadPromoCodes();
                        }
                    }
                }
            }
        }
    }
}