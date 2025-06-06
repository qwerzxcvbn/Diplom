using Microsoft.Win32;
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
    public partial class EditProductWindow : Window
    {
        private Products product;
        private string imagePath;

        public EditProductWindow(Products product)
        {
            InitializeComponent();
            this.product = product;
            imagePath = product.ImagePath;
            LoadCategories();
            LoadProductData();
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

        private void LoadProductData()
        {
            NameTextBox.Text = product.Name;
            CategoryComboBox.SelectedValue = product.CategoryId;
            WoodTypeTextBox.Text = product.WoodType;
            OilColorTextBox.Text = product.OilColor;
            FabricTypeTextBox.Text = product.FabricType;
            DescriptionTextBox.Text = product.Description;
            PriceTextBox.Text = product.Price.ToString("N2");
            ImagePathTextBlock.Text = System.IO.Path.GetFileName(product.ImagePath) ?? "Фото не выбрано";
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
                    WoodTypeTextBox.IsEnabled = true;
                    OilColorTextBox.IsEnabled = true;
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
            // Валидация
            if (string.IsNullOrWhiteSpace(NameTextBox.Text) ||
                CategoryComboBox.SelectedItem == null ||
                string.IsNullOrWhiteSpace(WoodTypeTextBox.Text) ||
                string.IsNullOrWhiteSpace(OilColorTextBox.Text) ||
                string.IsNullOrWhiteSpace(FabricTypeTextBox.Text) ||
                string.IsNullOrWhiteSpace(PriceTextBox.Text) ||
                string.IsNullOrWhiteSpace(imagePath))
            {
                MessageBox.Show("Заполните все поля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!decimal.TryParse(PriceTextBox.Text, out decimal price) || price <= 0)
            {
                MessageBox.Show("Введите корректную цену.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Формируем шаблонное описание
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
                var productToUpdate = context.Products.FirstOrDefault(p => p.Id == product.Id);
                if (productToUpdate != null)
                {
                    productToUpdate.Name = NameTextBox.Text;
                    productToUpdate.Description = description;
                    productToUpdate.Price = price;
                    productToUpdate.WoodType = WoodTypeTextBox.Text;
                    productToUpdate.OilColor = OilColorTextBox.Text;
                    productToUpdate.FabricType = FabricTypeTextBox.Text;
                    productToUpdate.CategoryId = (int)CategoryComboBox.SelectedValue;
                    productToUpdate.ImagePath = imagePath;

                    context.SaveChanges();

                    MessageBox.Show("Товар успешно обновлен.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.Close();
                }
            }
        }
    }
}