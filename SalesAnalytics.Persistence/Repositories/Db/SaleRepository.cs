using SalesAnalytics.Application.Repositories;
using SalesAnalytics.Persistence.Repositories.Db.Context;
using SalesAnalytics.Domain.Entities.Db;
using Microsoft.EntityFrameworkCore;
namespace SalesAnalytics.Persistence.Repositories.Db
{
    public class SaleRepository : ISaleRepository
    {
        private readonly SalesContext context;
        public SaleRepository(SalesContext context)
        {
            this.context = context;
        }
        public async Task<IEnumerable<sale>> GetSaleAsync()
        {
            return await this.context.Sales.ToListAsync();
        }
    }
}
