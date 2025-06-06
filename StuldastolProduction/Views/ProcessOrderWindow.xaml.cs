using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MailKit.Net.Smtp;
using MimeKit;

namespace StuldastolProduction.Views
{
    public partial class ProcessOrderWindow : Window
    {
        private ObservableCollection<TaskViewModel> tasks;
        private ProductionOrderViewModel order;

        public ProcessOrderWindow(ProductionOrderViewModel order)
        {
            InitializeComponent();
            this.order = order;
            tasks = new ObservableCollection<TaskViewModel>();
            TasksItemsControl.ItemsSource = tasks;
            LoadTasks();

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

        private void LoadTasks()
        {
            using (var context = new StuldastolEntities())
            {
                var product = context.Products.FirstOrDefault(p => p.Id == order.Product.Id);
                if (product != null && (product.CategoryId == 1 || product.CategoryId == 2 || product.CategoryId == 3)) // Стул Нара, Стул Киото или Стол
                {
                    tasks.Clear();

                    // Получаем текущий статус до выполнения LINQ-запроса
                    int currentStatusId = GetCurrentStatusId();
                    Console.WriteLine($"Current Status ID: {currentStatusId}"); // Отладка

                    // Проверяем, есть ли существующие задачи в базе для этого ProductionOrder
                    var existingTasks = context.ProductionTasks
                        .Where(pt => pt.ProductionOrderId == order.Id)
                        .Select(pt => new TaskViewModel
                        {
                            Id = pt.Id,
                            TaskName = pt.TaskName,
                            StatusId = pt.StatusId,
                            ImagePath = pt.ImagePath,
                            TotalCount = pt.TotalCount,
                            CompletedCount = pt.CompletedCount ?? 0, // Если null, устанавливаем 0
                            IsEnabled = false // Изначально все задачи неактивны
                        }).ToList();

                    if (existingTasks.Any())
                    {
                        foreach (var task in existingTasks)
                        {
                            Console.WriteLine($"Task: {task.TaskName}, StatusId: {task.StatusId}, Completed: {task.CompletedCount}/{task.TotalCount}, IsEnabled: {task.IsEnabled}"); // Отладка
                            tasks.Add(task);
                        }
                    }
                    else
                    {
                        // Определяем этапы в зависимости от категории
                        var tasksList = product.CategoryId == 3 // Стол
                            ? new[]
                            {
                                new TaskViewModel { TaskName = "Склейка", StatusId = 7, ImagePath = "/Images/skleika.png", TotalCount = order.Quantity },
                                new TaskViewModel { TaskName = "Шлифовка", StatusId = 2, ImagePath = "/Images/shlifovka.png", TotalCount = order.Quantity },
                                new TaskViewModel { TaskName = "Покраска", StatusId = 3, ImagePath = "/Images/pokraska.png", TotalCount = order.Quantity },
                                new TaskViewModel { TaskName = "Упаковка", StatusId = 8, ImagePath = "/Images/upakovka.png", TotalCount = order.Quantity }
                            }
                            : new[] // Стул Нара или Стул Киото
                            {
                                new TaskViewModel { TaskName = "Склейка каркасов", StatusId = 7, ImagePath = "/Images/skleika.png", TotalCount = order.Quantity },
                                new TaskViewModel { TaskName = "Шлифовка каркасов", StatusId = 2, ImagePath = "/Images/shlifovka.png", TotalCount = order.Quantity },
                                new TaskViewModel { TaskName = "Обивка сидушки", StatusId = 4, ImagePath = "/Images/obivka.png", TotalCount = order.Quantity },
                                new TaskViewModel { TaskName = "Покраска", StatusId = 3, ImagePath = "/Images/pokraska.png", TotalCount = order.Quantity },
                                new TaskViewModel { TaskName = "Упаковка", StatusId = 8, ImagePath = "/Images/upakovka.png", TotalCount = order.Quantity }
                            };

                        foreach (var task in tasksList)
                        {
                            var newTask = new ProductionTasks
                            {
                                ProductionOrderId = order.Id,
                                TaskName = task.TaskName,
                                StatusId = task.StatusId,
                                ImagePath = task.ImagePath,
                                TotalCount = task.TotalCount,
                                CompletedCount = 0
                            };
                            context.ProductionTasks.Add(newTask);
                            context.SaveChanges();
                            task.Id = newTask.Id; // Устанавливаем Id для последующего обновления
                            tasks.Add(task);
                        }
                    }

                    // Определяем порядок этапов
                    var statusOrder = product.CategoryId == 3
                        ? new[] { 7, 2, 3, 8, 5 } // Порядок этапов для столов
                        : new[] { 7, 2, 4, 3, 8, 5 }; // Порядок этапов для стульев
                    int currentIndex = Array.IndexOf(statusOrder, currentStatusId);

                    // Активируем только текущий этап, если все предыдущие полностью завершены
                    if (currentIndex >= 0 && currentIndex < statusOrder.Length)
                    {
                        bool allPreviousCompleted = true;
                        for (int i = 0; i < currentIndex; i++)
                        {
                            var previousTask = tasks.FirstOrDefault(t => t.StatusId == statusOrder[i]);
                            if (previousTask != null && previousTask.CompletedCount < previousTask.TotalCount)
                            {
                                allPreviousCompleted = false;
                                break;
                            }
                        }

                        if (allPreviousCompleted)
                        {
                            var currentTask = tasks.FirstOrDefault(t => t.StatusId == statusOrder[currentIndex]);
                            if (currentTask != null)
                            {
                                currentTask.IsEnabled = true;
                            }
                        }
                    }

                    // Дополнительная отладка после установки IsEnabled
                    foreach (var task in tasks)
                    {
                        Console.WriteLine($"After LoadTasks - Task: {task.TaskName}, StatusId: {task.StatusId}, IsEnabled: {task.IsEnabled}"); // Отладка
                    }
                }
            }
        }

        private int GetCurrentStatusId()
        {
            using (var context = new StuldastolEntities())
            {
                var productionOrder = context.ProductionOrders.FirstOrDefault(po => po.Id == order.Id);
                return productionOrder?.CurrentStatusId ?? 7; // Начальный статус - Склейка каркасов
            }
        }

        private string GetClientEmail(int orderId)
        {
            using (var context = new StuldastolEntities())
            {
                var order = context.Orders.FirstOrDefault(o => o.Id == orderId);
                if (order != null)
                {
                    var user = context.Users.FirstOrDefault(u => u.Id == order.UserId);
                    return user?.Email;
                }
                return null;
            }
        }

        private string GetStatusName(int statusId)
        {
            using (var context = new StuldastolEntities())
            {
                var status = context.ProductionStatuses.FirstOrDefault(s => s.Id == statusId);
                return status?.Name ?? "Неизвестный статус";
            }
        }

        private void SendEmail(string email, string subject, string body)
        {
            if (string.IsNullOrEmpty(email))
            {
                Console.WriteLine("Не удалось отправить email: адрес получателя пустой.");
                return;
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Stuldastol", "Shopnowopt1@yandex.com"));
            message.To.Add(new MailboxAddress("", email));
            message.Subject = subject;
            message.Body = new TextPart("plain") { Text = body };

            try
            {
                using (var client = new MailKit.Net.Smtp.SmtpClient())
                {
                    client.Connect("smtp.yandex.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                    client.Authenticate("Shopnowopt1@yandex.com", "tarkskdfkuchdpod");
                    client.Send(message);
                    client.Disconnect(true);
                    Console.WriteLine($"Email успешно отправлен на {email}. Тема: {subject}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при отправке email: {ex.Message}");
                MessageBox.Show($"Ошибка при отправке уведомления: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CompleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is TaskViewModel task)
            {
                var quantityTextBox = (button.Parent as Grid)?.FindName("QuantityTextBox") as TextBox;
                if (quantityTextBox != null && int.TryParse(quantityTextBox.Text, out int completedCount))
                {
                    // Проверяем, чтобы сумма не превышала TotalCount
                    int newCompletedCount = task.CompletedCount + completedCount;
                    if (newCompletedCount <= task.TotalCount)
                    {
                        task.CompletedCount = newCompletedCount;

                        using (var context = new StuldastolEntities())
                        {
                            var dbTask = context.ProductionTasks.FirstOrDefault(pt => pt.Id == task.Id);
                            if (dbTask != null)
                            {
                                dbTask.CompletedCount = task.CompletedCount;

                                // Обновляем статус заказа только если текущий этап полностью завершён
                                if (task.CompletedCount == task.TotalCount)
                                {
                                    var productionOrder = context.ProductionOrders.FirstOrDefault(po => po.Id == order.Id);
                                    if (productionOrder != null)
                                    {
                                        int nextStatusId = GetNextStatusId(task.StatusId);
                                        Console.WriteLine($"Current Task StatusId: {task.StatusId}, Next StatusId: {nextStatusId}"); // Отладка

                                        // Получаем email клиента перед обновлением статуса
                                        string clientEmail = GetClientEmail(productionOrder.OrderId);
                                        string oldStatusName = GetStatusName(productionOrder.CurrentStatusId);
                                        productionOrder.CurrentStatusId = nextStatusId;
                                        string newStatusName = GetStatusName(nextStatusId);

                                        context.ProductionStatusLogs.Add(new ProductionStatusLogs
                                        {
                                            ProductionOrderId = productionOrder.Id,
                                            StatusId = productionOrder.CurrentStatusId,
                                            ChangedAt = DateTime.Now,
                                            ChangedBy = CurrentUser.UserId
                                        });

                                        // Отправляем уведомление о смене этапа производства
                                        if (!string.IsNullOrEmpty(clientEmail))
                                        {
                                            string subject = "Обновление статуса производства";
                                            string body = $"Уважаемый клиент,\n\nСтатус производства вашего заказа (ID: {productionOrder.OrderId}) изменился.\n" +
                                                          $"Новый статус: {newStatusName}\n\n" +
                                                          $"Дата обновления: {DateTime.Now:dd.MM.yyyy HH:mm}\n\n" +
                                                          "С уважением,\nStuldastol";
                                            SendEmail(clientEmail, subject, body);
                                        }
                                    }
                                }

                                try
                                {
                                    context.SaveChanges();
                                    Console.WriteLine($"Task updated: CompletedCount = {task.CompletedCount}/{task.TotalCount}, StatusId = {task.StatusId}"); // Отладка
                                    if (task.CompletedCount == task.TotalCount)
                                    {
                                        Console.WriteLine($"Status updated to {GetCurrentStatusId()} and saved to database."); // Отладка
                                    }
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                    Console.WriteLine($"Error saving: {ex}"); // Отладка
                                }
                            }
                        }

                        // Сбрасываем значение в TextBox после выполнения
                        quantityTextBox.Text = "0";

                        // Обновляем список задач
                        LoadTasks();
                    }
                    else
                    {
                        MessageBox.Show("Введенное количество превышает общее количество.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                else
                {
                    MessageBox.Show("Введите корректное количество.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }

            // Проверка завершения
            if (GetCurrentStatusId() == 5) // Завершенный
            {
                try
                {
                    UpdateOrderStatus();
                    var orderListWindow = new OrderListWindow();
                    orderListWindow.Show();
                    Console.WriteLine("OrderListWindow opened successfully."); // Отладка
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при открытии OrderListWindow: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    Console.WriteLine($"Error opening OrderListWindow: {ex}"); // Отладка
                }
            }
        }

        private int GetNextStatusId(int currentStatusId)
        {
            var statusOrder = new[] { 7, 2, 4, 3, 8, 5 }; // Порядок этапов для стульев
            var tableStatusOrder = new[] { 7, 2, 3, 8, 5 }; // Порядок этапов для столов
            var product = new StuldastolEntities().Products.FirstOrDefault(p => p.Id == order.Product.Id);
            Console.WriteLine($"Product CategoryId: {product?.CategoryId}"); // Отладка

            var statusArray = product?.CategoryId == 3 ? tableStatusOrder : statusOrder; // Выбор порядка этапов
            int currentIndex = Array.IndexOf(statusArray, currentStatusId);
            Console.WriteLine($"Current Status Index: {currentIndex}, Status Array: [{string.Join(", ", statusArray)}]"); // Отладка

            return currentIndex < statusArray.Length - 1 && currentIndex >= 0 ? statusArray[currentIndex + 1] : 5;
        }

        private void UpdateOrderStatus()
        {
            using (var context = new StuldastolEntities())
            {
                var productionOrder = context.ProductionOrders.FirstOrDefault(po => po.Id == order.Id);
                if (productionOrder != null)
                {
                    var relatedOrder = context.Orders.FirstOrDefault(o => o.Id == productionOrder.OrderId);
                    if (relatedOrder != null)
                    {
                        // Проверяем, все ли ProductionOrders в этом заказе завершены
                        var allCompleted = context.ProductionOrders
                            .Where(po => po.OrderId == relatedOrder.Id)
                            .All(po => po.CurrentStatusId == 5);

                        if (allCompleted)
                        {
                            int oldStatusId = relatedOrder.StatusId;
                            relatedOrder.StatusId = productionOrder.IsSelfPickup ? 2 : 3; // 2 - Готов к выдаче, 3 - Отправленный
                            context.SaveChanges();
                            Console.WriteLine($"Order status updated to {relatedOrder.StatusId}."); // Отладка

                            // Отправляем уведомление о смене статуса заказа
                            string clientEmail = GetClientEmail(relatedOrder.Id);
                            if (!string.IsNullOrEmpty(clientEmail))
                            {
                                string oldStatusName = GetStatusName(oldStatusId);
                                string newStatusName = GetStatusName(relatedOrder.StatusId);
                                string subject = "Обновление статуса заказа";
                                string body = $"Уважаемый клиент,\n\nСтатус вашего заказа (ID: {relatedOrder.Id}) изменился.\n" +
                                              $"Новый статус: {newStatusName}\n\n" +
                                              $"Дата обновления: {DateTime.Now:dd.MM.yyyy HH:mm}\n\n" +
                                              "С уважением,\nStuldastol";
                                SendEmail(clientEmail, subject, body);
                            }
                        }
                    }
                }
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var activeOrdersWindow = new ActiveOrdersWindow();
            activeOrdersWindow.Show();
            this.Close();
        }
    }

    public class TaskViewModel
    {
        public int Id { get; set; } // Добавляем Id для связи с базой
        public string TaskName { get; set; }
        public int StatusId { get; set; }
        public string ImagePath { get; set; }
        public int TotalCount { get; set; }
        public int CompletedCount { get; set; }
        public bool IsEnabled { get; set; }
    }

    public class ProductionOrderViewModel
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public Products Product { get; set; }
        public int Quantity { get; set; }
        public string WoodType { get; set; }
        public string OilColor { get; set; }
        public string FabricType { get; set; }
        public DateTime Deadline { get; set; }
        public bool IsSelfPickup { get; set; }
        public UserViewModel AssignedToUser { get; set; }
    }
}