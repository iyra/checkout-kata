Console.WriteLine("Hello, World!");

public record Offer(string Sku, int OfferQuantity, decimal OfferPrice); // Discount rule is an offer, associated with a SKU
public record Product(string Sku, decimal UnitPrice); // Each SKU in the catalog
public record ReceiptLine(string Sku, decimal Value, bool IsDiscount); // For an accounting of purchased products and discounts
