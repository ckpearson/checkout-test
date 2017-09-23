using System.Collections.Generic;
using api.Utils;

namespace api.Models.dtos
{
    public class OrderDto
    {
        public int Id {get;set;}
        public int UserId {get;set;}
        public Dictionary<int,int> OrderLines {get;set;}
        public long? CompletionTimestamp {get;set;}

        public static OrderDto FromModel(Order order)
            => new OrderDto
            {
                Id = order.Id,
                UserId = order.UserId,
                OrderLines = order.OrderLines,
                CompletionTimestamp = order.CompletionTimestamp.Match(v => v, () => (long?)null)
            };
    }
}