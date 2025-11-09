using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SalesAnalytics.Api.Data.Interface;

namespace SalesAnalytics.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerApiController : ControllerBase
    {
        private readonly ICustomerRepository customerRepository;
        public CustomerApiController(ICustomerRepository customerRepository)
        {
            this.customerRepository = customerRepository;
        }
        [HttpGet("GetCustomers")]
        public async Task<IActionResult> GetCustomers()
        {
            var customers = await this.customerRepository.GetCustomersAsync();
            return Ok(customers);
        }
    }
}
