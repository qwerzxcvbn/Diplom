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
    public partial class ConsultationRequestsWindow : Window
    {
        public ConsultationRequestsWindow()
        {
            InitializeComponent();
            LoadConsultationRequests();
        }

        private void LoadConsultationRequests()
        {
            using (var context = new StuldastolEntities())
            {
                var requests = context.ConsultationRequests
                    .Select(cr => new
                    {
                        cr.Id,
                        cr.Name,
                        cr.PhoneNumber,
                        cr.Topic,
                        cr.CreatedAt
                    })
                    .ToList();

                ConsultationRequestsItemsControl.ItemsSource = requests;
            }
        }

        private void GoToCatalogButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                var dataContext = button.DataContext as dynamic; // Используем dynamic для доступа к анонимному типу
                if (dataContext != null && dataContext.Id != null)
                {
                    int requestId = dataContext.Id;

                    var result = MessageBox.Show("Вы уверены, что хотите удалить эту заявку?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        using (var context = new StuldastolEntities())
                        {
                            var requestToDelete = context.ConsultationRequests.FirstOrDefault(cr => cr.Id == requestId);
                            if (requestToDelete != null)
                            {
                                context.ConsultationRequests.Remove(requestToDelete);
                                context.SaveChanges();
                                LoadConsultationRequests();
                                MessageBox.Show("Заявка успешно удалена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }
                    }
                }
            }
        }
    }
}