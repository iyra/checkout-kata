interface ICheckout
{
    void Scan(string item);
    decimal GetTotalPrice();
}