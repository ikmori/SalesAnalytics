using SalesAnalytics.Api.Data.Entities;

namespace SalesAnalytics.Api.Data.Interface
{
    public interface IProductRepository
    {
        public Task<IEnumerable<Product>> GetProductsAsync();
    }
}
