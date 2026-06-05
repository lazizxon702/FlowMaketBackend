namespace FlowMarketService.Models;

public class Order
{
    public int Id { get; set; }

    public Guid? UserId { get; set; }
    public ApplicationUser? User { get; set; }

    public string CustomerName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }

    public int? ShippingAddressId { get; set; }
    public UserAddress? ShippingAddress { get; set; }

    public int? ShippingOptionId { get; set; }
    public ShippingOption? ShippingOption { get; set; }

    public Guid? SavedPaymentMethodId { get; set; }
    public SavedPaymentMethod? SavedPaymentMethod { get; set; }

    public OrderPaymentKind? PaymentMode { get; set; }
    public string? PromoCodeApplied { get; set; }

    public decimal Subtotal { get; set; }
    public decimal ShippingFee { get; set; }
    public decimal Discount { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public decimal Total { get; set; }
    public DateTime CreatedAtUtc { get; set; }

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}