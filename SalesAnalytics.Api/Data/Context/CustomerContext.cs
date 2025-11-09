using Microsoft.EntityFrameworkCore;
using SalesAnalytics.Api.Data.Entities;
namespace SalesAnalytics.Api.Data.Context
{
    public class CustomerContext: DbContext
    {
        public CustomerContext(DbContextOptions<CustomerContext> options) : base(options)
        {
        }
        public DbSet<Customer> Customers { get; set; }

    }
}
