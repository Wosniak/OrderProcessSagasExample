using Model;

namespace NewOrder.Generator
{
    internal static class OrderFactory
    {
        static readonly Random rnd = new Random();

        public static Order CreateOrder()
        {
            var Customer = CreateCustomer(Guid.NewGuid());

            var order = new Order(Customer);

            ((List<OrderItem>)order.Items).Add(new OrderItem(CreateProduct(), GetRandom(1, 15)));

            return order;
        }

        static Customer CreateCustomer(Guid id)
        {
            return new Customer()
            {
                Id = id,
                Name = $"Cliente {id}"
            };
        }

        static Product CreateProduct()
        {

            var sku = GetRandom(111111, 9999999);
            Console.WriteLine($"Gerando produto {sku}");

            return new Product()
            {
                SKU = sku,
                Bunddle = false,
                CommisionPercentage = (decimal)(GetRandom(0, 1500) / 100.00),
                Description = $"Produto {sku}",
                HasRoyaltiesFees = true,
                Price = (decimal)(GetRandom(100, 100000) / 100.00),
                Type = sku % 2 == 0 ? ProductType.Fisical : ProductType.Subscription
            };
        }

        static int GetRandom(int from, int to)
        {

            return rnd.Next(from, to);
        }

    }
}
