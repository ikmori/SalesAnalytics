using Microsoft.AspNetCore.Mvc;
using SalesAnalytics.Application.Repositories;

namespace SalesAnalytics.Web.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductApiRepository _productApiRepository;
        public ProductController(IProductApiRepository productApiRepository)
        {
            _productApiRepository = productApiRepository;
        }
        public async Task<IActionResult> Index()
        {
            var products =  await _productApiRepository.GetProductsAsync();
            return View(products);
        }
    }
}
