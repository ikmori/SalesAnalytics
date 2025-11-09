using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SalesAnalytics.Api.Data.Interface;

namespace SalesAnalytics.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductApiController : ControllerBase
    {
        private readonly IProductRepository productRepository;
        public ProductApiController(IProductRepository productRepository)
        {
            this.productRepository = productRepository;
        }
        [HttpGet("GetProducts")]
        public async Task<IActionResult> GetProducts()
        {
            var products = await this.productRepository.GetProductsAsync();
            return Ok(products);
        }
    }
}
