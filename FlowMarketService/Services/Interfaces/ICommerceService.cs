using FlowMarketService.Common;
using FlowMarketService.Contracts.Commerce;
using FlowMarketService.Models;

namespace FlowMarketService.Services.Interfaces;

public interface ICommerceService
{
    Task<Result<object>> GetCartAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<object>> AddCartItemAsync(Guid userId, AddCartItemRequest request, CancellationToken cancellationToken = default);
    Task<Result<object>> PatchCartItemAsync(Guid userId, int itemId, PatchCartItemRequest request, CancellationToken cancellationToken = default);
    Task<Result<object?>> RemoveCartItemAsync(Guid userId, int itemId, CancellationToken cancellationToken = default);
    Task<Result<object>> ApplyPromoAsync(Guid userId, ApplyPromoRequest request, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<object>>> GetShippingOptionsAsync(CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<object>>> ListAddressesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<int>> CreateAddressAsync(Guid userId, AddressRequest request, CancellationToken cancellationToken = default);
    Task<Result<object>> UpdateAddressAsync(Guid userId, int id, AddressRequest request, CancellationToken cancellationToken = default);
    Task<Result<object?>> DeleteAddressAsync(Guid userId, int id, CancellationToken cancellationToken = default);
    Task<Result<object>> SetPrimaryAddressAsync(Guid userId, int id, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<object>>> GetDistrictsAsync(string? city, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<object>>> ListCardsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<Guid>> AddCardAsync(Guid userId, AddCardRequest request, CancellationToken cancellationToken = default);
    Task<Result<object?>> DeleteCardAsync(Guid userId, Guid cardId, CancellationToken cancellationToken = default);
    Task<Result<object>> SetPrimaryCardAsync(Guid userId, Guid cardId, CancellationToken cancellationToken = default);
    Task<Result<object>> ConfirmCheckoutAsync(Guid userId, CheckoutRequest request, CancellationToken cancellationToken = default);
}
