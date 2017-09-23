using System.Collections.Generic;
using System.Threading.Tasks;
using api.Models;
using api.Utils;

namespace api.Services
{
    public interface IOrderService
    {
        Task<Result<IEnumerable<Order>, string>> GetCompletedOrdersForUser(int userId);
        Task<Result<Order, string>> GetOrCreateActiveOrderForUser(int userId);
        Task<Result<Order, string>> AddProductToActiveOrder(int userId, int productId);
        Task<Result<Order, string>> RemoveProductFromActiveOrder(int userId, int productId);
        Task<Result<Order, string>> SetProductQuanityOnActiveOrder(int userId, int productId, int quantity);
        Task<Result<Order, string>> ClearProductsForActiveOrder(int userId);
        Task<bool> OrderBelongsToUser(int userId);
    }
}