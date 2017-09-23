using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Models;
using api.Models.dtos;
using api.Services;
using api.Utils;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Route("api/orders")]
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [Route("completed/{userId}")]
        public async Task<IActionResult> GetCompletedOrdersForUser(int userId)
        {
            return (await _orderService.GetCompletedOrdersForUser(userId))
                .Map(v => v.Select(OrderDto.FromModel))
                .Match(Ok, e => (IActionResult)StatusCode(500,e));
        }
    }
}