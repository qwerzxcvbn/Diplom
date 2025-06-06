using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using MailKit.Net.Smtp;
using MimeKit;
using StuldastolClient.MessageWindow;

namespace StuldastolClient.Views
{
    /// <summary>
    /// Логика взаимодействия для ResetPasswordWindow.xaml
    /// </summary>
    public partial class ResetPasswordWindow : Window
    {
        private string _email;
        private string _code;

        public ResetPasswordWindow()
        {
            InitializeComponent();
        }  
        private void SendCodeButton_Click(object sender, RoutedEventArgs e)
        {
            _email = EmailTextBox.Text;
            if (!string.IsNullOrEmpty(_email))
            {
                using (var context = new StuldastolEntities())
                {
                    var user = context.Users.FirstOrDefault(u => u.Email == _email);
                    if (user != null)
                    {
                        _code = new Random().Next(100000, 999999).ToString(); // Генерация 6-значного кода
                        SendEmail(_email, _code);
                        TextBlockEmail.Visibility = System.Windows.Visibility.Collapsed;
                        EmailTextBox.Visibility = System.Windows.Visibility.Collapsed;
                        SendCodeButton.Visibility = System.Windows.Visibility.Collapsed;
                        CodeTextBox.Visibility = System.Windows.Visibility.Visible;
                        NewPasswordTextBox.Visibility = System.Windows.Visibility.Visible;
                        SaveButton.Visibility = System.Windows.Visibility.Visible;
                        TextBlockCode.Visibility = System.Windows.Visibility.Visible;
                        TextBlockNewPassword.Visibility = System.Windows.Visibility.Visible;
                        TitleTextBlock.Text = "ВОССТАНОВЛЕНИЕ ПАРОЛЯ";
                    }
                    else
                    {
                        var falseEmailWindow = new FalseEmailWindow();
                        falseEmailWindow.ShowDialog();
                        return;
                    }
                }
            }
            else
            {
                var falseInputEmailWindow = new FalseInputEmailWindow();
                falseInputEmailWindow.ShowDialog();
                return;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string enteredCode = CodeTextBox.Text;
            string newPassword = NewPasswordTextBox.Password;

            if (enteredCode == _code)
            {
                using (var context = new StuldastolEntities())
                {
                    var user = context.Users.FirstOrDefault(u => u.Email == _email);
                    if (user != null)
                    {
                        user.PasswordHash = newPassword; // В реальном проекте используйте хэширование
                        context.SaveChanges();
                        var donePassWindow = new DonePassWindow();
                        donePassWindow.ShowDialog();
                        var loginWindow = new MainWindow();
                        loginWindow.Show();
                        this.Close();
                    }
                }
            }
            else
            {
                var falseCodeWindow = new FasleCodeWindow();
                falseCodeWindow.ShowDialog();
                return;
            }
        }

        private void SendEmail(string email, string code)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Stuldastol", "Shopnowopt1@yandex.com"));
            message.To.Add(new MailboxAddress("", email));
            message.Subject = "Код восстановления пароля";
            message.Body = new TextPart("plain") { Text = $"Ваш код восстановления: {code}" };

            using (var client = new MailKit.Net.Smtp.SmtpClient())
            {
                client.Connect("smtp.yandex.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                client.Authenticate("Shopnowopt1@yandex.com", "tarkskdfkuchdpod");
                client.Send(message);
                client.Disconnect(true);
            }
        }

        private void BackButton_Click_1(object sender, RoutedEventArgs e)
        {
            var loginWindow = new MainWindow();
            loginWindow.Show();
            this.Close();
        }
    }
}