using StuldastolClient.Services;
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

namespace StuldastolClient.Views
{
    /// <summary>
    /// Логика взаимодействия для StartWindow.xaml
    /// </summary>
    public partial class StartWindow : Window
    {
        private readonly ICatalogService _catalogService;
        private readonly ICartService _cartService;
        private readonly StuldastolEntities _context;

        public StartWindow()
        {
            InitializeComponent();
            _context = new StuldastolEntities(); // Создаём новый контекст, если он нужен
            _catalogService = new CatalogService();
            _cartService = new CartService();
        }

        private void ConsultButton_Click(object sender, RoutedEventArgs e)
        {
            ConsultWindow consultWindow = new ConsultWindow();
            consultWindow.Show();
        }

        private void ContactButton_Click(object sender, RoutedEventArgs e)
        {
            ContactWindow contactWindow = new ContactWindow();
            contactWindow.Show();
        }

        private void CatalogButton_Click(object sender, RoutedEventArgs e)
        {
            CatalogMainWindow catalogWindow = new CatalogMainWindow(_catalogService, _cartService, _context);
            catalogWindow.Show();
            this.Close();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }
    }
}