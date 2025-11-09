using Microsoft.EntityFrameworkCore;
using SalesAnalytics.Api.Data.Context;
using SalesAnalytics.Api.Data.Entities;
using SalesAnalytics.Api.Data.Interface;

namespace SalesAnalytics.Api.Data.Repository
{
    public class CustomerRepository : ICustomerRepository
    {
        public readonly CustomerContext context;
        public CustomerRepository(CustomerContext context)
        {
            this.context = context;
        }
        public async Task<IEnumerable<Customer>> GetCustomersAsync()
        {
            return await this.context.Customers.ToArrayAsync();
        }
    }
}
