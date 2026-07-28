namespace Checkout.Tests;

public class CheckoutTests
{
    private readonly Product aProduct = new("A", 50);
    private readonly Product bProduct = new("B", 30);
    private readonly Product cProduct = new("C", 20);

    private readonly List<Offer> offers = [new("A", 3, 130m), new("B", 2, 35m)];

    [Fact]
    public void CheckScanExistingProducts()
    {
        // arrange
        var products = new List<Product>() { aProduct, bProduct, cProduct };

        var checkout = new InMemoryCheckout(products, offers);

        // act
        checkout.Scan("A");
        checkout.Scan("B");
        checkout.Scan("B");

        // assert via total: 1 A at 50 + 2 B's at 2 for 35
        Assert.Equal(50m + 35m, checkout.GetTotalPrice());
    }

    [Fact]
    public void CheckOneDiscountCalculates()
    {
        // arrange
        var products = new List<Product>() { aProduct, bProduct, cProduct };

        var checkout = new InMemoryCheckout(products, offers);

        // act
        checkout.Scan("A");
        checkout.Scan("A");
        checkout.Scan("A");

        // assert
        Assert.Equal(130m, checkout.GetTotalPrice());
    }

    [Fact]
    public void CheckMultipleDiscountGroupCalculates()
    {
        // arrange
        var products = new List<Product>() { aProduct, bProduct, cProduct };

        var checkout = new InMemoryCheckout(products, offers);

        // act
        checkout.Scan("A");
        checkout.Scan("A");
        checkout.Scan("A");

        checkout.Scan("A");
        checkout.Scan("A");
        checkout.Scan("A");

        // assert
        Assert.Equal(260m, checkout.GetTotalPrice());
    }

    [Fact]
    public void CheckMultipleProductsCalculatesWithRemainder()
    {
        // arrange
        var products = new List<Product>() { aProduct, bProduct, cProduct };

        var checkout = new InMemoryCheckout(products, offers);

        // act
        checkout.Scan("A");
        checkout.Scan("A");
        checkout.Scan("A");

        checkout.Scan("B");
        checkout.Scan("B");
        checkout.Scan("B"); // Discount shouldn't apply to this one

        // assert
        Assert.Equal(130m + 35m + 30m, checkout.GetTotalPrice());
    }

    [Fact]
    public void CheckUpdateOffersRecalculates()
    {
        // arrange
        var products = new List<Product>() { aProduct, bProduct, cProduct };

        var checkout = new InMemoryCheckout(products, offers);

        checkout.Scan("A");
        checkout.Scan("A");
        checkout.Scan("A");

        Assert.Equal(130m, checkout.GetTotalPrice()); // offer applies

        // act - remove all offers
        checkout.UpdateOffers([]);

        // assert - now full price
        Assert.Equal(150m, checkout.GetTotalPrice());
    }

    [Fact]
    public void CheckNegativePriceThrows()
    {
        // arrange
        var negativeProduct = new Product("X", -10m);
        var products = new List<Product>() { negativeProduct };

        // act & assert that negative unit prices have no meaning
        Assert.Throws<InvalidDataException>(() => new InMemoryCheckout(products, []));
    }

    [Fact]
    public void CheckNegativeOfferThrows()
    {
        // arrange
        var products = new List<Product>() { aProduct };
        var negativeOffer = new Offer("A", 3, -30m);

        // act & assert that negative prices aren't possible in this supermarket
        Assert.Throws<InvalidDataException>(() => new InMemoryCheckout(products, [negativeOffer]));
    }

    [Fact]
    public void CheckLargeNumberOfItemsCalculates()
    {
        // arrange
        var products = new List<Product>() { aProduct };

        var checkout = new InMemoryCheckout(products, offers);

        // act
        // Scan 100 A's; offer is 3 for 130, so 33 groups (99 items) + 1 leftover
        for (var i = 0; i < 100; i++)
        {
            checkout.Scan("A");
        }

        // assert 33 * 130 (grouped items) + 1 * 50 (standard price)
        Assert.Equal(33 * 130m + 50m, checkout.GetTotalPrice());
    }

    [Fact]
    public void CheckVeryLargeNumberOfItemsCalculates()
    {
        // arrange
        var products = new List<Product>() { aProduct };

        var checkout = new InMemoryCheckout(products, offers);

        // act - scan 100,000 A's; offer is 3 for 130, so 33333 groups (99999 items) + 1 remainder
        for (var i = 0; i < 100_000; i++)
        {
            checkout.Scan("A");
        }

        // assert - 33333 * 130 + 1 * 50
        Assert.Equal(33333 * 130m + 50m, checkout.GetTotalPrice());
    }

    [Fact]
    public void CheckOutOfOrderScansApplyPromotion()
    {
        // arrange
        var products = new List<Product>() { aProduct, bProduct };

        var checkout = new InMemoryCheckout(products, offers);

        // act by interspresing A and B scans; both should still qualify for their offers
        checkout.Scan("B");
        checkout.Scan("A");
        checkout.Scan("B");
        checkout.Scan("A");
        checkout.Scan("A");

        // assert that 3 A's at 3 for 130, 2 B's at 2 for 35
        Assert.Equal(130m + 35m, checkout.GetTotalPrice());
    }

    [Fact]
    public void CheckZeroItemsReturnsZero()
    {
        // arrange
        var products = new List<Product>() { aProduct, bProduct, cProduct };

        var checkout = new InMemoryCheckout(products, offers);

        // Act (do nothing)

        // assert
        Assert.Equal(0m, checkout.GetTotalPrice());
    }

    [Fact]
    public void CheckDuplicateSkuThrows()
    {
        // arrange
        var duplicateProduct = new Product("A", 99m);
        var products = new List<Product>() { aProduct, duplicateProduct };

        // act and assert
        Assert.Throws<InvalidDataException>(() => new InMemoryCheckout(products, offers));
    }

    [Fact]
    public void CheckDuplicateOfferSkuThrows()
    {
        // arrange two offers for the same SKU
        var products = new List<Product>() { aProduct };
        var duplicateOffers = new List<Offer> { new("A", 3, 130m), new("A", 2, 90m) };

        // Act and assert that only one offer per SKU is allowed
        Assert.Throws<InvalidDataException>(() => new InMemoryCheckout(products, duplicateOffers));
    }

    [Fact]
    public void CheckUpdateOffersWithDuplicateSkuThrows()
    {
        // arrange
        var products = new List<Product>() { aProduct };
        var checkout = new InMemoryCheckout(products, []);
        var duplicateOffers = new List<Offer> { new("A", 3, 130m), new("A", 2, 90m) };

        // act and assert that duplicate SKUs in UpdateOffers are rejected
        Assert.Throws<InvalidDataException>(() => checkout.UpdateOffers(duplicateOffers));
    }

    [Fact]
    public void CheckZeroOfferQuantityThrows()
    {
        // arrange an offer with quantity zero, which would cause division by zero when calculating promotions
        var products = new List<Product>() { aProduct };
        var zeroQuantityOffer = new Offer("A", 0, 130m);

        // act and assert
        Assert.Throws<InvalidDataException>(() => new InMemoryCheckout(products, [zeroQuantityOffer]));
    }

    [Fact]
    public void CheckGetTotalPriceCalledTwiceReturnsSameValue()
    {
        // arrange
        var products = new List<Product>() { aProduct, bProduct };

        var checkout = new InMemoryCheckout(products, offers);

        checkout.Scan("A");
        checkout.Scan("A");
        checkout.Scan("A");
        checkout.Scan("B");
        checkout.Scan("B");

        // act
        var firstCall = checkout.GetTotalPrice();
        var secondCall = checkout.GetTotalPrice();

        // assert receipt is cleared and recalculated each time, so both calls should match
        Assert.Equal(firstCall, secondCall);
    }
}
