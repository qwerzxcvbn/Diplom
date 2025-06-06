using System.ComponentModel;

namespace StuldastolClient.Views
{
    public class CartItemViewModel : INotifyPropertyChanged
    {


        private int _cartItemId;
        private Products _product;
        private int _quantity;
        private decimal _totalPrice;

        public int CartItemId
        {
            get => _cartItemId;
            set
            {
                _cartItemId = value;
                OnPropertyChanged(nameof(CartItemId));
            }
        }

        public Products Product
        {
            get => _product;
            set
            {
                _product = value;
                OnPropertyChanged(nameof(Product));
            }
        }

        public int Quantity
        {
            get => _quantity;
            set
            {
                _quantity = value;
                OnPropertyChanged(nameof(Quantity));
                TotalPrice = _quantity * _product.Price; // Обновляем общую цену при изменении количества
            }
        }

        public decimal TotalPrice
        {
            get => _totalPrice;
            set
            {
                _totalPrice = value;
                OnPropertyChanged(nameof(TotalPrice));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}