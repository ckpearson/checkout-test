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
        private readonly ConcurrentDictionary<Type, Dictionary<int, IDataItem>> _store =
            new ConcurrentDictionary<Type, Dictionary<int, IDataItem>>();

        public DummyDataStore()
        {
            var prodDict = new Dictionary<int, IDataItem>();
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

            var userDict = new Dictionary<int, IDataItem>();
            userDict.Add(1, new User {
                Id = 1,
                Name = "Clint Pearson"
            });

            _store.TryAdd(typeof(User), userDict);

            var ordersDict = new Dictionary<int, IDataItem>();
            ordersDict.Add(1, new Order {
                Id = 1,
                UserId = 1,
                OrderLines = new Dictionary<int, int> {
                    {1, 10}
                },
                CompletionTimestamp = Option<long>.Some(DateTime.UtcNow.Ticks)
            });
            _store.TryAdd(typeof(Order), ordersDict);
        }

        public Task<T> Create<T>() where T : IDataItem
        => Task.Run(() => {
            var item = Activator.CreateInstance<T>();
            item.Id = 1;
            _store.AddOrUpdate(typeof(T),
                new Dictionary<int, IDataItem> { {1, item} },
                (_, dict) => {
                    item.Id = dict.Count + 1;
                    dict.Add(item.Id, item);
                    return dict;
                });

            return item;
        });

        public Task<IEnumerable<T>> GetAll<T>() where T : IDataItem
            => Task.Run(() =>
                _store.GetValueOrNone(typeof(T)).Map(d => d.Values.Cast<T>()).ValueOrElse(Enumerable.Empty<T>()));

        public Task<Option<IEnumerable<T>>> GetAllWhere<T>(Func<T, bool> predicate) where T : IDataItem
            => Task.Run(() => {
                return _store
                    .GetValueOrNone(typeof(T))
                    .Map(dict => dict.Values.Cast<T>().Where(predicate).ToList())
                    .Bind(list => list.Count == 0 ? Option<IEnumerable<T>>.None : Option<IEnumerable<T>>.Some(list));
            });

        public Task<Option<T>> GetById<T>(int id) where T : IDataItem
            => Task.Run(() => {
               return from innerDict in _store.GetValueOrNone(typeof(T))
                      from innerVal  in innerDict.GetValueOrNone(id)
                      select (T)innerVal; 
            });

        public Task<Option<T>> SingleWhere<T>(Func<T, bool> predicate) where T : IDataItem
            => Task.Run(() => {
                return _store
                    .GetValueOrNone(typeof(T))
                    .Map(dict => dict.Values.Cast<T>())
                    .Bind(vals => {
                        var match = vals.Where(predicate).ToList();
                        return match.Count == 1
                            ? Option<T>.Some(match[0])
                            : Option<T>.None;
                    });
            });
    }
}