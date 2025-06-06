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

namespace StuldastolClient.Views
{
    public partial class AdminManagerWindow : Window
    {
        private int roleId;

        public AdminManagerWindow(int roleId)
        {
            InitializeComponent();
            this.roleId = roleId;

            if (roleId == 2)
            {
                EmployeesButton.Visibility = Visibility.Visible;
            }

            LoadProducts();
        }

        private void LoadProducts()
        {
            using (var context = new StuldastolEntities())
            {
                var products = context.Products.ToList();
                ProductsItemsControl.ItemsSource = products;
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Products product)
            {
                var editProductWindow = new EditProductWindow(product);
                editProductWindow.ShowDialog();
                LoadProducts();
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {

            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is Products product)
            {
                var detailsWindow = new ProductDetailsWindow(product)
                {
                    Owner = this
                };
                detailsWindow.ShowDialog();
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Products product)
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить этот товар?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    using (var context = new StuldastolEntities())
                    {
                        var productToDelete = context.Products.FirstOrDefault(p => p.Id == product.Id);
                        if (productToDelete != null)
                        {
                            context.Products.Remove(productToDelete);
                            context.SaveChanges();
                            LoadProducts();
                        }
                    }
                }
            }
        }

        private void ConsultationRequestsButton_Click(object sender, RoutedEventArgs e)
        {
            var consultationRequestsWindow = new ConsultationRequestsWindow();
            consultationRequestsWindow.ShowDialog();
        }

        private void AddProductButton_Click(object sender, RoutedEventArgs e)
        {
            var addProductWindow = new AddProductWindow();
            addProductWindow.ShowDialog();
            LoadProducts();
        }

        private void OrdersButton_Click(object sender, RoutedEventArgs e)
        {
            var ordersWindow = new OrdersAdminWindow();
            ordersWindow.ShowDialog();
        }

        private void PromoCodesButton_Click(object sender, RoutedEventArgs e)
        {
            var promoCodesWindow = new PromoCodesWindow();
            promoCodesWindow.ShowDialog();
        }

        private void EmployeesButton_Click(object sender, RoutedEventArgs e)
        {
            var employeesWindow = new EmployeesWindow();
            employeesWindow.ShowDialog();
        }
    }
}