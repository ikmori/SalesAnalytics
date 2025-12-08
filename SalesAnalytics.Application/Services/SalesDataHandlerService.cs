using ClassLibrary2.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SalesAnalytics.Application.Interfaces;
using SalesAnalytics.Application.Repositories;
using SalesAnalytics.Application.Result;
using SalesAnalytics.Domain.Entities.Csv;
using SalesAnalytics.Domain.Entities.Dwh.Dimensions;
using SalesAnalytics.Domain.Repository;

namespace SalesAnalytics.Application.Services
{
    public class SalesDataHandlerService : ISalesDataHandlerService
    {
        private readonly ILogger<SalesDataHandlerService> _logger;
        private readonly IConfiguration _configuration;

        //Repositorios CSV
        private readonly IFileReaderRepository<Customers> _customerCsvReader;
        private readonly IFileReaderRepository<Products> _productCsvReader;
        private readonly IFileReaderRepository<Orders> _orderCsvReader;
        private readonly IFileReaderRepository<OrderDetails> _orderDetailCsvReader;

        //Repositorios DB
        private readonly ISaleRepository _dbReader;

        //Repositorios API 
        private readonly ICustomerApiRepository _customerApiRepository;
        private readonly IProductApiRepository _productApiRepository;

        //Repositorio DWH
        private readonly IDwhRepository _dwhRepository;

        public SalesDataHandlerService(
            ILogger<SalesDataHandlerService> logger,
            IConfiguration configuration,
            IFileReaderRepository<Customers> customerCsvReader,
            IFileReaderRepository<Products> productCsvReader,
            IFileReaderRepository<Orders> orderCsvReader,
            IFileReaderRepository<OrderDetails> orderDetailCsvReader,
            ISaleRepository dbReader,
            ICustomerApiRepository customerApiRepository,
            IProductApiRepository productApiRepository,   
            IDwhRepository dwhRepository)
        {
            _logger = logger;
            _configuration = configuration;
            _customerCsvReader = customerCsvReader;
            _productCsvReader = productCsvReader;
            _orderCsvReader = orderCsvReader;
            _orderDetailCsvReader = orderDetailCsvReader;
            _dbReader = dbReader;
            _customerApiRepository = customerApiRepository;
            _productApiRepository = productApiRepository;
            _dwhRepository = dwhRepository;
        }

        public async Task<ServiceResult> ProcessSalesDataAsync()
        {
            try
            {
                _logger.LogInformation(">>> 1. INICIANDO EXTRACCIÓN MULTI-FUENTE (E)");

                string csvBasePath = _configuration["CsvSettings:BasePath"]
                                     ?? throw new Exception("CsvSettings:BasePath no configurado");

                //Tareas de CSV
                var tCsvCust = _customerCsvReader.ReadFileAsync(Path.Combine(csvBasePath, "customers.csv"));
                var tCsvProd = _productCsvReader.ReadFileAsync(Path.Combine(csvBasePath, "products.csv"));
                var tCsvOrd = _orderCsvReader.ReadFileAsync(Path.Combine(csvBasePath, "orders.csv"));
                var tCsvDet = _orderDetailCsvReader.ReadFileAsync(Path.Combine(csvBasePath, "order_details.csv"));

                //Tareas de DB (Activado) 
                var tDbSales = _dbReader.GetSaleAsync();

                //Tareas de API
                var tApiCust = _customerApiRepository.GetCustomersAsync();
                var tApiProd = _productApiRepository.GetProductsAsync();

                // Espero a que todas las fuentes respondan
                await Task.WhenAll(tCsvCust, tCsvProd, tCsvOrd, tCsvDet, tDbSales, tApiCust, tApiProd);

                //Obtener Resultados
                var csvCustomers = tCsvCust.Result;
                var csvProducts = tCsvProd.Result;
                var csvOrders = tCsvOrd.Result;
                var csvDetails = tCsvDet.Result;

                var dbSales = tDbSales.Result;

                var apiCustomers = tApiCust.Result; 
                var apiProducts = tApiProd.Result;  

                _logger.LogInformation($"Extracción completada:");
                _logger.LogInformation($" - CSV: {csvCustomers.Count()} clientes, {csvOrders.Count()} órdenes.");
                _logger.LogInformation($" - DB:  {dbSales.Count()} ventas históricas.");
                _logger.LogInformation($" - API: {apiCustomers.Count()} clientes, {apiProducts.Count()} productos.");


                _logger.LogInformation(">>> 2. TRANSFORMACIÓN Y CARGA (T & L)");

                // limpiesa de tablas antes de cargar 
                await _dwhRepository.CleanTablesAsync();

                //DIM FECHAS
                var dimDates = GenerateDateDimension(2023, 2026);
                await _dwhRepository.LoadDatesBulkAsync(dimDates);

                //DIM CUSTOMER (Merge: CSV + API + DB)
        
                var allCustomers = new List<DimCustomer>();

                // Mapeo CSV
                allCustomers.AddRange(csvCustomers.Select(c => new DimCustomer
                {
                    CustomerID_NK = c.CustomerID,
                    CustomerName = $"{c.FirstName} {c.LastName}",
                    Email = c.Email,
                    City = c.City,
                    Country = c.Country
                }));

                // Mapeo API
                allCustomers.AddRange(apiCustomers.Select(c => new DimCustomer
                {
                    CustomerID_NK = c.IdCliente, 
                    CustomerName = c.FirstName, 
                    Email = c.Email,
                    City = c.City,
                    Country = c.Country 
                }));

                // Mapeo DB
                allCustomers.AddRange(dbSales.Select(v => new DimCustomer
                {
                    CustomerID_NK = v.IdCliente,
                    CustomerName = "Cliente Historico",
                    Email = "N/A"
                }));

                // De-duplicación por ID
                var dimCustomers = allCustomers
                    .GroupBy(c => c.CustomerID_NK)
                    .Select(g => g.First())
                    .ToList();

                await _dwhRepository.LoadCustomersBulkAsync(dimCustomers);


                //DIM PRODUCT (Merge: CSV + API)
                var allProducts = new List<DimProduct>();

                // CSV
                allProducts.AddRange(csvProducts.Select(p => new DimProduct
                {
                    ProductID_NK = p.ProductID,
                    ProductName = p.ProductName,
                    Category = p.Category,
                    Price = p.Price
                }));

                // API
                allProducts.AddRange(apiProducts.Select(p => new DimProduct
                {
                    ProductID_NK = p.IdProducto,      
                    ProductName = p.NombreProducto,    
                    Category = p.Categoria,
                    Price = p.Precio
                }));

                // DB
                if (dbSales != null)
                {
                    allProducts.AddRange(dbSales
                        .Where(v => v.orderDetails != null) 
                        .SelectMany(v => v.orderDetails)
                        .Select(d => new DimProduct
                        {
                            ProductID_NK = d.IdProducto,
                            ProductName = $"Producto Histórico {d.IdProducto}",
                            Category = "Sin Categoría (DB)",
                            Price = 0
                        }));
                }

                var dimProducts = allProducts
                    .GroupBy(p => p.ProductID_NK)
                    .Select(g => g.First())
                    .ToList();

                await _dwhRepository.LoadProductsBulkAsync(dimProducts);


                //DIM STATUS
                var statusList = csvOrders.Select(o => o.Status)
                    .Concat(dbSales.Select(v => v.Status))
                    .Distinct()
                    .Select(s => new DimStatus { StatusName = s })
                    .ToList();

                await _dwhRepository.LoadStatusBulkAsync(statusList);


                //RECUPERAR LLAVES 
                _logger.LogInformation("Recuperando Surrogate Keys del DWH...");

                var dbCust = await _dwhRepository.GetCustomersAsync();
                var dbProd = await _dwhRepository.GetProductsAsync();
                var dbStat = await _dwhRepository.GetStatusesAsync();

                // Diccionarios
                var custDict = dbCust.ToDictionary(k => k.CustomerID_NK, v => v.CustomerKey);
                var prodDict = dbProd.ToDictionary(k => k.ProductID_NK, v => v.ProductKey);
                var statDict = dbStat.ToDictionary(k => k.StatusName, v => v.StatusKey);


                //FACT SALES (Merge: CSV + DB)
                _logger.LogInformation("Construyendo FactTable unificada...");

                var factSales = new List<FactSale>();

                //Procesar Ventas CSV (Orders + Details)
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

                
                if (dbSales != null)
                {
                    foreach (var venta in dbSales)
                    {
                        
                        if (venta.orderDetails != null)
                        {
                            
                            foreach (var detalle in venta.orderDetails)
                            {
                        
                                int cKey = custDict.TryGetValue(venta.IdCliente, out int ck) ? ck : -1;

                  
                                int pKey = prodDict.TryGetValue(detalle.IdProducto, out int pk) ? pk : -1;

                                int sKey = statDict.TryGetValue(venta.Status, out int sk) ? sk : -1;

                       
                                if (cKey != -1 && pKey != -1)
                                {
                                    factSales.Add(new FactSale
                                    {
                                        DateKey = int.Parse(venta.FechaVenta.ToString("yyyyMMdd")),
                                        CustomerKey = cKey,
                                        ProductKey = pKey,
                                        StatusKey = sKey,
                                        OrderID_NK = venta.IdVenta,

                                    
                                        Quantity = detalle.Cantidad,
                                        TotalPrice = detalle.TotalLinea
                                    });
                                }
                            }
                        }
                    }
                }

                await _dwhRepository.LoadFactsBulkAsync(factSales);

                return new ServiceResult { IsSuccess = true, Message = $"ETL Finalizado. Total Hechos: {factSales.Count}" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico en ETL");
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