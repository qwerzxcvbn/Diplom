using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using StuldastolClient.Services;

namespace StuldastolClient.Views
{
    public partial class CatalogMainWindow : Window
    {
        private ObservableCollection<Products> allProducts;
        private ObservableCollection<Categories> categories;
        private readonly ICatalogService _catalogService;
        private readonly ICartService _cartService;
        private readonly StuldastolEntities _context;
        private int? currentCategoryId;

        public CatalogMainWindow(ICatalogService catalogService, ICartService cartService, StuldastolEntities context)
        {
            InitializeComponent();
            _catalogService = catalogService;
            _cartService = cartService;
            _context = context ?? throw new ArgumentNullException(nameof(context));
            LoadData();
        }

        private void LoadData()
        {
            allProducts = _catalogService.GetProducts();
            categories = _catalogService.GetCategories();
            ProductsItemsControl.ItemsSource = allProducts;
            FilterStatusTextBlock.Text = "Все товары";

            // Загрузка типов дерева и масла
            LoadWoodTypes();
            LoadOilColors();
        }

        private void LoadWoodTypes()
        {
            var woodTypes = _catalogService.GetProducts()
                .Select(p => p.WoodType)
                .Distinct()
                .Where(w => !string.IsNullOrEmpty(w))
                .ToList();
            woodTypes.Insert(0, "Все");
            WoodTypeComboBox.ItemsSource = woodTypes;
            WoodTypeComboBox.SelectedIndex = 0;
        }

        private void LoadOilColors()
        {
            var oilColors = _catalogService.GetProducts()
                .Select(p => p.OilColor)
                .Distinct()
                .Where(o => !string.IsNullOrEmpty(o))
                .ToList();
            oilColors.Insert(0, "Все");
            OilColorComboBox.ItemsSource = oilColors;
            OilColorComboBox.SelectedIndex = 0;
        }

        private void NaraChairButton_Click(object sender, RoutedEventArgs e)
        {
            currentCategoryId = 1;
            FilterStatusTextBlock.Text = "Стул Нара";
            ApplyFilters();
        }

        private void KyotoChairButton_Click(object sender, RoutedEventArgs e)
        {
            currentCategoryId = 2;
            FilterStatusTextBlock.Text = "Стул Киото";
            ApplyFilters();
        }

        private void TablesButton_Click(object sender, RoutedEventArgs e)
        {
            currentCategoryId = 3;
            FilterStatusTextBlock.Text = "Столы";
            ApplyFilters();
        }

        private void ResetFilterButton_Click(object sender, RoutedEventArgs e)
        {
            currentCategoryId = null;
            ProductsItemsControl.ItemsSource = allProducts;
            FilterStatusTextBlock.Text = "Все товары";
            PriceFromTextBox.Text = string.Empty;
            PriceToTextBox.Text = string.Empty;
            WoodTypeComboBox.SelectedIndex = 0;
            OilColorComboBox.SelectedIndex = 0;
        }

        private void ApplyFiltersButton_Click(object sender, RoutedEventArgs e)
        {
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            decimal? priceFrom = null;
            decimal? priceTo = null;
            string woodType = WoodTypeComboBox.SelectedItem?.ToString();
            string oilColor = OilColorComboBox.SelectedItem?.ToString();

            if (!string.IsNullOrEmpty(PriceFromTextBox.Text) && decimal.TryParse(PriceFromTextBox.Text, out decimal pf))
            {
                priceFrom = pf;
            }
            if (!string.IsNullOrEmpty(PriceToTextBox.Text) && decimal.TryParse(PriceToTextBox.Text, out decimal pt))
            {
                priceTo = pt;
            }

            var filteredProducts = _catalogService.GetProducts(
                currentCategoryId,
                priceFrom,
                priceTo,
                woodType == "Все" ? null : woodType,
                oilColor == "Все" ? null : oilColor
            );

            ProductsItemsControl.ItemsSource = filteredProducts;
        }

        private void SortPriceAscButton_Click(object sender, RoutedEventArgs e)
        {
            var currentItems = ProductsItemsControl.ItemsSource as ObservableCollection<Products>;
            if (currentItems != null)
            {
                var sortedItems = new ObservableCollection<Products>(currentItems.OrderBy(p => p.Price));
                ProductsItemsControl.ItemsSource = sortedItems;
            }
        }

        private void SortPriceDescButton_Click(object sender, RoutedEventArgs e)
        {
            var currentItems = ProductsItemsControl.ItemsSource as ObservableCollection<Products>;
            if (currentItems != null)
            {
                var sortedItems = new ObservableCollection<Products>(currentItems.OrderByDescending(p => p.Price));
                ProductsItemsControl.ItemsSource = sortedItems;
            }
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

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            StartWindow startWindow = new StartWindow();
            startWindow.Show();
            this.Close();
        }

        private void GoToCartButton_Click(object sender, RoutedEventArgs e)
        {
            var cartWindow = new CartWindow(_cartService, new OrderService());
            cartWindow.Show();
        }

        private void AddCart_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Products product)
            {
                _cartService.AddToCart(product.Id, 1);
            }
        }

        private void MyOrdersButton_Click(object sender, RoutedEventArgs e)
        {
            int userId = CurrentUser.UserId;
            if (userId == 0)
            {
                return;
            }

            var ordersWindow = new OrdersWindow(new OrderService());
            ordersWindow.Show();
        }
    }
}