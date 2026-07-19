public record Offer(string Sku, int OfferQuantity, decimal OfferPrice); // discount rule associated with a SKU
public record Product(string Sku, decimal UnitPrice); // each SKU in the catalog
public record ReceiptLine(string Sku, decimal Value, bool IsDiscount); // accounting of purchased products and discounts