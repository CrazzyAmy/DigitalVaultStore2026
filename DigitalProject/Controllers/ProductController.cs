using DigitalProject.Interface.Prouduct;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DigitalProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] Guid? categoryId)
        {
            var products = await _productService.GetAllAsync(categoryId);
            return Ok(products);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null)
                return NotFound();

            return Ok(product);
        }


    }
}
