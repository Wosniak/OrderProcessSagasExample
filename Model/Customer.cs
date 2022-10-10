namespace Model
{
    public class Customer
    {
        public Customer()
        {
            Name = string.Empty;
        }

        public Guid Id { get; set; }
        public string Name { get; set; }

    }
}
