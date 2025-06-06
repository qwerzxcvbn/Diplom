using System.Linq;
using System.Windows;

namespace StuldastolClient.Views
{
    public partial class ProductDetailsWindow : Window
    {
        private Products Product { get; set; }

        public ProductDetailsWindow(Products product)
        {
            InitializeComponent();
            Product = product;
            DataContext = Product;

            // Разделяем описание на части
            SplitAndSetDescription();
        }

        private void SplitAndSetDescription()
        {
            if (string.IsNullOrWhiteSpace(Product.Description))
            {
                MaterialTextBlock.Text = "Материал: Не указано.";
                ColorTextBlock.Text = "Цвет дерева: Не указано.";
                SeatTextBlock.Text = "Сиденье: Не указано.";
                RestDescriptionTextBlock.Text = "Дополнительная информация отсутствует.";
                return;
            }

            // Разделяем описание на строки
            var lines = Product.Description.Split(new[] { '.' }, System.StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.Trim() + (line.EndsWith(".") ? "" : "."))
                .ToList();

            // Первые три строки: материал, цвет дерева, сиденье
            MaterialTextBlock.Text = lines.Count > 0 ? lines[0] : "Материал: Не указано.";
            ColorTextBlock.Text = lines.Count > 1 ? lines[1] : "Цвет дерева: Не указано.";
            SeatTextBlock.Text = lines.Count > 2 ? lines[2] : "Сиденье: Не указано.";

            // Остальное описание (начиная с "Стулья покрыты маслом Biofa...")
            if (lines.Count > 3)
            {
                var restDescription = string.Join(" ", lines.Skip(3));
                RestDescriptionTextBlock.Text = restDescription;
            }
            else
            {
                RestDescriptionTextBlock.Text = "";
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void AddToCartButton_Click(object sender, RoutedEventArgs e)
        {
            using (var context = new StuldastolEntities())
            {
                // Предполагаем, что текущий пользователь хранится в глобальной переменной CurrentUser
                int userId = CurrentUser.UserId;

                // Проверяем, есть ли корзина для текущего пользователя
                var cart = context.Carts.FirstOrDefault(c => c.UserId == userId);
                if (cart == null)
                {
                    // Если корзины нет, создаем новую
                    cart = new Carts
                    {
                        UserId = userId,
                        CreatedAt = System.DateTime.Now
                    };
                    context.Carts.Add(cart);
                    context.SaveChanges();
                }

                // Проверяем, есть ли уже этот товар в корзине
                var cartItem = context.CartItems.FirstOrDefault(ci => ci.CartId == cart.Id && ci.ProductId == Product.Id);
                if (cartItem != null)
                {
                    // Если товар уже в корзине, увеличиваем количество
                    cartItem.Quantity += 1;
                }
                else
                {
                    // Если товара нет в корзине, добавляем новый элемент
                    cartItem = new CartItems
                    {
                        CartId = cart.Id,
                        ProductId = Product.Id,
                        Quantity = 1,
                        CreatedAt = System.DateTime.Now
                    };
                    context.CartItems.Add(cartItem);
                }

                context.SaveChanges();
            }

            Close();
        }
    }
}