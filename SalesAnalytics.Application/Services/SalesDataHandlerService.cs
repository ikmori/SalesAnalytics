using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SalesAnalytics.Application.Interfaces;
using SalesAnalytics.Application.Repositories;
using SalesAnalytics.Application.Result;
using SalesAnalytics.Domain.Entities.Csv;
using SalesAnalytics.Domain.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesAnalytics.Application.Services
{
    internal class SalesDataHandlerService: ISalesDataHandlerService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SalesDataHandlerService> _logger;
        private readonly IDwhRepository _dwhRepo;

        // Fuentes de API
        private readonly ICustomerApiRepository _customerApiRepo;
        private readonly IProductApiRepository _productApiRepo;

        // Fuente de Base de Datos
        private readonly ISaleRepository _saleDbRepo;

        // Fuentes de CSV
        private readonly IFileReaderRepository<Products> _productCsvReader;
        private readonly IFileReaderRepository<Customers> _customerCsvReader;
        private readonly IFileReaderRepository<Orders> _saleCsvReader;

        public SalesDataHandlerService(ILogger<SalesDataHandlerService> logger,
                                       IConfiguration configuration,
                                       IDwhRepository dwhRepo,
                                       ICustomerApiRepository customerApiRepo,
                                       IProductApiRepository productApiRepo,
                                       ISaleRepository saleDbRepo,
                                       IFileReaderRepository<Products> productCsvReader,
                                       IFileReaderRepository<Customers> customerCsvReader,
                                       IFileReaderRepository<Orders> saleCsvReader)
        {
            _logger = logger;
            _dwhRepo = dwhRepo;
            _customerApiRepo = customerApiRepo;
            _productApiRepo = productApiRepo;
            _saleDbRepo = saleDbRepo;
            _productCsvReader = productCsvReader;
            _customerCsvReader = customerCsvReader;
            _saleCsvReader = saleCsvReader;
        }

        //string basePath = @"C:\Users\ellia\Desktop\Bigdata\Archivo CSV Análisis de Ventas-20250923";
        //movi eso para el appsettings.json del worker y anadi el configutation para que no este hardcodeado

        public async Task<ServiceResult> ProcessSalesDataAsync()
        {

            string basePath = _configuration["EtlSettings:CsvBasePath"];
            if (string.IsNullOrEmpty(basePath))
            {
                _logger.LogError("La ruta 'EtlSettings:CsvBasePath' no está configurada en appsettings.json.");
                
                throw new InvalidOperationException("Configuración de CsvBasePath no encontrada.");
            }

            //Extraccion de datos desde las diferentes fuentes
            _logger.LogInformation("Extrayendo datos de APIs...");
            var apiCustomers = await _customerApiRepo.GetCustomersAsync();
            var apiProducts = await _productApiRepo.GetProductsAsync();

            _logger.LogInformation("Extrayendo datos de Base de Datos...");
            var dbSales = await _saleDbRepo.GetSaleAsync();

            _logger.LogInformation("Extrayendo datos de CSVs...");
            var csvProducts = await _productCsvReader.ReadFileAsync(Path.Combine(basePath, "products.csv"));
            var csvCustomers = await _customerCsvReader.ReadFileAsync(Path.Combine(basePath, "customers.csv"));
            var csvSales = await _saleCsvReader.ReadFileAsync(Path.Combine(basePath, "orders.csv")); 


            throw new NotImplementedException();
        }
    }
}
