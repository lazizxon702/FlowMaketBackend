using FlowMarketService.Models;

namespace FlowMarketService.Contracts.Commerce;

public record AddCartItemRequest(int ProductId, int Quantity);
public record PatchCartItemRequest(int? Quantity, bool? IsSelected);
public record ApplyPromoRequest(string Code);

public record AddressRequest(
    string Label,
    int DistrictId,
    string Street,
    string HouseNumber,
    string? Apartment,
    string? Comment,
    double? Latitude,
    double? Longitude,
    bool IsPrimary);

public record AddCardRequest(
    string CardNumber,
    int ExpiryMonth,
    int ExpiryYear,
    int Cvv,
    string CardholderName,
    bool SetPrimary);

public record CheckoutRequest(
    int AddressId,
    int ShippingOptionId,
    OrderPaymentKind PaymentMode,
    Guid? SavedPaymentMethodId);
