using ClassLibrary2.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SalesAnalytics.Application.Repositories;
using SalesAnalytics.Domain.Entities.Dwh.Dimensions;
using SalesAnalytics.Persistence.Extensions;
using SalesAnalytics.Persistence.Repositories.Dwh.Context; 
using System.Data;

namespace SalesAnalytics.Persistence.Repositories.Dwh
{
    public class DwhRepository : IDwhRepository
    {
        private readonly SalesDwhContext _context; 
        private readonly string _connectionString;

        public DwhRepository(SalesDwhContext context, IConfiguration configuration)
        {
            _context = context;
            _connectionString = configuration.GetConnectionString("DwhConnection")
                                ?? throw new Exception("DwhConnection not found");
        }

        public async Task CleanTablesAsync()
        {
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM Fact.FactSales");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM Dimension.DimCustomer");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM Dimension.DimProduct");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM Dimension.DimStatus");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM Dimension.DimDate");

            await _context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('Dimension.DimCustomer', RESEED, 0)");
            await _context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('Dimension.DimProduct', RESEED, 0)");
            await _context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('Dimension.DimStatus', RESEED, 0)");
        }

        private async Task BulkInsertAsync<T>(IEnumerable<T> data, string tableName)
        {
            var list = data.ToList();
            if (!list.Any()) return;
            var dataTable = list.ToDataTable();
            using (var bulkCopy = new SqlBulkCopy(_connectionString))
            {
                bulkCopy.DestinationTableName = tableName;
                bulkCopy.BatchSize = 50000;
                bulkCopy.BulkCopyTimeout = 600;
                foreach (DataColumn column in dataTable.Columns)
                    bulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
                await bulkCopy.WriteToServerAsync(dataTable);
            }
        }

        public async Task LoadCustomersBulkAsync(IEnumerable<DimCustomer> c) => await BulkInsertAsync(c, "Dimension.DimCustomer");
        public async Task LoadProductsBulkAsync(IEnumerable<DimProduct> p) => await BulkInsertAsync(p, "Dimension.DimProduct");
        public async Task LoadStatusBulkAsync(IEnumerable<DimStatus> s) => await BulkInsertAsync(s, "Dimension.DimStatus");
        public async Task LoadDatesBulkAsync(IEnumerable<DimDate> d) => await BulkInsertAsync(d, "Dimension.DimDate");
        public async Task LoadFactsBulkAsync(IEnumerable<FactSale> f) => await BulkInsertAsync(f, "Fact.FactSales");

        // Lecturas usando el contexto correcto
        public async Task<List<DimCustomer>> GetCustomersAsync() => await _context.DimCustomers.AsNoTracking().ToListAsync();
        public async Task<List<DimProduct>> GetProductsAsync() => await _context.DimProducts.AsNoTracking().ToListAsync();
        public async Task<List<DimStatus>> GetStatusesAsync() => await _context.DimStatuses.AsNoTracking().ToListAsync();
    }
}