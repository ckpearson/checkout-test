using System.Collections.Generic;
using api.Utils;

namespace api.Models
{
    public class Order : IDataItem
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public Dictionary<int, int> OrderLines { get; set; }
        public Option<long> CompletionTimestamp {get;set;}
    }
}