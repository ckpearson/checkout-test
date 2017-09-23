using System.Collections.Generic;
using System.Threading.Tasks;
using api.Models;
using api.Utils;

namespace api.Services
{
    public interface IOrderService
    {
        Task<Result<IEnumerable<Order>, string>> GetCompletedOrdersForUser(int userId);
        Task<Order> GetOrCreateActiveOrderForUser(int userId);
        Task<Result<Order, string>> AddProductToOrder(int orderId, int productId);
        Task<Result<Order, string>> RemoveProductFromOrder(int orderId, int productId);
        Task<bool> OrderBelongsToUser(int userId);
    }
}