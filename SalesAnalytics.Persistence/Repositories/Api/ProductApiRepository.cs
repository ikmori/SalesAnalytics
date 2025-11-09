using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SalesAnalytics.Application.Repositories;
using SalesAnalytics.Domain.Entities.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace SalesAnalytics.Persistence.Repositories.Api
{
    public class ProductApiRepository:IProductApiRepository
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly ILogger<ProductApiRepository> _logger;
        private readonly IConfiguration _configuration;
        private string baseUrl = string.Empty;

        public ProductApiRepository(IHttpClientFactory clientFactory, ILogger<ProductApiRepository> logger, IConfiguration configuration)
        {
            _clientFactory = clientFactory;
            _logger = logger;
            _configuration = configuration;
            baseUrl = _configuration["ApiConfig:BaseUrl"] ?? string.Empty;
        }
        public async Task<IEnumerable<Product>> GetProductsAsync()
        {
            List<Product> products = new List<Product>();
            try
            {
                using var client = _clientFactory.CreateClient("ProductApiClient");
                client.BaseAddress = new Uri(baseUrl);
                using var response = await client.GetAsync("/api/ProductApi/GetProducts");
                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = await response.Content.ReadFromJsonAsync<IEnumerable<Product>>();
                    if (apiResponse != null)
                    {
                        products = apiResponse.ToList();
                    }
                }
                else
                {
                    _logger.LogError("Failed to fetch products from API. Status Code: {StatusCode}", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                products = null!;
                _logger.LogError(ex, "Error fetching products from API");
            }
            return products!;
        }
    }
}
