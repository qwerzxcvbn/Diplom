using StuldastolClient.Views;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using StuldastolClient.MessageWindow;
using StuldastolClient.Views.WindowAdmin;
using StuldastolClient.Services;

namespace StuldastolClient
{
    public partial class MainWindow : Window
    {
        private readonly IUserService _userService;
        private readonly StuldastolEntities _context;
        public MainWindow(IUserService userService)
        {
            InitializeComponent();
            _userService = userService;
            _context = new StuldastolEntities();
        }

        public MainWindow() : this(new UserService())
        {
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginTextBox.Text;
            string password = PasswordTextBox.Password;

            if (_userService.AuthenticateUser(login, password))
            {
                int roleId = _userService.GetUserRole(CurrentUser.UserId);
                switch (roleId)
                {
                    case 1: // Customer
                        StartWindow startWindow = new StartWindow();
                        startWindow.Show();
                        this.Close();
                        break;

                    case 2: // Admin
                    case 3: // Manager
                        AdminManagerWindow adminManagerWindow = new AdminManagerWindow(roleId);
                        adminManagerWindow.Show();
                        this.Close();
                        break;
                    default:
                        break;
                }
            }
            else
            {
                var falseAuthWindow = new FalseAuthWindow();
                falseAuthWindow.ShowDialog();
            }
        }

        private void RegisterLink_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var registerWindow = new RegisterWindow(new UserService());
            registerWindow.Show();
            this.Close();
        }

        private void ResetPasswordLink_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var resetPasswordWindow = new ResetPasswordWindow();
            resetPasswordWindow.Show();
            this.Close();
        }
    }
}