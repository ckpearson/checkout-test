using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using api.Models;
using api.Services;
using api.Utils;

namespace api.Controllers
{
    [Route("api/products")]
    public class ProductsController : Controller
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }
        public async Task<IEnumerable<Product>> GetAll()
        {
            return await _productService.GetAll();
        }

        [Route("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            return (await _productService.GetById(id)).Match<Product, IActionResult>(Ok, NotFound);
        }
    }
}