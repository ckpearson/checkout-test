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

        private Task<Result<User, string>> GetUserById(int id)
            => _userService.GetById(id).ResOfOption(() => $"No user for ID: {id}");

        private Task<Result<Product, string>> GetProductById(int id)
            => _productService.GetById(id).ResOfOption(() => $"No product for ID: {id}");

        private Task<Result<Order, string>> ModifyOrderForProduct(
            int userId,
            int productId,
            Func<Order, Product, Result<Order, string>> modBind)
        =>
            (from product in GetProductById(productId)
             from activeOrder in GetOrCreateActiveOrderForUser(userId)
             select Tuple.Create(activeOrder, product))
            .Map(res => res.Bind(t =>
            {
                t.Deconstruct(out var activeOrder, out var product);
                return modBind(activeOrder, product);
            }));

        /*
            ASSUMPTIONS:
                1. That it is ok for further add ops to increase quantity
                2. That it is ok for an order to not already exist

            IMPLEMENTATION:
                Obviously were this to be backed by some proper data store, there would
                likely need to be some sort of commit-phase here.
         */
        public Task<Result<Order, string>> AddProductToActiveOrder(int userId, int productId)
            => ModifyOrderForProduct(userId, productId,
                (activeOrder, product) =>
                {
                    if (activeOrder.OrderLines.ContainsKey(product.Id))
                    {
                        activeOrder.OrderLines[product.Id]++;
                    }
                    else
                    {
                        activeOrder.OrderLines.Add(product.Id, 1);
                    }
                    return Result<Order, string>.AsSuccess(activeOrder);
                });

        /*
            ASSUMPTIONS:
                1. That further remove ops can decrease quantity
                2. That line should be cleared if quantity <= 0
                3. That it isn't an error if that product isn't present on the order

            IMPLEMENTATION:
                This would require a commit-phase were a proper store used.
         */
        public Task<Result<Order, string>> RemoveProductFromActiveOrder(int userId, int productId)
            => ModifyOrderForProduct(userId, productId,
                (activeOrder, product) =>
                {
                    if (activeOrder.OrderLines.ContainsKey(product.Id))
                    {
                        var quantity = activeOrder.OrderLines[product.Id];
                        if (quantity - 1 <= 0)
                        {
                            activeOrder.OrderLines.Remove(product.Id);
                        }
                        else
                        {
                            activeOrder.OrderLines[product.Id]--;
                        }
                    }
                    return Result<Order, string>.AsSuccess(activeOrder);
                });

        public Task<Result<Order, string>> SetProductQuanityOnActiveOrder(int userId, int productId, int quantity)
            => ModifyOrderForProduct(userId, productId,
                (activeOrder, product) =>
                {
                    if (quantity < 0)
                    {
                        return Result<Order, string>.AsError($"Quantity must be positive; {quantity} is not valid");
                    }

                    if (quantity == 0 && activeOrder.OrderLines.ContainsKey(product.Id))
                    {
                        activeOrder.OrderLines.Remove(product.Id);
                    }
                    else
                    {
                        activeOrder.OrderLines[product.Id] = quantity;
                    }

                    return Result<Order, string>.AsSuccess(activeOrder);
                });

        public Task<Result<Order, string>> ClearProductsForActiveOrder(int userId)
            =>
            GetOrCreateActiveOrderForUser(userId)
            .Map(res => res.Map(order =>
            {
                order.OrderLines.Clear();
                return order;
            }));

        public Task<Result<IEnumerable<Order>, string>> GetCompletedOrdersForUser(int userId)
            =>
                from user in GetUserById(userId)
                from orders in
                    _store.GetAllWhere<Order>(o => o.UserId == user.Id && o.CompletionTimestamp.HasValue)
                    .ResOfOption(() => "No completed orders found for this user")
                select orders;

        public Task<Result<Order, string>> GetOrCreateActiveOrderForUser(int userId)
            =>
                from user in GetUserById(userId)
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
            => (from user in GetUserById(userId)
                from activeOrder in
                     _store.SingleWhere<Order>(o => o.UserId == user.Id && !o.CompletionTimestamp.HasValue)
                    .ResOfOption(() => $"Unable to locate an active order for user: {user.Id}")
                select activeOrder)
            .Map(res => res.Bind(order =>
            {
                if (order.OrderLines.Count == 0)
                {
                    return Result<Order, string>.AsError("Order does not contain any lines");
                }

                order.CompletionTimestamp = Option<long>.Some(DateTime.UtcNow.Ticks);
                return Result<Order, string>.AsSuccess(order);
            }));
    }
}