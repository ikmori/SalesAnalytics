using Microsoft.AspNetCore.Mvc;
using SalesAnalytics.Application.Repositories;

namespace SalesAnalytics.Web.Controllers
{
    public class CustomerController : Controller
    {
        private readonly ICustomerApiRepository _customerApiRepository;
        public CustomerController(ICustomerApiRepository customerApiRepository) 
        { 
           _customerApiRepository = customerApiRepository;
        }
        public async Task<IActionResult> Index()
        {
            var customers = await _customerApiRepository.GetCustomersAsync();
            return View(customers);
        }
    }
}
