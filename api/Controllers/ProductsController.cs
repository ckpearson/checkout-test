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
        private readonly IDataStore _store;

        public ProductsController(IDataStore store)
        {
            _store = store;
        }
        public async Task<IEnumerable<Product>> GetAll()
        {
            return await _store.GetAll<Product>();
        }

        [Route("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            return (await _store.GetById<Product>(id)).Match<Product, IActionResult>(Ok, NotFound);
        }
    }
}