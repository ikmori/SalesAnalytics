using Microsoft.EntityFrameworkCore;
using System.Data;
using SalesAnalytics.Api.Data.Entities;
namespace SalesAnalytics.Api.Data.Context
{
    public class ProductContext: DbContext
    {
        public ProductContext(DbContextOptions<ProductContext> options) : base(options)
        {
        }
        public DbSet<Product> Products { get; set; }
    }
}
