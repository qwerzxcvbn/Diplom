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
    public partial class CreateUserWindow : Window
    {
        public CreateUserWindow()
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
            if (string.IsNullOrWhiteSpace(LoginTextBox.Text) ||
                string.IsNullOrWhiteSpace(EmailTextBox.Text) ||
                string.IsNullOrWhiteSpace(PasswordBox.Password) ||
                RoleComboBox.SelectedItem == null ||
                string.IsNullOrWhiteSpace(FirstNameTextBox.Text) ||
                string.IsNullOrWhiteSpace(LastNameTextBox.Text))
            {
                MessageBox.Show("Заполните все поля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            using (var context = new StuldastolEntities())
            {
                // Проверяем уникальность логина и email
                if (context.Users.Any(u => u.Login == LoginTextBox.Text))
                {
                    MessageBox.Show("Пользователь с таким логином уже существует.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (context.Users.Any(u => u.Email == EmailTextBox.Text))
                {
                    MessageBox.Show("Пользователь с таким email уже существует.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var user = new Users
                {
                    Login = LoginTextBox.Text,
                    Email = EmailTextBox.Text,
                    PasswordHash = PasswordBox.Password, // В реальном приложении нужно хешировать пароль
                    RoleId = int.Parse(((ComboBoxItem)RoleComboBox.SelectedItem).Tag.ToString()),
                    FirstName = FirstNameTextBox.Text,
                    LastName = LastNameTextBox.Text,
                    CreatedAt = DateTime.Now
                };

                context.Users.Add(user);
                context.SaveChanges();

                MessageBox.Show("Пользователь успешно создан.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
        }
    }
}