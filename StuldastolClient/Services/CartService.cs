using StuldastolClient.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using StuldastolClient.Views;

namespace StuldastolClient.Services
{
    public class CartService : ICartService
    {
        public CartService()
        {
        }

        public ObservableCollection<CartItemViewModel> GetCartItems(int userId)
        {
            using (var context = new StuldastolEntities())
            {
                var cart = context.Carts.FirstOrDefault(c => c.UserId == userId);
                if (cart != null)
                {
                    var cartItemsData = context.CartItems
                        .Where(ci => ci.CartId == cart.Id)
                        .Select(ci => new CartItemViewModel
                        {
                            CartItemId = ci.Id,
                            Product = ci.Products,
                            Quantity = ci.Quantity,
                            TotalPrice = ci.Quantity * ci.Products.Price
                        }).ToList();

                    return new ObservableCollection<CartItemViewModel>(cartItemsData);
                }
                return new ObservableCollection<CartItemViewModel>();
            }
        }

        public void AddToCart(int productId, int quantity)
        {
            int userId = CurrentUser.UserId;
            if (userId == 0)
            {
                return;
            }

            using (var context = new StuldastolEntities())
            {
                var cart = context.Carts.FirstOrDefault(c => c.UserId == userId);
                if (cart == null)
                {
                    cart = new Carts
                    {
                        UserId = userId,
                        CreatedAt = DateTime.Now
                    };
                    context.Carts.Add(cart);
                    context.SaveChanges();
                }

                var existingCartItem = context.CartItems.FirstOrDefault(ci => ci.CartId == cart.Id && ci.ProductId == productId);
                if (existingCartItem != null)
                {
                    existingCartItem.Quantity += quantity;
                    existingCartItem.CreatedAt = DateTime.Now;
                }
                else
                {
                    var newCartItem = new CartItems
                    {
                        CartId = cart.Id,
                        ProductId = productId,
                        Quantity = quantity,
                        CreatedAt = DateTime.Now
                    };
                    context.CartItems.Add(newCartItem);
                }

                context.SaveChanges();
            }
        }

        public void RemoveFromCart(int cartItemId)
        {
            using (var context = new StuldastolEntities())
            {
                var itemToDelete = context.CartItems.FirstOrDefault(ci => ci.Id == cartItemId);
                if (itemToDelete != null)
                {
                    context.CartItems.Remove(itemToDelete);
                    context.SaveChanges();
                }
            }
        }
    }
}