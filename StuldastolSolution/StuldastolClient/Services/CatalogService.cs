using System.Collections.ObjectModel;
using System.Linq;

namespace StuldastolClient.Services
{
    public class CatalogService : ICatalogService
    {
        public CatalogService()
        {
        }

        public ObservableCollection<Products> GetProducts(int? categoryId = null, decimal? priceFrom = null, decimal? priceTo = null, string woodType = null, string oilColor = null)
        {
            using (var context = new StuldastolEntities())
            {
                var query = context.Products.AsQueryable();

                if (categoryId.HasValue)
                {
                    query = query.Where(p => p.CategoryId == categoryId);
                }
                if (priceFrom.HasValue)
                {
                    query = query.Where(p => p.Price >= priceFrom.Value);
                }
                if (priceTo.HasValue)
                {
                    query = query.Where(p => p.Price <= priceTo.Value);
                }
                if (!string.IsNullOrEmpty(woodType))
                {
                    query = query.Where(p => p.WoodType == woodType);
                }
                if (!string.IsNullOrEmpty(oilColor))
                {
                    query = query.Where(p => p.OilColor == oilColor);
                }

                return new ObservableCollection<Products>(query.ToList());
            }
        }

        public ObservableCollection<Categories> GetCategories()
        {
            using (var context = new StuldastolEntities())
            {
                return new ObservableCollection<Categories>(context.Categories.ToList());
            }
        }

        public Products GetProductDetails(int productId)
        {
            using (var context = new StuldastolEntities())
            {
                return context.Products.FirstOrDefault(p => p.Id == productId);
            }
        }
    }
}