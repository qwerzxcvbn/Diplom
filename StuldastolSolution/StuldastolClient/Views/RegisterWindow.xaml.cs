using StuldastolClient.MessageWindow;
using System;
using System.Windows;
using System.Windows.Controls;
using StuldastolClient.Services;

namespace StuldastolClient.Views
{
    public partial class RegisterWindow : Window
    {
        private readonly IUserService _userService;

        public RegisterWindow(IUserService userService)
        {
            InitializeComponent();
            _userService = userService;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new MainWindow(new UserService());
            loginWindow.Show();
            this.Close();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string fullName = FullNameTextBox.Text;
            string phone = PhoneTextBox.Text;
            string email = AddressTextBox.Text;
            string login = LoginTextBox.Text;
            string password = PasswordTextBox.Password;

            try
            {
                // Проверка, занят ли email
                if (_userService.IsEmailTaken(email))
                {
                    var falseEmailRegWindow = new FalseEmailRegWindow();
                    falseEmailRegWindow.ShowDialog();
                    return; // Прерываем регистрацию
                }

                // Если email не занят, продолжаем регистрацию
                _userService.RegisterUser(fullName, phone, email, login, password);
                var doneRegWindow = new DoneRegWindow();
                if (doneRegWindow.ShowDialog() == true)
                {
                    var loginWindow = new MainWindow(new UserService());
                    loginWindow.Show();
                    this.Close();
                }
            }
            catch (Exception)
            {
                var falseRegWindow = new FalseRegWindow();
                falseRegWindow.ShowDialog();
            }
        }
    }
}