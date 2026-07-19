var catalog = new List<Product>
{
    new("A", 0.50m),
    new("B", 0.30m),
    new("C", 0.20m),
    new("D", 0.15m),
    new("E", 0.40m),
    new("F", 0.10m),
};

var offers = new List<Offer>
{
    new("A", 3, 1.30m),
    new("B", 2, 0.45m),
    new("E", 4, 1.30m),
    new("F", 3, 0.20m),
};

if (args.Length == 0)
{
    Console.WriteLine("no SKUs provided. pass SKUs as arguments.");
    return;
}

var checkout = new InMemoryCheckout(catalog, offers);

foreach (var sku in args)
{
    try
    {
        checkout.Scan(sku);
    }
    catch
    {
        Console.WriteLine($"skipping unknown SKU: {sku}");
    }
}

var total = checkout.GetTotalPrice();
Console.WriteLine($"total: ${total}");