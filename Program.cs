Console.WriteLine("Hello, World!");

public record Offer(int OfferQuantity, decimal OfferPrice); // Discount rule is an offer
public record Product(string Sku, decimal UnitPrice, List<Offer> Offers); // Each SKU optionally has offers applied to it
public record ReceiptLine(string Sku, decimal? Discount); // For an accounting of purchased products and discounts
