using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace StuldastolProduction.Views
{
    public partial class ActiveOrdersWindow : Window, INotifyPropertyChanged
    {
        private ObservableCollection<ProductionOrderViewModel> orders;
        private bool _isNotAdmin;

        public bool IsNotAdmin
        {
            get => _isNotAdmin;
            set
            {
                _isNotAdmin = value;
                OnPropertyChanged(nameof(IsNotAdmin));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ActiveOrdersWindow()
        {
            InitializeComponent();
            DataContext = this; // Устанавливаем DataContext на само окно

            orders = new ObservableCollection<ProductionOrderViewModel>();
            OrdersItemsControl.ItemsSource = orders;

            // Определяем роль пользователя
            int role = GetCurrentUserRole();
            Console.WriteLine($"User Role: {role}"); // Отладка
            IsNotAdmin = role != 2; // Устанавливаем значение свойства

            LoadOrders();

            // Отображаем информацию о пользователе
            using (var context = new StuldastolEntities())
            {
                var user = context.Users.FirstOrDefault(u => u.Id == CurrentUser.UserId);
                if (user != null)
                {
                    UserInfoTextBlock.Text = $"ID {user.Id} | {user.LastName} {user.FirstName}";
                }
            }
        }

        private void LoadOrders()
        {
            using (var context = new StuldastolEntities())
            {
                var query = context.ProductionOrders
                    .Where(po => po.CurrentStatusId != 1 && po.CurrentStatusId != 5) // Активные заказы
                    .Join(context.Users,
                        po => po.AssignedTo,
                        u => u.Id,
                        (po, u) => new { po, u });

                // Если пользователь - рабочий склада (RoleId == 4), показываем только его заказы
                int currentRole = GetCurrentUserRole();
                if (currentRole == 4)
                {
                    query = query.Where(x => x.po.AssignedTo == CurrentUser.UserId);
                }

                var activeOrders = query
                    .Select(x => new ProductionOrderViewModel
                    {
                        Id = x.po.Id,
                        OrderId = x.po.OrderId,
                        Product = x.po.Products,
                        Quantity = x.po.Quantity,
                        WoodType = x.po.WoodType,
                        OilColor = x.po.OilColor,
                        FabricType = x.po.FabricType,
                        Deadline = x.po.Deadline,
                        IsSelfPickup = x.po.IsSelfPickup,
                        AssignedToUser = new UserViewModel
                        {
                            Id = x.u.Id,
                            FullName = x.u.LastName + " " + x.u.FirstName
                        }
                    }).ToList();

                orders.Clear();
                foreach (var order in activeOrders)
                {
                    orders.Add(order);
                }
            }
        }

        private int GetCurrentUserRole()
        {
            using (var context = new StuldastolEntities())
            {
                var user = context.Users.FirstOrDefault(u => u.Id == CurrentUser.UserId);
                if (user == null)
                {
                    Console.WriteLine($"User with ID {CurrentUser.UserId} not found.");
                    return 0;
                }
                return user.RoleId;
            }
        }

        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is ProductionOrderViewModel order)
            {
                var processWindow = new ProcessOrderWindow(order);
                processWindow.Show();
                this.Close();
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var orderListWindow = new OrderListWindow();
            orderListWindow.Show();
            this.Close();
        }
    }
}