using System.Collections.Generic;
using System.Threading.Tasks;
using api.Models;
using api.Utils;

namespace api.Services
{
    public interface IProductService
    {
        Task<IEnumerable<Product>> GetAll();
        Task<Option<Product>> GetById(int productId);
    }
}