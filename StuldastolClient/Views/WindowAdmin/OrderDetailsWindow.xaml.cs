using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace StuldastolClient.Views.WindowAdmin
{
    public partial class OrderDetailsWindow : Window
    {
        public OrderDetailsWindow(int orderId)
        {
            InitializeComponent();
            LoadOrderDetails(orderId);
        }

        private void LoadOrderDetails(int orderId)
        {
            using (var context = new StuldastolEntities())
            {
                var orderDetails = (from order in context.Orders
                                    join user in context.Users on order.UserId equals user.Id
                                    join orderStatus in context.OrderStatuses on order.StatusId equals orderStatus.Id
                                    join productionOrder in context.ProductionOrders on order.Id equals productionOrder.OrderId
                                    join product in context.Products on productionOrder.ProductId equals product.Id
                                    join productionStatus in context.ProductionStatuses on productionOrder.CurrentStatusId equals productionStatus.Id
                                    join promoCode in context.PromoCodes on order.PromoCodeId equals promoCode.Id into pcGroup
                                    from promoCode in pcGroup.DefaultIfEmpty() // Левый JOIN, чтобы получить null, если промокод не применён
                                    where order.Id == orderId
                                    select new
                                    {
                                        OrderId = order.Id,
                                        UserFirstName = user.FirstName,
                                        UserLastName = user.LastName,
                                        UserPhone = user.Phone,
                                        IsSelfPickup = order.IsSelfPickup,
                                        DeliveryAddress = order.DeliveryAddress,
                                        OrderStatusName = orderStatus.Name,
                                        PromoCode = promoCode != null ? promoCode.Code : "Не применён", // Проверяем наличие промокода
                                        Product = product,
                                        ProductionStatusName = productionStatus.Name
                                    })
                                    .AsEnumerable()
                                    .GroupBy(o => o.OrderId)
                                    .Select(g => new
                                    {
                                        OrderIdText = "Заказ #" + g.Key.ToString(),
                                        UserFirstName = g.First().UserFirstName,
                                        UserLastName = g.First().UserLastName,
                                        UserPhone = g.First().UserPhone,
                                        DeliveryType = g.First().IsSelfPickup ? "Самовывоз" : "Доставка (" + (g.First().DeliveryAddress ?? "Не указан") + ")",
                                        OrderStatusName = g.First().OrderStatusName,
                                        PromoCode = g.First().PromoCode,
                                        Products = g.Select(x => new
                                        {
                                            Product = x.Product,
                                            ProductionStatusName = x.ProductionStatusName
                                        }).ToList()
                                    })
                                    .FirstOrDefault();

                if (orderDetails != null)
                {
                    DataContext = orderDetails;
                }
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}