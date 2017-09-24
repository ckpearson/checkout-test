using System.Collections.Generic;

namespace api_client
{
    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public Dictionary<int, int> OrderLines { get; set; }
        public long? CompletionTimestamp { get; set; }
    }
}