public class InMemoryCheckout : ICheckout
{
    public Dictionary<Product, int> Basket { get; set; }
    private Dictionary<string, Product> Catalog { get; set; }

    public InMemoryCheckout(List<Product> validProducts)
    {
        Basket = [];

        try
        {
            Catalog = validProducts.ToDictionary(p => p.Sku);
        }
        catch (ArgumentException e)
        {
            throw new InvalidDataException($"Product catalog cannot contain duplicate SKUs: {e}");
        }
    }

    public void Scan(string Sku)
    {
        var product = Catalog.GetValueOrDefault(Sku) ?? throw new InvalidOperationException($"Product {Sku} doesn't exist in the catalog");

        if (!Basket.ContainsKey(product))
        {
            Basket.Add(product, 1); // Product is being scanned for the first time, add one to the basket
        }
        else
        {
            Basket[product] += 1; // Subsequent scan, increment quantity in the basket
        }
    }

    public decimal GetTotalPrice() => 0; // dummy value for now
}

/*
A - $5, 3 for 10

add A
add A
add A - discountable detected
X = get price without discount (product.UnitPrice * offer.OfferQuantity) = 5 * 3 = $15
Y = get price with discount (offer.OfferPrice) = $10
saving = X - Y = $5
receipt.Add(new ReceiptItem(Product = null, Discount = $5))
*/