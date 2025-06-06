using StuldastolClient.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StuldastolClient.Services
{
    public interface ICartService
    {
        System.Collections.ObjectModel.ObservableCollection<CartItemViewModel> GetCartItems(int userId);
        void AddToCart(int productId, int quantity);
        void RemoveFromCart(int cartItemId);
    }
}
