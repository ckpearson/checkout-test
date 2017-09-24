using System;
using api.Services;
using Xunit;
using Moq;
using System.Threading.Tasks;
using api.Models;
using System.Linq;
using api.Utils;

namespace api_tests
{
    public class Fixture : IDisposable
    {
        public Fixture()
        {
            Store = new DummyDataStore(false);

            UserService = new UserService(Store);
            ProductService = new ProductService(Store);
            OrderService = new OrderService(Store, UserService, ProductService);
        }
        public void Dispose()
        {
        }

        public IDataStore Store { get; }

        public IUserService UserService { get; }
        public IProductService ProductService { get; }
        public IOrderService OrderService { get; }
    }

    // General testing class for sanity checking the various
    // service operations
    public class OperationTests
    {
        private readonly Fixture _fixture;

        public OperationTests()
        {
            _fixture = new Fixture();
        }

        [Fact]
        public async Task UserService_GetById_ReturnsNoneForNonExistentUser()
        {
            var result = await _fixture.UserService.GetById(1);

            Assert.False(result.HasValue);
        }

        [Fact]
        public async Task UserService_GetById_ReturnsNoneForMissingId()
        {
            var user = await _fixture.Store.Create<User>();
            var result = await _fixture.UserService.GetById(user.Id + 1);

            Assert.False(result.HasValue);
        }

        [Fact]
        public async Task UserService_GetById_ReturnsSomeForExistingUser()
        {
            var user = await _fixture.Store.Create<User>();
            var result = await _fixture.UserService.GetById(user.Id);

            Assert.Same(user, result.GetValueOrThrow());
        }

        [Fact]
        public async Task ProductService_GetById_ReturnsNoneForNonExistentProduct()
        {
            var result = await _fixture.ProductService.GetById(1);
            Assert.False(result.HasValue);
        }

        [Fact]
        public async Task ProductService_GetById_ReturnsNoneForMissingId()
        {
            var product = await _fixture.Store.Create<Product>();
            var result = await _fixture.ProductService.GetById(product.Id + 1);
            Assert.False(result.HasValue);
        }

        [Fact]
        public async Task ProductService_GetById_ReturnsSomeForExistingProduct()
        {
            var product = await _fixture.Store.Create<Product>();
            var result = await _fixture.ProductService.GetById(product.Id);

            Assert.Same(product, result.GetValueOrThrow());
        }

        [Fact]
        public async Task ProductService_GetAll_ReturnsEmptyForNoProducts()
        {
            var products = await _fixture.ProductService.GetAll();
            Assert.Empty(products);
        }

        [Fact]
        public async Task ProductService_GetAll_ReturnsAllExistingProducts()
        {
            var prod1 = await _fixture.Store.Create<Product>();
            var prod2 = await _fixture.Store.Create<Product>();

            var products = await _fixture.ProductService.GetAll();

            Assert.Equal(new[] { prod1.Id, prod2.Id }, products.Select(p => p.Id));
        }

        [Fact]
        public async Task OrderService_GetOrCreateActiveOrder_Error_IfNoUser()
        {
            var orderResult = await _fixture.OrderService.GetOrCreateActiveOrderForUser(1);
            Assert.False(orderResult.IsSuccess);
        }

        [Fact]
        public async Task OrderService_GetOrCreateActiveOrder_CreatesOrderIfNotAlreadyPresent()
        {
            var user = await _fixture.Store.Create<User>();
            var orderRes = await _fixture.OrderService.GetOrCreateActiveOrderForUser(user.Id);

            Assert.True(orderRes.IsSuccess);

            var order = orderRes.SuccessValOrThrow();

            var orderLookup = await _fixture.Store.GetById<Order>(order.Id);

            Assert.Same(order, orderLookup.GetValueOrThrow());
        }

        [Fact]
        public async Task OrderService_GetOrCreateActiveOrder_GetsExistingOrder()
        {
            var user = await _fixture.Store.Create<User>();
            var order = (await _fixture.OrderService.GetOrCreateActiveOrderForUser(user.Id)).SuccessValOrThrow();

            Assert.Same(order, (await _fixture.OrderService.GetOrCreateActiveOrderForUser(user.Id)).SuccessValOrThrow());
        }

        [Fact]
        public async Task OrderService_GetOrCreateActiveOrder_CreatesOrderIfNoActiveOrderPresent()
        {
            var user = await _fixture.Store.Create<User>();

            var originalOrder = await _fixture.Store.Create<Order>();
            originalOrder.CompletionTimestamp = Option<long>.Some(DateTime.UtcNow.Ticks);

            var activeOrder = await _fixture.OrderService.GetOrCreateActiveOrderForUser(user.Id);

            Assert.NotSame(originalOrder, activeOrder.SuccessValOrThrow());
        }

        [Fact]
        public async Task OrderService_GetCompletedOrders_ReturnsErrorIfNoUser()
        {
            var completedRes = await _fixture.OrderService.GetCompletedOrdersForUser(10);
            Assert.False(completedRes.IsSuccess);
        }

        [Fact]
        public async Task OrderService_GetCompletedOrders_ReturnsErrorIfNoneCompleted()
        {
            var user = await _fixture.Store.Create<User>();
            var order = await _fixture.OrderService.GetOrCreateActiveOrderForUser(user.Id);
            var completed = await _fixture.OrderService.GetCompletedOrdersForUser(user.Id);

            Assert.False(completed.IsSuccess);
        }

        [Fact]
        public async Task OrderService_GetCompletedOrders_ReturnsCompletedOrdersIfPresent()
        {
            var user = await _fixture.Store.Create<User>();
            var ord1 = (await _fixture.OrderService.GetOrCreateActiveOrderForUser(user.Id)).SuccessValOrThrow();
            ord1.CompletionTimestamp = Option<long>.Some(DateTime.UtcNow.Ticks);

            var completed = await _fixture.OrderService.GetCompletedOrdersForUser(user.Id);

            Assert.Equal(new[] { ord1 }, completed.SuccessValOrThrow());
        }

        [Fact]
        public async Task OrderService_AddProduct_ErrorIfNoUser()
        {
            var product = await _fixture.Store.Create<Product>();
            var order = await _fixture.OrderService.AddProductToActiveOrder(1, product.Id);

            Assert.False(order.IsSuccess);
        }

        [Fact]
        public async Task OrderService_AddProduct_ErrorIfNoProduct()
        {
            var user = await _fixture.Store.Create<User>();
            var order = await _fixture.OrderService.AddProductToActiveOrder(user.Id, 10);

            Assert.False(order.IsSuccess);
        }

        [Fact]
        public async Task OrderService_AddProduct_CreatesActiveOrderIfNone()
        {
            var user = await _fixture.Store.Create<User>();
            var product = await _fixture.Store.Create<Product>();

            var order = await _fixture.OrderService.AddProductToActiveOrder(user.Id, product.Id);

            Assert.True(order.IsSuccess);
        }

        [Fact]
        public async Task OrderService_AddProduct_AddsLineToOrder()
        {
            var user = await _fixture.Store.Create<User>();
            var product = await _fixture.Store.Create<Product>();

            var order = (await _fixture.OrderService.GetOrCreateActiveOrderForUser(user.Id)).SuccessValOrThrow();

            Assert.Empty(order.OrderLines);

            await _fixture.OrderService.AddProductToActiveOrder(user.Id, product.Id);

            Assert.True(order.OrderLines.ContainsKey(product.Id));
            Assert.Equal(1, order.OrderLines[product.Id]);
        }

        [Fact]
        public async Task OrderService_AddProduct_IncrementsQuantity()
        {
            var user = await _fixture.Store.Create<User>();
            var product = await _fixture.Store.Create<Product>();

            var order = (await _fixture.OrderService.GetOrCreateActiveOrderForUser(user.Id)).SuccessValOrThrow();

            await _fixture.OrderService.AddProductToActiveOrder(user.Id, product.Id);

            Assert.Equal(1, order.OrderLines[product.Id]);

            await _fixture.OrderService.AddProductToActiveOrder(user.Id, product.Id);

            Assert.Equal(2, order.OrderLines[product.Id]);
        }

        [Fact]
        public async Task OrderService_RemoveProduct_ErrorIfNoUser()
        {
            var product = await _fixture.Store.Create<Product>();

            var res = await _fixture.OrderService.RemoveProductFromActiveOrder(1, product.Id);

            Assert.False(res.IsSuccess);
        }

        [Fact]
        public async Task OrderService_RemoveProduct_ErrorIfNoProduct()
        {
            var user = await _fixture.Store.Create<User>();

            var res = await _fixture.OrderService.RemoveProductFromActiveOrder(user.Id, 10);

            Assert.False(res.IsSuccess);
        }

        [Fact]
        public async Task OrderService_RemoveProduct_DoesNothingIfProductNotPresentOnOrder()
        {
            var user = await _fixture.Store.Create<User>();
            var product = await _fixture.Store.Create<Product>();

            var res = await _fixture.OrderService.RemoveProductFromActiveOrder(user.Id, product.Id);

            Assert.True(res.IsSuccess);
        }

        [Fact]
        public async Task OrderService_RemoveProduct_DecrementsQuantity()
        {
            var user = await _fixture.Store.Create<User>();
            var product = await _fixture.Store.Create<Product>();

            var order = (await _fixture.OrderService.GetOrCreateActiveOrderForUser(user.Id)).SuccessValOrThrow();

            await _fixture.OrderService.AddProductToActiveOrder(user.Id, product.Id);
            await _fixture.OrderService.AddProductToActiveOrder(user.Id, product.Id);

            Assert.Equal(2, order.OrderLines[product.Id]);

            await _fixture.OrderService.RemoveProductFromActiveOrder(user.Id, product.Id);

            Assert.Equal(1, order.OrderLines[product.Id]);
        }

        [Fact]
        public async Task OrderService_RemoveProduct_RemovesLineIfQuantityBecomesZero()
        {
            var user = await _fixture.Store.Create<User>();
            var product = await _fixture.Store.Create<Product>();

            var order = (await _fixture.OrderService.GetOrCreateActiveOrderForUser(user.Id)).SuccessValOrThrow();

            await _fixture.OrderService.AddProductToActiveOrder(user.Id, product.Id);

            Assert.Equal(1, order.OrderLines[product.Id]);

            await _fixture.OrderService.RemoveProductFromActiveOrder(user.Id, product.Id);

            Assert.Empty(order.OrderLines);
        }

        [Fact]
        public async Task OrderService_SetQuantity_ErrorIfNoUser()
        {
            var product = await _fixture.Store.Create<Product>();

            var res = await _fixture.OrderService.SetProductQuanityOnActiveOrder(1, product.Id, 10);

            Assert.False(res.IsSuccess);
        }

        [Fact]
        public async Task OrderService_SetQuantity_ErrorIfNoProduct()
        {
            var user = await _fixture.Store.Create<User>();

            var res = await _fixture.OrderService.SetProductQuanityOnActiveOrder(user.Id, 10, 5);

            Assert.False(res.IsSuccess);
        }

        [Fact]
        public async Task OrderService_SetQuantity_AddsLineIfNotPresent()
        {
            var user = await _fixture.Store.Create<User>();
            var product = await _fixture.Store.Create<Product>();

            const int q = 10;

            var order = (await _fixture.OrderService.GetOrCreateActiveOrderForUser(user.Id)).SuccessValOrThrow();

            var res = await _fixture.OrderService.SetProductQuanityOnActiveOrder(user.Id, product.Id, q);

            Assert.True(order.OrderLines.ContainsKey(product.Id));
            Assert.Equal(q, order.OrderLines[product.Id]);
        }

        [Fact]
        public async Task OrderService_SetQuantity_UpdatesLine()
        {
            var user = await _fixture.Store.Create<User>();
            var product = await _fixture.Store.Create<Product>();

            const int initial = 5;
            const int final = 10;

            var order = (await _fixture.OrderService.GetOrCreateActiveOrderForUser(user.Id)).SuccessValOrThrow();

            order.OrderLines.Add(product.Id, initial);

            await _fixture.OrderService.SetProductQuanityOnActiveOrder(user.Id, product.Id, final);

            Assert.Equal(final, order.OrderLines[product.Id]);
        }

        [Fact]
        public async Task OrderService_SetQuantity_RemovesLineForZeroQuantity()
        {
            var user = await _fixture.Store.Create<User>();
            var product = await _fixture.Store.Create<Product>();

            var order = (await _fixture.OrderService.GetOrCreateActiveOrderForUser(user.Id)).SuccessValOrThrow();

            order.OrderLines.Add(product.Id, 10);

            await _fixture.OrderService.SetProductQuanityOnActiveOrder(user.Id, product.Id, 0);

            Assert.Empty(order.OrderLines);
        }

        [Fact]
        public async Task OrderService_SetQuantity_ErrorForNegativeQuantity()
        {
            var user = await _fixture.Store.Create<User>();
            var product = await _fixture.Store.Create<Product>();

            var res = await _fixture.OrderService.SetProductQuanityOnActiveOrder(user.Id, product.Id, -10);

            Assert.False(res.IsSuccess);
        }

        [Fact]
        public async Task OrderService_Clear_ErrorForNoUser()
        {
            var res = await _fixture.OrderService.ClearProductsForActiveOrder(10);
            Assert.False(res.IsSuccess);
        }

        [Fact]
        public async Task OrderService_Clear_DoesNothingForOrderWithNoLines()
        {
            var user = await _fixture.Store.Create<User>();
            var order = (await _fixture.OrderService.GetOrCreateActiveOrderForUser(user.Id)).SuccessValOrThrow();

            var res = await _fixture.OrderService.ClearProductsForActiveOrder(user.Id);

            Assert.True(res.IsSuccess);
        }

        [Fact]
        public async Task OrderService_Clear_RemovesLinesFromActiveOrder()
        {
            var user = await _fixture.Store.Create<User>();

            var order = (await _fixture.OrderService.GetOrCreateActiveOrderForUser(user.Id)).SuccessValOrThrow();
            order.OrderLines.Add(1, 10);
            order.OrderLines.Add(2, 5);

            var res = await _fixture.OrderService.ClearProductsForActiveOrder(user.Id);
            Assert.True(res.IsSuccess);

            Assert.Empty(order.OrderLines);
        }

        [Fact]
        public async Task OrderService_CompleteOrder_ErrorIfNoUser()
        {
            var orderUser = await _fixture.Store.Create<User>();
            var order = (await _fixture.OrderService.GetOrCreateActiveOrderForUser(orderUser.Id)).SuccessValOrThrow();

            var res = await _fixture.OrderService.CompleteActiveOrder(orderUser.Id + 1);

            Assert.False(res.IsSuccess);
        }

        [Fact]
        public async Task OrderService_CompleteOrder_ErrorIfNoActiveOrder()
        {
            var user = await _fixture.Store.Create<User>();

            var res = await _fixture.OrderService.CompleteActiveOrder(user.Id);

            Assert.False(res.IsSuccess);
        }

        [Fact]
        public async Task OrderService_CompleteOrder_ErrorIfNoLines()
        {
            var user = await _fixture.Store.Create<User>();
            var order = (await _fixture.OrderService.GetOrCreateActiveOrderForUser(user.Id)).SuccessValOrThrow();

            var res = await _fixture.OrderService.CompleteActiveOrder(user.Id);

            Assert.False(res.IsSuccess);
        }

        [Fact]
        public async Task OrderService_CompleteOrder_SuccessAndMarksOrderWithLinesAsComplete()
        {
            var user = await _fixture.Store.Create<User>();
            var product = await _fixture.Store.Create<Product>();

            var order = (await _fixture.OrderService.GetOrCreateActiveOrderForUser(user.Id)).SuccessValOrThrow();

            await _fixture.OrderService.AddProductToActiveOrder(user.Id, product.Id);

            Assert.False(order.CompletionTimestamp.HasValue);

            var res = await _fixture.OrderService.CompleteActiveOrder(user.Id);

            Assert.True(res.IsSuccess);
            Assert.True(order.CompletionTimestamp.HasValue);
        }
    }
}