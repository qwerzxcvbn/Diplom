using Microsoft.Win32;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
 

namespace StuldastolClient.Views.WindowAdmin
{
    public partial class AddProductWindow : Window
    {
        private string imagePath;

        public AddProductWindow()
        {
            InitializeComponent();
            LoadCategories();
            // Инициализируем видимость полей при открытии окна
            CategoryComboBox_SelectionChanged(null, null);
        }

        private void LoadCategories()
        {
            using (var context = new StuldastolEntities())
            {
                var categories = context.Categories.ToList();
                CategoryComboBox.ItemsSource = categories;
                CategoryComboBox.DisplayMemberPath = "Name";
                CategoryComboBox.SelectedValuePath = "Id";
            }
        }

        private void GoToCatalogButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void CategoryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CategoryComboBox.SelectedItem is Categories selectedCategory)
            {
                if (selectedCategory.Name == "Столы")
                {
                    DescriptionLabel.Visibility = Visibility.Visible;
                    DescriptionTextBox.Visibility = Visibility.Visible;
                    // Для столов скрываем поля, которые не нужны
                    WoodTypeTextBox.IsEnabled = false;
                    OilColorTextBox.IsEnabled = false;
                    FabricTypeTextBox.IsEnabled = false;
                }
                else
                {
                    DescriptionLabel.Visibility = Visibility.Collapsed;
                    DescriptionTextBox.Visibility = Visibility.Collapsed;
                    // Для стульев включаем поля
                    WoodTypeTextBox.IsEnabled = true;
                    OilColorTextBox.IsEnabled = true;
                    FabricTypeTextBox.IsEnabled = true;
                }
            }
            else
            {
                // Если категория не выбрана, скрываем поле описания
                DescriptionLabel.Visibility = Visibility.Collapsed;
                DescriptionTextBox.Visibility = Visibility.Collapsed;
                WoodTypeTextBox.IsEnabled = true;
                OilColorTextBox.IsEnabled = true;
                FabricTypeTextBox.IsEnabled = true;
            }
        }

        private void UploadImageButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.jpg, *.jpeg, *.png)|*.jpg;*.jpeg;*.png";
            if (openFileDialog.ShowDialog() == true)
            {
                imagePath = openFileDialog.FileName;
                ImagePathTextBlock.Text = System.IO.Path.GetFileName(imagePath);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Валидация общих полей
            if (string.IsNullOrWhiteSpace(NameTextBox.Text) ||
                CategoryComboBox.SelectedItem == null ||
                string.IsNullOrWhiteSpace(PriceTextBox.Text) ||
                string.IsNullOrWhiteSpace(imagePath))
            {
                MessageBox.Show("Заполните все обязательные поля (название, категория, цена, фото).", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!decimal.TryParse(PriceTextBox.Text, out decimal price) || price <= 0)
            {
                MessageBox.Show("Введите корректную цену.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string description;
            var selectedCategory = CategoryComboBox.SelectedItem as Categories;

            if (selectedCategory == null)
            {
                MessageBox.Show("Категория не выбрана. Пожалуйста, выберите категорию.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (selectedCategory.Name == "Стол")
            {
                // Для столов используем ручное описание
                if (string.IsNullOrWhiteSpace(DescriptionTextBox.Text))
                {
                    MessageBox.Show("Введите описание для стола.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                description = DescriptionTextBox.Text;
            }
            else if (selectedCategory.Name == "Стул Киото" || selectedCategory.Name == "Стул Нара")
            {
                // Для стульев проверяем поля и формируем автоматическое описание
                if (string.IsNullOrWhiteSpace(WoodTypeTextBox.Text) ||
                    string.IsNullOrWhiteSpace(OilColorTextBox.Text) ||
                    string.IsNullOrWhiteSpace(FabricTypeTextBox.Text))
                {
                    MessageBox.Show("Заполните поля: тип дерева, тип масла, ткань.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                description = $"Материал - {WoodTypeTextBox.Text}. " +
                              $"Цвет дерева - {OilColorTextBox.Text}. " +
                              $"Сиденье - {FabricTypeTextBox.Text}. " +
                              "Стулья покрыты маслом Biofa. Данное покрытие не шелушится и не растрескивается со временем, " +
                              "имеет на 100% экологичный состав безопасный для здоровья человека и экологии. " +
                              "Основа сиденья изготовлена из фанеры, наполнителем является высококачественный поролон ППУ 30 мм и синтепон 200г/м2.";
            }
            else
            {
                MessageBox.Show($"Выбрана неподдерживаемая категория: {selectedCategory.Name}. Поддерживаются только Стул Киото, Стул Нара и Стол.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            using (var context = new StuldastolEntities())
            {
                var product = new Products
                {
                    Name = NameTextBox.Text,
                    Description = description,
                    Price = price,
                    StockQuantity = 0,
                    WoodType = selectedCategory.Name == "Стол" ? null : WoodTypeTextBox.Text,
                    OilColor = selectedCategory.Name == "Стол" ? null : OilColorTextBox.Text,
                    FabricType = selectedCategory.Name == "Стол" ? null : FabricTypeTextBox.Text,
                    CreatedAt = DateTime.Now,
                    CategoryId = (int)CategoryComboBox.SelectedValue,
                    ImagePath = imagePath
                };

                context.Products.Add(product);
                context.SaveChanges();

                MessageBox.Show("Товар успешно добавлен.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
        }
    }
}