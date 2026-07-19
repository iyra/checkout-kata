interface ICheckout
{
    void Scan(string item);
    decimal GetTotalPrice();
    void UpdateOffers(List<Offer> offers);
}