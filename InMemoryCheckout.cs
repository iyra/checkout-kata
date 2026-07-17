public class InMemoryCheckout : ICheckout
{
    public Dictionary<Product, int> Basket { get; set; }
    private Dictionary<string, Product> Catalog { get; set; }
    private List<ReceiptLine> Receipt { get; set; }

    public InMemoryCheckout(List<Product> validProducts)
    {
        Basket = [];
        Receipt = [];

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

    private List<ReceiptLine> MakeLines(Product product, int quantity)
    {
        var lines = new List<ReceiptLine>();
        var totalDiscount = 0m;
        if (product.Offers.Any())
        {
            var offer = product.Offers.First();
            var promotionGroups = quantity / offer.OfferQuantity; // how many times the offer can apply given the quantity of product

            var normalPricePerGroup = product.UnitPrice * offer.OfferQuantity;
            var discountPerGroup = normalPricePerGroup - offer.OfferPrice;
            totalDiscount = promotionGroups * discountPerGroup;
        }

        for (var i = 0; i < quantity; i++)
        {
            lines.Add(new(product.Sku, product.UnitPrice, IsDiscount: false));
        }

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

/*
A - $5, 3 for 10

add A
add A
add A - discountable detected
X = get price without discount (product.UnitPrice * offer.OfferQuantity) = 5 * 3 = $15
Y = get price with discount (offer.OfferPrice) = $10
saving = X - Y = $5
receipt.Add(new ReceiptItem(Product = null, Discount = $5))

A $5
A $5
A $5
A -$5

*/