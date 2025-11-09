using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesAnalytics.Application.Repositories
{
    using SalesAnalytics.Domain.Entities.Api;
    public interface IProductApiRepository
    {
        Task<IEnumerable<Product>> GetProductsAsync();
    }
}
