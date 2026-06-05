using FlowMarketService.Models;

namespace FlowMarketService.Contracts;

public record OrderItemLineResponse(
    int ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    string? VariantDescription);

public record OrderResponse(
    int Id,
    Guid? UserId,
    string CustomerName,
    string Email,
    string? Phone,
    OrderStatus Status,
    decimal Subtotal,
    decimal ShippingFee,
    decimal Discount,
    decimal Total,
    string? PromoCodeApplied,
    OrderPaymentKind? PaymentMode,
    DateTime CreatedAtUtc,
    IReadOnlyList<OrderItemLineResponse> Items);

public record CreateOrderItemRequest(int ProductId, int Quantity);

public record CreateOrderRequest(
    string CustomerName,
    string Email,
    string? Phone,
    IReadOnlyList<CreateOrderItemRequest> Items);

public record UpdateOrderStatusRequest(OrderStatus Status);