using StuldastolProduction.MessageWindow;
using StuldastolProduction.Views;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace StuldastolProduction
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginTextBox.Text;
            string password = PasswordTextBox.Password;
            using (var context = new StuldastolEntities())
            {
                var user = context.Users.FirstOrDefault(u => u.Login == login && u.PasswordHash == password && (u.RoleId == 2 || u.RoleId == 4));
                if (user != null)
                {
                    CurrentUser.SetUser(user.Id, user.Login, user.Email);

                    if (user.RoleId == 2) // Администратор
                    {
                        var orderListWindow = new OrderListWindow();
                        orderListWindow.Show();
                        this.Close();
                    }
                    else if (user.RoleId == 4) // Рабочий цеха
                    {
                        var orderListWindow = new OrderListWindow();
                        orderListWindow.Show();
                        this.Close();
                    }
                }
                else
                {
                    FalseAuthWindow falseAuthWindow = new FalseAuthWindow();
                    falseAuthWindow.ShowDialog();
                }
            }
        }
    }
}