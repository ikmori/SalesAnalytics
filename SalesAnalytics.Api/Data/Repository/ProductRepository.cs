using Microsoft.EntityFrameworkCore;
using SalesAnalytics.Api.Data.Context;
using SalesAnalytics.Api.Data.Entities;
using SalesAnalytics.Api.Data.Interface;

namespace SalesAnalytics.Api.Data.Repository
{
    public class ProductRepository: IProductRepository
    {
        public readonly ProductContext context;
        public ProductRepository(ProductContext context)
        {
            this.context = context;
        }
        public async Task<IEnumerable<Product>> GetProductsAsync()
        {
            return await this.context.Products.ToArrayAsync();
        }
    }
}
