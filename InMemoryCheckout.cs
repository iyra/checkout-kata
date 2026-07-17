public class InMemoryCheckout : ICheckout
{
    private List<Product> Basket { get; set; }

    public InMemoryCheckout()
    {
        Basket = [];
    }

    public void Scan(string Sku) { }

    public decimal GetTotalPrice() => 0; // dummy value for now
}