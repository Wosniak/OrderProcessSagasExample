namespace Model
{
    public class Product
    {
        public Product()
        {
            Description = String.Empty;
            IncludedProducts = new List<Product>();
        }

        public int SKU { get; set; }

        public ProductType Type { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public bool Bunddle { get; set; }
        public decimal CommisionPercentage { get; set; }
        public bool HasRoyaltiesFees { get; set; }
        public IEnumerable<Product> IncludedProducts { private set; get; }

    }
}
