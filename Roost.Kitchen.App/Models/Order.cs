namespace Roost.Kitchen.App.Models
{
    public class Order
    {
        public long? OrderNumber { get; set; }

        public DateTime OrderDateTime { get; set; }

        public List<OrderItem>? orderItems { set; get; }

        public decimal? SubTotal { get; set; }

        public decimal? Total { get; set; }

        public decimal? SalesTax { get; set; }
    }
}
