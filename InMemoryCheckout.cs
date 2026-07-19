public class InMemoryCheckout : ICheckout
{
    // Scanned items go into a basket of (Product, quantity)
    // We could also use an index-based system, but this is semantically nicer IMO
    public Dictionary<Product, int> Basket { get; set; }

    // Catalog holds list of allowed SKUs for early scanning rejection
    private Dictionary<string, Product> Catalog { get; set; }

    // Offers indexed by SKU for fast lookup during receipt calculation
    private Dictionary<string, Offer> Offers { get; set; }

    // Receipt is the log of prices and discounts; this could be done more basically with a running total instead
    private List<ReceiptLine> Receipt { get; set; }

    public InMemoryCheckout(List<Product> validProducts, List<Offer> offers)
    {
        Basket = [];
        Receipt = [];

        try
        {
            Catalog = validProducts.ToDictionary(p => p.Sku);
        }
        catch (ArgumentException e)
        {
            // Design decision to allow one SKU per name; a more advanced system might use a hidden primary key
            throw new InvalidDataException($"Product catalog cannot contain duplicate SKUs: {e}");
        }

        Offers = offers.ToDictionary(o => o.Sku);
    }

    public void UpdateOffers(List<Offer> offers)
    {
        Offers = offers.ToDictionary(o => o.Sku);
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

    private List<ReceiptLine> MakeLines(Product product, int quantity)
    {
        var lines = new List<ReceiptLine>();
        var totalDiscount = 0m;
        if (Offers.TryGetValue(product.Sku, out var offer))
        {
            var promotionGroups = quantity / offer.OfferQuantity; // how many times the offer can apply given the quantity of product
            // Integer division does a kind of floor for us

            var normalPricePerGroup = product.UnitPrice * offer.OfferQuantity;
            var discountPerGroup = normalPricePerGroup - offer.OfferPrice;
            totalDiscount = promotionGroups * discountPerGroup;
        }

        // Add individual items to the receipt (could also do one bulk insert here, but it's a design decision)
        for (var i = 0; i < quantity; i++)
        {
            lines.Add(new(product.Sku, product.UnitPrice, IsDiscount: false));
        }

        // Add discount value as a pseudo-SKU for the receipt. If we ever wanted to display this to a customer, it can just be read off
        if (totalDiscount > 0)
        {
            lines.Add(new($"Bulk discount ${product.Sku}", totalDiscount, IsDiscount: true));
        }

        return lines;
    }

    private void CalculateReceipt()
    {
        Receipt = []; // Clear the receipt for calculating

        foreach (var (item, qty) in Basket)
        {
            var receiptLines = MakeLines(item, qty);
            Receipt.AddRange(receiptLines);
        }
    }

    private decimal GetReceiptTotal()
    {
        CalculateReceipt();

        return Receipt.Sum(l => l.IsDiscount ? (-1 * l.Value) : l.Value);
    }

    public decimal GetTotalPrice() => GetReceiptTotal(); // Trigger receipt generation every time we want an up to date price
}
