using FlowMarketService.Common;
using FlowMarketService.Contracts.Commerce;
using FlowMarketService.Data;
using FlowMarketService.Infrastructure;
using FlowMarketService.Models;
using FlowMarketService.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FlowMarketService.Services;

public class CommerceService(AppDbContext db) : ICommerceService
{
    public async Task<Result<object>> GetCartAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var cart = await GetOrCreateCartAsync(db, userId, cancellationToken);
        var items = await db.CartItems.AsNoTracking()
            .Where(i => i.CartId == cart.Id)
            .Include(i => i.Product)
            .Select(i => new
            {
                i.Id,
                i.ProductId,
                productName = i.Product.Name,
                i.Quantity,
                i.IsSelected,
                unitPrice = i.UnitPriceSnapshot,
                i.VariantLabel,
                imageUrl = i.Product.ImageUrl
            })
            .ToListAsync(cancellationToken);
        return Result<object>.Ok(new { cart.Id, cart.AppliedPromoCode, items });
    }

    public async Task<Result<object>> AddCartItemAsync(Guid userId, AddCartItemRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.Quantity <= 0)
            return Result<object>.Fail("Miqdor 0 dan katta bo‘lishi kerak.");

        var p = await db.Products.FirstOrDefaultAsync(x => x.Id == request.ProductId && x.IsActive, cancellationToken);
        if (p is null)
            return Result<object>.Fail("Mahsulot yo'q.");
        var cart = await GetOrCreateCartAsync(db, userId, cancellationToken);
        var existing = await db.CartItems.FirstOrDefaultAsync(i => i.CartId == cart.Id && i.ProductId == request.ProductId,
            cancellationToken);
        if (existing is not null)
        {
            existing.Quantity += request.Quantity;
            existing.UnitPriceSnapshot = p.Price;
        }
        else
        {
            db.CartItems.Add(new CartItem
            {
                CartId = cart.Id,
                ProductId = request.ProductId,
                Quantity = request.Quantity,
                IsSelected = true,
                UnitPriceSnapshot = p.Price,
                VariantLabel = p.AttributesSummary
            });
        }

        cart.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return Result<object>.Ok(new { ok = true });
    }

    public async Task<Result<object>> PatchCartItemAsync(Guid userId, int itemId, PatchCartItemRequest request,
        CancellationToken cancellationToken = default)
    {
        var cart = await db.ShoppingCarts.FirstAsync(c => c.UserId == userId, cancellationToken);
        var item = await db.CartItems.FirstOrDefaultAsync(i => i.Id == itemId && i.CartId == cart.Id, cancellationToken);
        if (item is null)
            return Result<object>.Fail("Topilmadi.", 404);
        if (request.Quantity is { } q)
        {
            if (q < 1)
                return Result<object>.Fail("Miqdor kamida 1 bo‘lishi kerak.");
            item.Quantity = q;
        }
        if (request.IsSelected is { } s)
            item.IsSelected = s;
        cart.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return Result<object>.Ok(new { ok = true });
    }

    public async Task<Result<object?>> RemoveCartItemAsync(Guid userId, int itemId,
        CancellationToken cancellationToken = default)
    {
        var cart = await db.ShoppingCarts.FirstAsync(c => c.UserId == userId, cancellationToken);
        var item = await db.CartItems.FirstOrDefaultAsync(i => i.Id == itemId && i.CartId == cart.Id, cancellationToken);
        if (item is null)
            return Result<object?>.Fail("Topilmadi.", 404);
        db.CartItems.Remove(item);
        await db.SaveChangesAsync(cancellationToken);
        return Result<object?>.Ok(null, 204);
    }

    public async Task<Result<object>> ApplyPromoAsync(Guid userId, ApplyPromoRequest request,
        CancellationToken cancellationToken = default)
    {
        var cart = await GetOrCreateCartAsync(db, userId, cancellationToken);
        var coupon = await db.Coupons.FirstOrDefaultAsync(c => c.Code == request.Code.Trim().ToUpperInvariant(),
            cancellationToken);
        if (coupon is null)
            return Result<object>.Fail("Promo noto'g'ri.");
        if (coupon.ValidUntilUtc is { } v && v < DateTime.UtcNow)
            return Result<object>.Fail("Muddati tugagan.");
        if (coupon.MaxUses is { } m && coupon.UsedCount >= m)
            return Result<object>.Fail("Limit tugagan.");
        cart.AppliedPromoCode = coupon.Code;
        cart.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return Result<object>.Ok(new { applied = coupon.Code });
    }

    public async Task<Result<IReadOnlyList<object>>> GetShippingOptionsAsync(CancellationToken cancellationToken = default)
    {
        var list = await db.ShippingOptions.AsNoTracking()
            .OrderBy(s => s.SortOrder)
            .Select(s => new { s.Id, s.Code, s.Name, s.Description, s.Price })
            .ToListAsync(cancellationToken);
        return Result<IReadOnlyList<object>>.Ok(list);
    }

    public async Task<Result<IReadOnlyList<object>>> ListAddressesAsync(Guid userId,
        CancellationToken cancellationToken = default)
    {
        var list = await db.UserAddresses.AsNoTracking()
            .Where(a => a.UserId == userId)
            .Include(a => a.District)
            .Select(a => new
            {
                a.Id,
                a.Label,
                district = a.District.Name,
                city = a.District.City,
                a.Street,
                a.HouseNumber,
                a.Apartment,
                a.Comment,
                a.Latitude,
                a.Longitude,
                a.IsPrimary
            })
            .ToListAsync(cancellationToken);
        return Result<IReadOnlyList<object>>.Ok(list);
    }

    public async Task<Result<int>> CreateAddressAsync(Guid userId, AddressRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.IsPrimary)
            await ClearPrimaryAsync(db, userId, cancellationToken);
        var a = new UserAddress
        {
            UserId = userId,
            Label = request.Label,
            DistrictId = request.DistrictId,
            Street = request.Street,
            HouseNumber = request.HouseNumber,
            Apartment = request.Apartment,
            Comment = request.Comment,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            IsPrimary = request.IsPrimary
        };
        db.UserAddresses.Add(a);
        await db.SaveChangesAsync(cancellationToken);
        return Result<int>.Ok(a.Id, 201);
    }

    public async Task<Result<object>> UpdateAddressAsync(Guid userId, int id, AddressRequest request,
        CancellationToken cancellationToken = default)
    {
        var a = await db.UserAddresses.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, cancellationToken);
        if (a is null)
            return Result<object>.Fail("Topilmadi.", 404);
        if (request.IsPrimary)
            await ClearPrimaryAsync(db, userId, cancellationToken);
        a.Label = request.Label;
        a.DistrictId = request.DistrictId;
        a.Street = request.Street;
        a.HouseNumber = request.HouseNumber;
        a.Apartment = request.Apartment;
        a.Comment = request.Comment;
        a.Latitude = request.Latitude;
        a.Longitude = request.Longitude;
        a.IsPrimary = request.IsPrimary;
        await db.SaveChangesAsync(cancellationToken);
        return Result<object>.Ok(new { ok = true });
    }

    public async Task<Result<object?>> DeleteAddressAsync(Guid userId, int id, CancellationToken cancellationToken = default)
    {
        var a = await db.UserAddresses.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, cancellationToken);
        if (a is null)
            return Result<object?>.Fail("Topilmadi.", 404);
        db.UserAddresses.Remove(a);
        await db.SaveChangesAsync(cancellationToken);
        return Result<object?>.Ok(null, 204);
    }

    public async Task<Result<object>> SetPrimaryAddressAsync(Guid userId, int id, CancellationToken cancellationToken = default)
    {
        var a = await db.UserAddresses.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, cancellationToken);
        if (a is null)
            return Result<object>.Fail("Topilmadi.", 404);
        await ClearPrimaryAsync(db, userId, cancellationToken);
        a.IsPrimary = true;
        await db.SaveChangesAsync(cancellationToken);
        return Result<object>.Ok(new { ok = true });
    }

    public async Task<Result<IReadOnlyList<object>>> GetDistrictsAsync(string? city,
        CancellationToken cancellationToken = default)
    {
        var q = db.Districts.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(city))
            q = q.Where(d => d.City == city);
        var list = await q.OrderBy(d => d.Name).Select(d => new { d.Id, d.City, d.Name }).ToListAsync(cancellationToken);
        return Result<IReadOnlyList<object>>.Ok(list);
    }

    public async Task<Result<IReadOnlyList<object>>> ListCardsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var list = await db.SavedPaymentMethods.AsNoTracking()
            .Where(c => c.UserId == userId)
            .Select(c => new
            {
                c.Id,
                c.MaskedPan,
                c.ExpiryMonth,
                c.ExpiryYear,
                c.CardholderName,
                c.Brand,
                c.IsPrimary
            })
            .ToListAsync(cancellationToken);
        return Result<IReadOnlyList<object>>.Ok(list);
    }

    public async Task<Result<Guid>> AddCardAsync(Guid userId, AddCardRequest request,
        CancellationToken cancellationToken = default)
    {
        var digits = new string(request.CardNumber.Where(char.IsDigit).ToArray());
        if (digits.Length is < 13 or > 19 || !CardValidation.LuhnCheck(digits))
            return Result<Guid>.Fail("Karta raqami noto'g'ri.");
        var month = request.ExpiryMonth;
        var year = request.ExpiryYear;
        if (month is < 1 or > 12)
            return Result<Guid>.Fail("Oy noto'g'ri.");
        var last2 = year % 100;
        var exp = new DateTime(2000 + last2, month, 1).AddMonths(1).AddDays(-1);
        if (exp < DateTime.UtcNow.Date)
            return Result<Guid>.Fail("Muddati o'tgan.");
        if (request.Cvv is < 100 or > 9999)
            return Result<Guid>.Fail("CVV noto'g'ri.");

        if (request.SetPrimary)
            await db.SavedPaymentMethods.Where(c => c.UserId == userId && c.IsPrimary)
                .ExecuteUpdateAsync(s => s.SetProperty(c => c.IsPrimary, false), cancellationToken);

        var last4 = digits[^4..];
        var card = new SavedPaymentMethod
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            MaskedPan = "**** **** **** " + last4,
            ExpiryMonth = month,
            ExpiryYear = year,
            CardholderName = request.CardholderName.Trim(),
            Brand = CardValidation.DetectBrand(digits),
            IsPrimary = request.SetPrimary,
            PaymentToken = $"tok_{Guid.NewGuid():N}"
        };
        db.SavedPaymentMethods.Add(card);
        await db.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Ok(card.Id, 201);
    }

    public async Task<Result<object?>> DeleteCardAsync(Guid userId, Guid cardId,
        CancellationToken cancellationToken = default)
    {
        var c = await db.SavedPaymentMethods.FirstOrDefaultAsync(x => x.Id == cardId && x.UserId == userId,
            cancellationToken);
        if (c is null)
            return Result<object?>.Fail("Topilmadi.", 404);
        db.SavedPaymentMethods.Remove(c);
        await db.SaveChangesAsync(cancellationToken);
        return Result<object?>.Ok(null, 204);
    }

    public async Task<Result<object>> SetPrimaryCardAsync(Guid userId, Guid cardId,
        CancellationToken cancellationToken = default)
    {
        var c = await db.SavedPaymentMethods.FirstOrDefaultAsync(x => x.Id == cardId && x.UserId == userId,
            cancellationToken);
        if (c is null)
            return Result<object>.Fail("Topilmadi.", 404);
        await db.SavedPaymentMethods.Where(x => x.UserId == userId)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsPrimary, false), cancellationToken);
        c.IsPrimary = true;
        await db.SaveChangesAsync(cancellationToken);
        return Result<object>.Ok(new { ok = true });
    }

    public async Task<Result<object>> ConfirmCheckoutAsync(Guid userId, CheckoutRequest request,
        CancellationToken cancellationToken = default)
    {
        var cart = await db.ShoppingCarts.Include(c => c.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);
        if (cart is null || cart.Items.Count == 0)
            return Result<object>.Fail("Savat bo'sh.");

        var lines = cart.Items.Where(i => i.IsSelected).ToList();
        if (lines.Count == 0)
            return Result<object>.Fail("Tanlangan mahsulot yo'q.");

        var user = await db.Users.FirstAsync(u => u.Id == userId, cancellationToken);
        var addr = await db.UserAddresses.FirstOrDefaultAsync(a => a.Id == request.AddressId && a.UserId == userId,
            cancellationToken);
        if (addr is null)
            return Result<object>.Fail("Manzil topilmadi.");
        var ship = await db.ShippingOptions.FirstOrDefaultAsync(s => s.Id == request.ShippingOptionId, cancellationToken);
        if (ship is null)
            return Result<object>.Fail("Yetkazib berish usuli topilmadi.");

        decimal subtotal = 0;
        foreach (var line in lines)
        {
            if (line.Product.Stock < line.Quantity)
                return Result<object>.Fail($"{line.Product.Name} uchun zaxira yetmaydi.");
            subtotal += line.UnitPriceSnapshot * line.Quantity;
        }

        decimal discount = 0;
        if (!string.IsNullOrEmpty(cart.AppliedPromoCode))
        {
            var coupon = await db.Coupons.FirstOrDefaultAsync(c => c.Code == cart.AppliedPromoCode, cancellationToken);
            if (coupon is not null)
            {
                if (coupon.ValidUntilUtc is { } vu && vu < DateTime.UtcNow)
                    return Result<object>.Fail("Promo muddati tugagan.");
                if (coupon.MaxUses is { } max && coupon.UsedCount >= max)
                    return Result<object>.Fail("Promo limiti tugagan.");

                if (coupon.DiscountPercent is { } dp)
                    discount += subtotal * (dp / 100m);
                if (coupon.DiscountAmount is { } da)
                    discount += da;
            }
        }

        var shippingFee = ship.Price;
        var total = subtotal - discount + shippingFee;
        if (total < 0)
            total = 0;

        await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);

        var appliedPromo = cart.AppliedPromoCode;

        var order = new Order
        {
            UserId = userId,
            CustomerName = user.FullName,
            Email = user.Email!,
            Phone = user.PhoneNumber,
            ShippingAddressId = addr.Id,
            ShippingOptionId = ship.Id,
            SavedPaymentMethodId = request.SavedPaymentMethodId,
            PaymentMode = request.PaymentMode,
            PromoCodeApplied = appliedPromo,
            Subtotal = subtotal,
            ShippingFee = shippingFee,
            Discount = discount,
            Total = total,
            Status = OrderStatus.Pending,
            CreatedAtUtc = DateTime.UtcNow
        };

        foreach (var line in lines)
        {
            var p = line.Product;
            p.Stock -= line.Quantity;
            p.SalesThisMonth += line.Quantity;
            order.Items.Add(new OrderItem
            {
                ProductId = p.Id,
                Quantity = line.Quantity,
                UnitPrice = line.UnitPriceSnapshot,
                VariantDescription = line.VariantLabel
            });
        }

        db.Orders.Add(order);

        db.CartItems.RemoveRange(lines);
        cart.AppliedPromoCode = null;
        cart.UpdatedAtUtc = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        if (!string.IsNullOrEmpty(appliedPromo))
        {
            var couponRow = await db.Coupons.FirstOrDefaultAsync(c => c.Code == appliedPromo, cancellationToken);
            if (couponRow is not null)
                couponRow.UsedCount += 1;
        }

        var cashback = Math.Floor(subtotal * RewardConstants.PurchaseCashbackPercent);
        if (cashback > 0)
        {
            var w = await db.Wallets.FirstAsync(x => x.UserId == userId, cancellationToken);
            w.CoinBalance += cashback;
            db.CoinTransactions.Add(new CoinTransaction
            {
                UserId = userId,
                Amount = cashback,
                Type = CoinTransactionType.PurchaseCashback,
                Description = "Xarid uchun keshbek",
                Reference = order.Id.ToString(),
                CreatedAtUtc = DateTime.UtcNow
            });
        }

        await db.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);

        return Result<object>.Ok(new { orderId = order.Id, total });
    }

    private static async Task<ShoppingCart> GetOrCreateCartAsync(AppDbContext dbContext, Guid userId,
        CancellationToken cancellationToken)
    {
        var c = await dbContext.ShoppingCarts.FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
        if (c is not null)
            return c;
        c = new ShoppingCart { UserId = userId, UpdatedAtUtc = DateTime.UtcNow };
        dbContext.ShoppingCarts.Add(c);
        await dbContext.SaveChangesAsync(cancellationToken);
        return c;
    }

    private static async Task ClearPrimaryAsync(AppDbContext dbContext, Guid uid, CancellationToken cancellationToken)
    {
        await dbContext.UserAddresses.Where(x => x.UserId == uid && x.IsPrimary)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsPrimary, false), cancellationToken);
    }
}
