namespace Roost.Kitchen.App.Models
{
    public class OrderItem
    {
        public Guid? OrderItemId { get; set; }

        public Item? Item { set; get; }

        public int Quantity { get; set; }

        public decimal? Price { get; set; }

        public List<Option> Options { set; get; } = new List<Option>();
    }
}
