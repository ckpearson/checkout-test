using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Models;
using api.Utils;

namespace api.Services
{
    public class OrderService : IOrderService
    {
        private readonly IDataStore _store;

        public OrderService(IDataStore store)
        {
            _store = store;
        }

        public Task<Result<Order, string>> AddProductToOrder(int orderId, int productId)
        {
            throw new System.NotImplementedException();
        }

        public Task<Result<IEnumerable<Order>, string>> GetCompletedOrdersForUser(int userId)
            =>
                from user in _store.GetById<User>(userId).ResOfOption(() => $"No user for ID: {userId}")
                from orders in
                    _store.GetAllWhere<Order>(o => o.UserId == user.Id && o.CompletionTimestamp.HasValue)
                    .ResOfOption(() => "No completed orders found for this user")
                select orders;

        public Task<Result<Order, string>> GetOrCreateActiveOrderForUser(int userId)
            =>
                from user in _store.GetById<User>(userId).ResOfOption(() => $"No user for ID: {userId}")
                from order in _store.SingleWhere<Order>(o => o.UserId == userId && !o.CompletionTimestamp.HasValue)
                    .Bind(o => o.Match<Order, Task<Option<Order>>>(
                        order => Task.FromResult(Option<Order>.Some(order)),
                        () => _store.Create<Order>().Map(order => {
                            order.CompletionTimestamp = Option<long>.None;
                            order.UserId = userId;
                            return Option<Order>.Some(order);
                        })
                    ))
                    .ResOfOption(() => "Failed to locate or construct order for user")
                select order;

        public Task<bool> OrderBelongsToUser(int userId)
        {
            throw new System.NotImplementedException();
        }

        public Task<Result<Order, string>> RemoveProductFromOrder(int orderId, int productId)
        {
            throw new System.NotImplementedException();
        }
    }
}