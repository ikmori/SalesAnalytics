using SalesAnalytics.Api.Data.Entities;

namespace SalesAnalytics.Api.Data.Interface
{
    public interface ICustomerRepository
    {
        public Task<IEnumerable<Customer>> GetCustomersAsync();
    }
}
