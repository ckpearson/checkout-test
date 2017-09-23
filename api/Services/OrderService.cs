using System;
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
        private readonly IUserService _userService;
        private readonly IProductService _productService;

        public OrderService(
            IDataStore store,
            IUserService userService,
            IProductService productService)
        {
            _store = store;
            _userService = userService;
            _productService = productService;
        }

        /*
            ASSUMPTIONS:
                1. That it is ok for further add ops to increase quantity
                2. That it is ok for an order to not already exist

            IMPLEMENTATION:
                Obviously were this to be backed by some proper data store, there would
                likely need to be some sort of commit-phase here.
         */
        public Task<Result<Order, string>> AddProductToActiveOrder(int userId, int productId)
            =>
                (from user in _store.GetById<User>(userId).ResOfOption(() => $"No user for ID: {userId}")
                 from product in _store.GetById<Product>(productId).ResOfOption(() => $"No product for ID: {productId}")
                 from activeOrder in GetOrCreateActiveOrderForUser(userId)
                 select activeOrder)
                .Map(res => res.Map(order =>
                {
                    if (order.OrderLines.ContainsKey(productId))
                    {
                        order.OrderLines[productId]++;
                    }
                    else
                    {
                        order.OrderLines.Add(productId, 1);
                    }
                    return order;
                }));

        /*
            ASSUMPTIONS:
                1. That further remove ops can decrease quantity
                2. That line should be cleared if quantity <= 0
                3. That it isn't an error if that product isn't present on the order

            IMPLEMENTATION:
                This would require a commit-phase were a proper store used.
         */
        public Task<Result<Order, string>> RemoveProductFromActiveOrder(int userId, int productId)
            =>
                (from user in _store.GetById<User>(userId).ResOfOption(() => $"No user for ID: {userId}")
                 from product in _store.GetById<Product>(productId).ResOfOption(() => $"No product for ID: {productId}")
                 from activeOrder in GetOrCreateActiveOrderForUser(userId)
                 select activeOrder)
                .Map(res => res.Map(order =>
                {
                    if (order.OrderLines.ContainsKey(productId))
                    {
                        var quantity = order.OrderLines[productId];
                        if (quantity - 1 <= 0)
                        {
                            order.OrderLines.Remove(productId);
                        }
                        else
                        {
                            order.OrderLines[productId]--;
                        }
                    }
                    return order;
                }));

        public Task<Result<Order, string>> SetProductQuanityOnActiveOrder(int userId, int productId, int quantity)
            =>
                (from normalisedQuantity in Task.FromResult((quantity < 0 ? Option<int>.None : Option<int>.Some(quantity))
                    .ResOfOption(() => $"Quantity must be positive; {quantity} is invalid"))
                 from user in _store.GetById<User>(userId).ResOfOption(() => $"No user for ID: {userId}")
                 from product in _store.GetById<Product>(productId).ResOfOption(() => $"No product for ID: {productId}")
                 from activeOrder in GetOrCreateActiveOrderForUser(userId)
                 select activeOrder)
                .Map(res => res.Map(order =>
                {
                    if (quantity == 0 && order.OrderLines.ContainsKey(productId))
                    {
                        order.OrderLines.Remove(productId);
                    }
                    else
                    {
                        order.OrderLines[productId] = quantity;
                    }
                    return order;
                }));

        public Task<Result<Order, string>> ClearProductsForActiveOrder(int userId)
            =>
            (from user in _store.GetById<User>(userId).ResOfOption(() => $"No user for ID: {userId}")
            from activeOrder in GetOrCreateActiveOrderForUser(userId)
            select activeOrder)
            .Map(res => res.Map(order => {
                order.OrderLines.Clear();
                return order;
            }));

        public Task<Result<IEnumerable<Order>, string>> GetCompletedOrdersForUser(int userId)
            =>
                from user in _store.GetById<User>(userId).ResOfOption(() => $"No user for ID: {userId}")
                from orders in
                    _store.GetAllWhere<Order>(o => o.UserId == user.Id && o.CompletionTimestamp.HasValue)
                    .ResOfOption(() => "No completed orders found for this user")
                select orders;

        public Task<Result<Order, string>> GetOrCreateActiveOrderForUser(int userId)
            =>
                from user in _userService.GetById(userId).ResOfOption(() => "No user for ID: {userId}")
                // from user in _store.GetById<User>(userId).ResOfOption(() => $"No user for ID: {userId}")
                from order in _store.SingleWhere<Order>(o => o.UserId == userId && !o.CompletionTimestamp.HasValue)
                    .Bind(o => o.Match<Order, Task<Option<Order>>>(
                        order => Task.FromResult(Option<Order>.Some(order)),
                        () => _store.Create<Order>().Map(order =>
                        {
                            order.CompletionTimestamp = Option<long>.None;
                            order.UserId = userId;
                            order.OrderLines = new Dictionary<int, int>();
                            return Option<Order>.Some(order);
                        })
                    ))
                    .ResOfOption(() => "Failed to locate or construct order for user")
                select order;

        public Task<Result<Order, string>> CompleteActiveOrder(int userId)
            =>
            (from user in _store.GetById<User>(userId).ResOfOption(() => $"No user for ID: {userId}")
            from activeOrder in _store.SingleWhere<Order>(o => o.UserId == userId && !o.CompletionTimestamp.HasValue)
                .ResOfOption(() => $"No active order exists for user: {userId}")
            select activeOrder)
            .Map(res => res.Bind(order => {
                if (order.OrderLines.Count == 0)
                {
                    return Result<Order,string>.AsError("Order does not contain any lines");
                }

                order.CompletionTimestamp = Option<long>.Some(DateTime.UtcNow.Ticks);
                return Result<Order,string>.AsSuccess(order);
            }));
    }
}