using System;
using System.Linq;
using System.Collections.ObjectModel;
using StuldastolClient.Views;
using StuldastolClient.Services;

namespace StuldastolClient.Services
{
    public class OrderService : IOrderService
    {
        public void CreateOrder(ObservableCollection<CartItemViewModel> cartItems, string deliveryAddress, bool isSelfPickup)
        {
            int userId = CurrentUser.UserId;
            if (userId == 0 || cartItems == null || !cartItems.Any())
            {
                return;
            }

            using (var context = new StuldastolEntities())
            {
                var cart = context.Carts.FirstOrDefault(c => c.UserId == userId);
                if (cart != null)
                {
                    var paymentWindow = new PaymentWindow(cartItems, deliveryAddress, isSelfPickup);
                    if (paymentWindow.ShowDialog() == true)
                    {
                        var order = new Orders
                        {
                            UserId = userId,
                            TotalAmount = paymentWindow.FinalTotal,
                            DeliveryAddress = deliveryAddress,
                            IsSelfPickup = isSelfPickup,
                            PromoCodeId = paymentWindow.PromoCodeId,
                            StatusId = 1, // "В обработке"
                            CreatedAt = DateTime.Now
                        };
                        context.Orders.Add(order);
                        context.SaveChanges(); // Сохраняем заказ, чтобы получить order.Id

                        foreach (var cartItem in cartItems)
                        {
                            var productionOrder = new ProductionOrders
                            {
                                OrderId = order.Id,
                                ProductId = cartItem.Product.Id,
                                Quantity = cartItem.Quantity,
                                WoodType = cartItem.Product.WoodType,
                                OilColor = cartItem.Product.OilColor,
                                FabricType = cartItem.Product.FabricType,
                                Deadline = DateTime.Now.AddDays(14),
                                DeliveryAddress = order.DeliveryAddress,
                                IsSelfPickup = order.IsSelfPickup,
                                CurrentStatusId = 1, // "Принял"
                                AssignedTo = userId,
                                CreatedAt = DateTime.Now
                            };
                            context.ProductionOrders.Add(productionOrder);
                        }

                        var cartItemsToDelete = context.CartItems.Where(ci => ci.CartId == cart.Id).ToList();
                        context.CartItems.RemoveRange(cartItemsToDelete);

                        context.SaveChanges();
                    }
                }
            }
        }

        public ObservableCollection<OrderViewModel> GetOrders(int userId)
        {
            using (var context = new StuldastolEntities())
            {
                var userOrders = context.Orders
                    .Where(o => o.UserId == userId)
                    .Select(o => new OrderViewModel
                    {
                        OrderId = o.Id,
                        CreatedAt = o.CreatedAt,
                        TotalAmount = o.TotalAmount,
                        StatusName = o.OrderStatuses.Name,
                        ProductionOrders = o.ProductionOrders.Select(po => new ProductionOrderViewModel
                        {
                            Product = po.Products,
                            Quantity = po.Quantity,
                            CurrentStatusName = po.ProductionStatuses.Name
                        }).ToList()
                    }).ToList();

                return new ObservableCollection<OrderViewModel>(userOrders);
            }
        }
    }
}