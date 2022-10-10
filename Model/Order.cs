namespace Model
{
    public class Order
    {
        public Order(Customer customer)
        {
            Customer = customer;
            Items = new List<OrderItem>();
        }

        public Customer Customer { get; private set; }
        public IEnumerable<OrderItem> Items { get; set; }

        public Status Status { get; set; }

    }

    public class OrderItem
    {
        public OrderItem(Product product, int quantity)
        {
            Product = product;
            Quantity = quantity;
        }

        public Product Product { get; private set; }
        public int Quantity { get; set; }
    }
}