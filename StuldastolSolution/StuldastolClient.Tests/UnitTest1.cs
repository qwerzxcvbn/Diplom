using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using StuldastolClient;
using StuldastolClient.Services;
using StuldastolClient.Views;
using System.Collections.ObjectModel;

namespace StuldastolClient.Tests
{
    [TestClass]
    public class UnitTests
    {
        [TestInitialize]
        public void Setup()
        {
            // Очищаем текущего пользователя перед каждым тестом
            CurrentUser.ClearUser();
        }

        [TestMethod]
        public void AuthenticateUser_ValidCredentials_ReturnsTrue()
        {
            var userServiceMock = new Mock<IUserService>();
            userServiceMock.Setup(us => us.AuthenticateUser("testuser", "testpass")).Returns(true);

            bool result = userServiceMock.Object.AuthenticateUser("testuser", "testpass");

            Assert.IsTrue(result, "Авторизация должна пройти успешно для корректных данных.");
        }

        [TestMethod]
        public void AddToCart_ValidProduct_CallsService()
        {
            var cartServiceMock = new Mock<ICartService>();
            CurrentUser.SetUser(1, "testuser", "test@example.com"); // Устанавливаем пользователя
            int productId = 1;
            int quantity = 1;

            cartServiceMock.Object.AddToCart(productId, quantity);

            cartServiceMock.Verify(cs => cs.AddToCart(productId, quantity), Times.Once(), "Метод AddToCart должен быть вызван один раз.");
        }

        [TestMethod]
        public void CreateOrder_NonEmptyCart_CallsServiceAndClearsCart()
        {
            var orderServiceMock = new Mock<IOrderService>();
            var cartServiceMock = new Mock<ICartService>();
            var product = new Products { Id = 1, Price = 1000m }; // Создаём тестовый продукт
            var cartItems = new ObservableCollection<CartItemViewModel>
            {
                new CartItemViewModel { CartItemId = 1, Product = product, Quantity = 1, TotalPrice = 1000m }
            };
            string deliveryAddress = "Адрес тест";
            bool isSelfPickup = false;

            // Настраиваем мок OrderService для имитации успешного создания заказа
            orderServiceMock.Setup(os => os.CreateOrder(It.IsAny<ObservableCollection<CartItemViewModel>>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Callback((ObservableCollection<CartItemViewModel> items, string addr, bool pickup) =>
                {
                    // Имитация очистки корзины после создания заказа
                    items.Clear();
                });

            orderServiceMock.Object.CreateOrder(cartItems, deliveryAddress, isSelfPickup);

            orderServiceMock.Verify(os => os.CreateOrder(cartItems, deliveryAddress, isSelfPickup), Times.Once(), "Метод CreateOrder должен быть вызван один раз.");
            Assert.AreEqual(0, cartItems.Count, "Корзина должна быть очищена после оформления заказа.");
        }
    }
}