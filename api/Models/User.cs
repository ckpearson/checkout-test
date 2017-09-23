namespace api.Models
{
    public class User : IDataItem
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Name { get; set; }
    }
}