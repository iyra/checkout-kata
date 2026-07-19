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

        // assert
        Assert.Contains(aProduct, checkout.Basket);
        Assert.Equal(1, checkout.Basket[aProduct]);

        Assert.Contains(bProduct, checkout.Basket);
        Assert.Equal(2, checkout.Basket[bProduct]);
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

    // negative price test
    // negative offer test
    // large # of items test
    // 0 items test
    // sku same name test
    // getreceipttotal clears receipt
}
