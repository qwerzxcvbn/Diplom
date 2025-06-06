using StuldastolClient.MessageWindow;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace StuldastolClient.Views
{
    /// <summary>
    /// Логика взаимодействия для ConsultWindow.xaml
    /// </summary>
    public partial class ConsultWindow : Window
    {
        public ConsultWindow()
        {
            InitializeComponent();
        }

        private void SendRequestButton_Click(object sender, RoutedEventArgs e)
        {
            string name = NameTB.Text;
            string phoneNumber = NumberPhoneTB.Text;
            string topic = TopicTB.Text;

            // Проверка заполненности всех полей
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(phoneNumber) || string.IsNullOrWhiteSpace(topic))
            {
                FalseConsultWindow falseConsultWindow = new FalseConsultWindow();
                falseConsultWindow.ShowDialog();
                return;
            }

            // Проверка номера телефона (только цифры и символы +()-)
            if (!Regex.IsMatch(phoneNumber, @"^[\d\s+()-]+$"))
            {
                FalseNumberPhoneWindow falseNumberPhoneWindow = new FalseNumberPhoneWindow();
                falseNumberPhoneWindow.ShowDialog();
                return;
            }

            // Сохранение заявки в базу данных
            try
            {
                using (var context = new StuldastolEntities())
                {
                    var request = new ConsultationRequests
                    {
                        Name = name,
                        PhoneNumber = phoneNumber,
                        Topic = topic,
                        CreatedAt = DateTime.Now
                    };

                    context.ConsultationRequests.Add(request);
                    context.SaveChanges();

                    DoneConsultWindow doneConsultWindow = new DoneConsultWindow();
                    doneConsultWindow.ShowDialog();
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при отправке заявки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}