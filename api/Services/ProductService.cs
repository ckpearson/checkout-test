using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Models;
using api.Utils;

namespace api.Services
{
    public class ProductService : IProductService
    {
        private readonly IDataStore _store;

        public ProductService(IDataStore store)
        {
            _store = store;
        }

        public Task<IEnumerable<Product>> GetAll()
        {
            return _store.GetAll<Product>();
        }

        public Task<Option<Product>> GetById(int id)
        {
            return _store.GetById<Product>(id);
        }
    }
}