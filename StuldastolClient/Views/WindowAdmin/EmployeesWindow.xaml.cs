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
using System.Data.Entity; // Для использования Include

namespace StuldastolClient.Views.WindowAdmin
{
    public partial class EmployeesWindow : Window
    {
        public EmployeesWindow()
        {
            InitializeComponent();
            LoadEmployees();
        }

        private void LoadEmployees()
        {
            using (var context = new StuldastolEntities())
            {
                var employees = context.Users
                    .Include(u => u.Roles) // Загружаем связанные данные для Role
                    .Where(u => u.RoleId == 2 || u.RoleId == 3 || u.RoleId == 4) // Админ, Менеджер, Рабочий цеха
                    .ToList();
                EmployeesItemsControl.ItemsSource = employees;
            }
        }

        private void GoToCatalogButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void CreateEmployeeButton_Click(object sender, RoutedEventArgs e)
        {
            var createUserWindow = new CreateUserWindow();
            createUserWindow.ShowDialog();
            LoadEmployees();
        }

        private void DeleteEmployeeButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Users employee)
            {
                var result = MessageBox.Show($"Вы уверены, что хотите удалить сотрудника '{employee.FirstName} {employee.LastName}'?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    using (var context = new StuldastolEntities())
                    {
                        var employeeToDelete = context.Users.FirstOrDefault(u => u.Id == employee.Id);
                        if (employeeToDelete != null)
                        {
                            context.Users.Remove(employeeToDelete);
                            context.SaveChanges();
                            LoadEmployees();
                        }
                    }
                }
            }
        }
    }
}