using System.Collections.Generic;
using Refit;

namespace api_client
{
    public interface IOrderApi
    {
        [Get("/api/products")]
        IEnumerable<Product> GetAllProducts();

        [Get("/api/products/{id}")]
        Product GetProductById(int id);

        [Get("/api/orders/completed/{userId}")]
        IEnumerable<Order> GetCompletedOrdersForUser(int userId);

        [Get("/api/orders/current/{userId}")]
        Order GetCurrentOrderForUser(int userId);

        [Get("/api/orders/current/{userId}/addOneOf/{productId}")]
        Order AddOneOfProductToCurrentOrderForUser(int userId, int productId);

        [Get("/api/orders/current/{userId}/removeOneOf/{productId}")]
        Order RemoveOneOfProductFromCurrentOrderForUser(int userId, int productId);

        [Get("/api/orders/current/{userId}/setQuantity/{productId}/{quantity}")]
        Order SetQuantityForProductOnCurrentOrderForUser(int userId, int productId, int quantity);

        [Get("/api/orders/current/{userId}/clear")]
        Order ClearOrderLinesFromCurrentOrderForUser(int userId);

        [Get("/api/orders/current/{userId}/complete")]
        Order CompleteCurrentOrderForUser(int userId);
    }
}