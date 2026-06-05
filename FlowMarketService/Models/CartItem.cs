namespace FlowMarketService.Models;

public class CartItem
{
    public int Id { get; set; }
    public int CartId { get; set; }
    public ShoppingCart Cart { get; set; } = null!;

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int Quantity { get; set; }
    public bool IsSelected { get; set; } = true;
    public decimal UnitPriceSnapshot { get; set; }
    public string? VariantLabel { get; set; }
}
