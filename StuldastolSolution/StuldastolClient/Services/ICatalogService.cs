using System.Collections.ObjectModel;

namespace StuldastolClient.Services
{
    public interface ICatalogService
    {
        ObservableCollection<Products> GetProducts(int? categoryId = null, decimal? priceFrom = null, decimal? priceTo = null, string woodType = null, string oilColor = null);
        ObservableCollection<Categories> GetCategories();
        Products GetProductDetails(int productId);
    }
}