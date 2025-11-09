using Microsoft.EntityFrameworkCore;
using SalesAnalytics.Domain.Entities.Db;
namespace SalesAnalytics.Persistence.Repositories.Db.Context
{
    public class SalesContext: DbContext
    {
        public SalesContext(DbContextOptions<SalesContext> options) : base(options)
        {

        }
        public DbSet<sale> Sales { get; set; }
    }
}
