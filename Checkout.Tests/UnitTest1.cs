namespace Checkout.Tests;

public class CheckoutTests
{
    private readonly Product aProduct = new("A", 50, [new Offer(3, 130m)]);
    private readonly Product bProduct = new("B", 30, [new Offer(2, 35m)]);
    private readonly Product cProduct = new("C", 20, []);

    [Fact]
    public void CheckScanExistingProducts()
    {
        // arrange
        var products = new List<Product>() { aProduct, bProduct, cProduct };

        var checkout = new InMemoryCheckout(products);

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

        var checkout = new InMemoryCheckout(products);

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

        var checkout = new InMemoryCheckout(products);

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

        var checkout = new InMemoryCheckout(products);

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
}
