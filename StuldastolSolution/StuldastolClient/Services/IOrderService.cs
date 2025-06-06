using StuldastolClient.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StuldastolClient.Services
{
    public interface IOrderService
    {
        void CreateOrder(System.Collections.ObjectModel.ObservableCollection<CartItemViewModel> cartItems, string deliveryAddress, bool isSelfPickup);
        System.Collections.ObjectModel.ObservableCollection<OrderViewModel> GetOrders(int userId);
    }
}
