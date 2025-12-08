using ClassLibrary2.Models;
using Microsoft.Extensions.Configuration; 
using Microsoft.Extensions.Logging;
using SalesAnalytics.Application.Interfaces;
using SalesAnalytics.Application.Repositories;
using SalesAnalytics.Application.Result;
using SalesAnalytics.Domain.Entities.Csv;
// using SalesAnalytics.Domain.Entities.Db; 
using SalesAnalytics.Domain.Entities.Dwh.Dimensions;
using SalesAnalytics.Domain.Repository; 

namespace SalesAnalytics.Application.Services
{
    public class SalesDataHandlerService : ISalesDataHandlerService
    {
        private readonly ILogger<SalesDataHandlerService> _logger;
        private readonly IConfiguration _configuration;

        
        private readonly IFileReaderRepository<Customers> _customerCsvReader;
        private readonly IFileReaderRepository<Products> _productCsvReader;
        private readonly IFileReaderRepository<Orders> _orderCsvReader;
        private readonly IFileReaderRepository<OrderDetails> _orderDetailCsvReader;

        private readonly ISaleRepository _dbReader;
        private readonly IDwhRepository _dwhRepository;

        public SalesDataHandlerService(
            ILogger<SalesDataHandlerService> logger,
            IConfiguration configuration,
            IFileReaderRepository<Customers> customerCsvReader,
            IFileReaderRepository<Products> productCsvReader,
            IFileReaderRepository<Orders> orderCsvReader,
            IFileReaderRepository<OrderDetails> orderDetailCsvReader,
            ISaleRepository dbReader,
            IDwhRepository dwhRepository)
        {
            _logger = logger;
            _configuration = configuration;
            _customerCsvReader = customerCsvReader;
            _productCsvReader = productCsvReader;
            _orderCsvReader = orderCsvReader;
            _orderDetailCsvReader = orderDetailCsvReader;
            _dbReader = dbReader;
            _dwhRepository = dwhRepository;
        }

        public async Task<ServiceResult> ProcessSalesDataAsync()
        {
            try
            {
                _logger.LogInformation(">>> 1. Extracción (E)");


                string csvBasePath = _configuration["CsvSettings:BasePath"]
                                     ?? throw new Exception("CsvSettings:BasePath no configurado");

                var taskCustomers = _customerCsvReader.ReadFileAsync(Path.Combine(csvBasePath, "customers.csv"));
                var taskProducts = _productCsvReader.ReadFileAsync(Path.Combine(csvBasePath, "products.csv"));
                var taskOrders = _orderCsvReader.ReadFileAsync(Path.Combine(csvBasePath, "orders.csv"));
                var taskDetails = _orderDetailCsvReader.ReadFileAsync(Path.Combine(csvBasePath, "order_details.csv"));

                
                // var taskDbSales = _dbReader.GetSalesAsync();

                //esperamos a que todo termine
                await Task.WhenAll(taskCustomers, taskProducts, taskOrders, taskDetails /*, taskDbSales */);

                var csvCustomers = taskCustomers.Result;
                var csvProducts = taskProducts.Result;
                var csvOrders = taskOrders.Result;
                var csvDetails = taskDetails.Result;
                //var dbSales = taskDbSales.Result;

                _logger.LogInformation($"Leídos: {csvCustomers.Count()} clientes, {csvProducts.Count()} productos, {csvOrders.Count()} ordenes.");

                _logger.LogInformation(">>> 2. Transformación y Carga (T & L)");

                await _dwhRepository.CleanTablesAsync();

                var dimDates = GenerateDateDimension(2023, 2026);
                await _dwhRepository.LoadDatesBulkAsync(dimDates);

               
                var dimCustomers = csvCustomers
                    .Select(c => new DimCustomer
                    {
                        CustomerID_NK = c.CustomerID,
                        CustomerName = $"{c.FirstName} {c.LastName}",
                        Email = c.Email,
                        City = c.City,
                        Country = c.Country
                    })
                    // .Concat(...) // Aqui conxion DB si los tuvieras
                    .GroupBy(c => c.CustomerID_NK)
                    .Select(g => g.First())
                    .ToList();

                await _dwhRepository.LoadCustomersBulkAsync(dimCustomers);

                
                var dimProducts = csvProducts
                    .Select(p => new DimProduct
                    {
                        ProductID_NK = p.ProductID,
                        ProductName = p.ProductName,
                        Category = p.Category,
                        Price = p.Price
                    })
                    .GroupBy(p => p.ProductID_NK)
                    .Select(g => g.First())
                    .ToList();

                await _dwhRepository.LoadProductsBulkAsync(dimProducts);

               
                var dimStatuses = csvOrders
                    .Select(o => o.Status)
                    .Distinct()
                    .Select(s => new DimStatus { StatusName = s })
                    .ToList();

                await _dwhRepository.LoadStatusBulkAsync(dimStatuses);

                
                _logger.LogInformation("Recuperando Surrogate Keys...");

                var dbCust = await _dwhRepository.GetCustomersAsync();
                var dbProd = await _dwhRepository.GetProductsAsync();
                var dbStat = await _dwhRepository.GetStatusesAsync();

                var custDict = dbCust.ToDictionary(k => k.CustomerID_NK, v => v.CustomerKey);
                var prodDict = dbProd.ToDictionary(k => k.ProductID_NK, v => v.ProductKey);
                var statDict = dbStat.ToDictionary(k => k.StatusName, v => v.StatusKey);

                // factsales
                _logger.LogInformation("Construyendo FactTable...");

                var factSales = new List<FactSale>();

                
                var ordersMap = csvOrders.ToDictionary(o => o.OrderID, o => o);

                foreach (var detail in csvDetails)
                {
                    if (ordersMap.TryGetValue(detail.OrderID, out var order))
                    {
                     
                        int cKey = custDict.TryGetValue(order.CustomerID, out int ck) ? ck : -1;
                        int pKey = prodDict.TryGetValue(detail.ProductID, out int pk) ? pk : -1;
                        int sKey = statDict.TryGetValue(order.Status, out int sk) ? sk : -1;

                        if (cKey != -1 && pKey != -1)
                        {
                            factSales.Add(new FactSale
                            {
                                DateKey = int.Parse(order.OrderDate.ToString("yyyyMMdd")),
                                CustomerKey = cKey,
                                ProductKey = pKey,
                                StatusKey = sKey,
                                OrderID_NK = order.OrderID,
                                Quantity = detail.Quantity,
                                TotalPrice = detail.TotalPrice
                            });
                        }
                    }
                }

                await _dwhRepository.LoadFactsBulkAsync(factSales);

                return new ServiceResult { IsSuccess = true, Message = $"Procesados {factSales.Count} registros." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en ETL");
                return new ServiceResult { IsSuccess = false, Message = ex.Message };
            }
        }

        private List<DimDate> GenerateDateDimension(int startYear, int endYear)
        {
            var dates = new List<DimDate>();
            var startDate = new DateTime(startYear, 1, 1);
            var endDate = new DateTime(endYear, 12, 31);

            for (var dt = startDate; dt <= endDate; dt = dt.AddDays(1))
            {
                dates.Add(new DimDate
                {
                    DateKey = int.Parse(dt.ToString("yyyyMMdd")),

                    FullDate = DateOnly.FromDateTime(dt),

                    Day = dt.Day,
                    Month = dt.Month,
                    MonthName = dt.ToString("MMMM"),
                    Year = dt.Year,
                    Quarter = (dt.Month - 1) / 3 + 1,
                    DayOfWeek = dt.DayOfWeek.ToString(),
                    DayOfYear = dt.DayOfYear
                });
            }
            return dates;
        }
    }
}