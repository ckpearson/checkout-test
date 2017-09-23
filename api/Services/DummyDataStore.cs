using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Models;
using api.Utils;

namespace api.Services
{
    public class DummyDataStore : IDataStore
    {
        private readonly ConcurrentDictionary<Type, Dictionary<int, object>> _store =
            new ConcurrentDictionary<Type, Dictionary<int, object>>();

        public DummyDataStore()
        {
            var prodDict = new Dictionary<int, object>();
            prodDict.Add(1, new Product
            {
                Id = 1,
                Name = "Some product"
            });

            prodDict.Add(10, new Product
            {
                Id = 10,
                Name = "Some other product"
            });
            
            _store.TryAdd(typeof(Product), prodDict);
        }

        public Task<IEnumerable<T>> GetAll<T>() where T : IDataItem
            => Task.Run(() =>
                _store.GetValueOrNone(typeof(T)).Map(d => d.Values.Cast<T>()).ValueOrElse(Enumerable.Empty<T>()));

        public Task<Option<T>> GetById<T>(int id) where T : IDataItem
            => Task.Run(() => {
               return from innerDict in _store.GetValueOrNone(typeof(T))
                      from innerVal  in innerDict.GetValueOrNone(id)
                      select (T)innerVal; 
            });
    }
}