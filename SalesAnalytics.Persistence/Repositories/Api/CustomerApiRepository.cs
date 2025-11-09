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
    public class CustomerApiRepository : ICustomerApiRepository
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly ILogger<CustomerApiRepository> _logger;
        private readonly IConfiguration _configuration;
        private string baseUrl = string.Empty;

        public CustomerApiRepository(IHttpClientFactory clientFactory, ILogger<CustomerApiRepository> logger, IConfiguration configuration) 
        { 
            _clientFactory = clientFactory;
            _logger = logger;
            _configuration = configuration;
            baseUrl = _configuration["ApiConfig:BaseUrl"] ?? string.Empty;
        }
        public async Task<IEnumerable<Customer>> GetCustomersAsync()
        {
            List<Customer> customers = new List<Customer>();

            try
            {
                using var client = _clientFactory.CreateClient("CustomerApiClient");
                client.BaseAddress = new Uri(baseUrl);
                using var response = await client.GetAsync("/api/CustomerApi/GetCustomers");

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = await response.Content.ReadFromJsonAsync<IEnumerable<Customer>>();
                    if (apiResponse != null)
                    {
                        customers = apiResponse.ToList();
                    }
                }
                else
                {
                    _logger.LogError("Failed to fetch customers from API. Status Code: {StatusCode}", response.StatusCode);
                }

            }
            catch (Exception ex)
            {
                customers = null!;
                _logger.LogError(ex, "Error fetching customers from API");

            }
            return customers!;
        }
    }
}   
