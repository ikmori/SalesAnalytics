using ClassLibrary2.Models;
using SalesAnalytics.Domain.Entities.Dwh.Dimensions;


namespace SalesAnalytics.Application.Repositories
{
    public interface IDwhRepository
    {
        Task CleanTablesAsync();

        //Metodos Bulk
        Task LoadCustomersBulkAsync(IEnumerable<DimCustomer> customers);
        Task LoadProductsBulkAsync(IEnumerable<DimProduct> products);
        Task LoadStatusBulkAsync(IEnumerable<DimStatus> statuses);
        Task LoadDatesBulkAsync(IEnumerable<DimDate> dates);
        Task LoadFactsBulkAsync(IEnumerable<FactSale> facts);

        //Metodos de Lectura 
        Task<List<DimCustomer>> GetCustomersAsync();
        Task<List<DimProduct>> GetProductsAsync();
        Task<List<DimStatus>> GetStatusesAsync();
    }
}