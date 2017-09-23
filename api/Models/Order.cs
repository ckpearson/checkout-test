using System.Collections.Generic;

namespace api.Models
{
    public class Order : IDataItem
    {
        public int Id { get; set; }
        public User User { get; set; }
        public Dictionary<Product, int> OrderLines { get; set; }
    }
}