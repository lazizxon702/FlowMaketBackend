using FlowMarketService.Common;
using FlowMarketService.Contracts;
using FlowMarketService.Data;
using FlowMarketService.Models;
using FlowMarketService.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FlowMarketService.Services;

public class CatalogService(AppDbContext db) : ICatalogService
{
    public async Task<Result<IReadOnlyList<CategoryResponse>>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        var list = await db.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new CategoryResponse(c.Id, c.Name, c.Description))
            .ToListAsync(cancellationToken);
        return Result<IReadOnlyList<CategoryResponse>>.Ok(list);
    }

    public async Task<Result<CategoryResponse>> GetCategoryAsync(int id, CancellationToken cancellationToken = default)
    {
        var c = await db.Categories.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (c is null)
            return Result<CategoryResponse>.Fail("Kategoriya topilmadi.", 404);
        return Result<CategoryResponse>.Ok(new CategoryResponse(c.Id, c.Name, c.Description));
    }

    public async Task<Result<CategoryResponse>> CreateCategoryAsync(CreateCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return Result<CategoryResponse>.Fail("Name majburiy.");

        var entity = new Category { Name = request.Name.Trim(), Description = request.Description?.Trim() };
        db.Categories.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        var response = new CategoryResponse(entity.Id, entity.Name, entity.Description);
        return Result<CategoryResponse>.Ok(response, 201);
    }

    public async Task<Result<CategoryResponse>> UpdateCategoryAsync(int id, UpdateCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return Result<CategoryResponse>.Fail("Name majburiy.");

        var entity = await db.Categories.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (entity is null)
            return Result<CategoryResponse>.Fail("Kategoriya topilmadi.", 404);

        entity.Name = request.Name.Trim();
        entity.Description = request.Description?.Trim();
        await db.SaveChangesAsync(cancellationToken);

        return Result<CategoryResponse>.Ok(new CategoryResponse(entity.Id, entity.Name, entity.Description));
    }

    public async Task<Result<object?>> DeleteCategoryAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await db.Categories.Include(c => c.Products).FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (entity is null)
            return Result<object?>.Fail("Kategoriya topilmadi.", 404);

        if (entity.Products.Count > 0)
            return Result<object?>.Fail(
                "Bu kategoriyada mahsulotlar bor — avval ularni o‘chiring yoki boshqa kategoriyaga ko‘chiring.", 409);

        db.Categories.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
        return Result<object?>.Ok(null, 204);
    }

    public async Task<Result<IReadOnlyList<ProductResponse>>> GetProductsAsync(int? categoryId, bool includeInactive,
        CancellationToken cancellationToken = default)
    {
        var q = db.Products.AsNoTracking().Include(p => p.Category).Include(p => p.Merchant).AsQueryable();
        if (categoryId is { } cid)
            q = q.Where(p => p.CategoryId == cid);
        if (!includeInactive)
            q = q.Where(p => p.IsActive);

        var list = await q
            .OrderBy(p => p.Name)
            .Select(p => new ProductResponse(
                p.Id,
                p.CategoryId,
                p.Category.Name,
                p.MerchantId,
                p.Merchant != null ? p.Merchant.Name : null,
                p.Name,
                p.Description,
                p.AttributesSummary,
                p.Price,
                p.Stock,
                p.ImageUrl,
                p.IsActive,
                p.IsTrending,
                p.CreatedAtUtc))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<ProductResponse>>.Ok(list);
    }

    public async Task<Result<ProductResponse>> GetProductAsync(int id, CancellationToken cancellationToken = default)
    {
        var p = await db.Products.AsNoTracking().Include(x => x.Category).Include(x => x.Merchant)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (p is null)
            return Result<ProductResponse>.Fail("Mahsulot topilmadi.", 404);

        var response = new ProductResponse(
            p.Id,
            p.CategoryId,
            p.Category.Name,
            p.MerchantId,
            p.Merchant?.Name,
            p.Name,
            p.Description,
            p.AttributesSummary,
            p.Price,
            p.Stock,
            p.ImageUrl,
            p.IsActive,
            p.IsTrending,
            p.CreatedAtUtc);

        return Result<ProductResponse>.Ok(response);
    }

    public async Task<Result<ProductResponse>> CreateProductAsync(CreateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return Result<ProductResponse>.Fail("Name majburiy.");
        if (request.Price <= 0)
            return Result<ProductResponse>.Fail("Narx 0 dan katta bo‘lishi kerak.");
        if (request.Stock < 0)
            return Result<ProductResponse>.Fail("Zaxira manfiy bo‘lmasligi kerak.");

        var categoryExists = await db.Categories.AnyAsync(c => c.Id == request.CategoryId, cancellationToken);
        if (!categoryExists)
            return Result<ProductResponse>.Fail("Kategoriya topilmadi.", 404);

        if (request.MerchantId is { } mid && !await db.Merchants.AnyAsync(m => m.Id == mid, cancellationToken))
            return Result<ProductResponse>.Fail("Merchant topilmadi.", 404);

        var entity = new Product
        {
            CategoryId = request.CategoryId,
            MerchantId = request.MerchantId,
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            AttributesSummary = request.AttributesSummary?.Trim(),
            Price = request.Price,
            Stock = request.Stock,
            ImageUrl = string.IsNullOrWhiteSpace(request.ImageUrl) ? null : request.ImageUrl.Trim(),
            IsActive = request.IsActive,
            IsTrending = request.IsTrending,
            CreatedAtUtc = DateTime.UtcNow
        };

        db.Products.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        var catName = await db.Categories.AsNoTracking().Where(c => c.Id == entity.CategoryId).Select(c => c.Name)
            .FirstAsync(cancellationToken);
        var mName = entity.MerchantId is { } m2
            ? await db.Merchants.AsNoTracking().Where(m => m.Id == m2).Select(m => m.Name).FirstOrDefaultAsync(cancellationToken)
            : null;

        var response = new ProductResponse(
            entity.Id,
            entity.CategoryId,
            catName,
            entity.MerchantId,
            mName,
            entity.Name,
            entity.Description,
            entity.AttributesSummary,
            entity.Price,
            entity.Stock,
            entity.ImageUrl,
            entity.IsActive,
            entity.IsTrending,
            entity.CreatedAtUtc);

        return Result<ProductResponse>.Ok(response, 201);
    }

    public async Task<Result<ProductResponse>> UpdateProductAsync(int id, UpdateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return Result<ProductResponse>.Fail("Name majburiy.");
        if (request.Price <= 0)
            return Result<ProductResponse>.Fail("Narx 0 dan katta bo‘lishi kerak.");
        if (request.Stock < 0)
            return Result<ProductResponse>.Fail("Zaxira manfiy bo‘lmasligi kerak.");

        var entity = await db.Products.Include(p => p.Category).Include(p => p.Merchant)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (entity is null)
            return Result<ProductResponse>.Fail("Mahsulot topilmadi.", 404);

        entity.Name = request.Name.Trim();
        entity.Description = request.Description?.Trim();
        entity.AttributesSummary = request.AttributesSummary?.Trim();
        entity.Price = request.Price;
        entity.Stock = request.Stock;
        entity.ImageUrl = string.IsNullOrWhiteSpace(request.ImageUrl) ? null : request.ImageUrl.Trim();
        entity.IsActive = request.IsActive;
        entity.IsTrending = request.IsTrending;
        await db.SaveChangesAsync(cancellationToken);

        var response = new ProductResponse(
            entity.Id,
            entity.CategoryId,
            entity.Category.Name,
            entity.MerchantId,
            entity.Merchant?.Name,
            entity.Name,
            entity.Description,
            entity.AttributesSummary,
            entity.Price,
            entity.Stock,
            entity.ImageUrl,
            entity.IsActive,
            entity.IsTrending,
            entity.CreatedAtUtc);

        return Result<ProductResponse>.Ok(response);
    }

    public async Task<Result<object?>> DeleteProductAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await db.Products.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (entity is null)
            return Result<object?>.Fail("Mahsulot topilmadi.", 404);

        var hasOrders = await db.OrderItems.AnyAsync(i => i.ProductId == id, cancellationToken);
        if (hasOrders)
            return Result<object?>.Fail("Bu mahsulot buyurtmalarda ishlatilgan — o‘rniga IsActive=false qiling.", 409);

        db.Products.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
        return Result<object?>.Ok(null, 204);
    }

    public async Task<Result<IReadOnlyList<OrderResponse>>> GetOrdersAsync(Guid? currentUserId, bool isAdmin,
        CancellationToken cancellationToken = default)
    {
        if (!isAdmin && currentUserId is null)
            return Result<IReadOnlyList<OrderResponse>>.Fail("Buyurtmalar ro‘yxati uchun avtorizatsiya kerak.", 401);

        var q = db.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .AsQueryable();

        if (!isAdmin)
            q = q.Where(o => o.UserId == currentUserId);

        var orders = await q
            .OrderByDescending(o => o.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        var result = orders.Select(MapOrder).ToList();
        return Result<IReadOnlyList<OrderResponse>>.Ok(result);
    }

    public async Task<Result<OrderResponse>> GetOrderAsync(int id, Guid? currentUserId, bool isAdmin,
        CancellationToken cancellationToken = default)
    {
        var o = await db.Orders
            .AsNoTracking()
            .Include(x => x.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (o is null)
            return Result<OrderResponse>.Fail("Buyurtma topilmadi.", 404);

        if (!isAdmin && (currentUserId is null || o.UserId != currentUserId))
            return Result<OrderResponse>.Fail("Buyurtma topilmadi.", 404);

        return Result<OrderResponse>.Ok(MapOrder(o));
    }

    public async Task<Result<OrderResponse>> CreateOrderAsync(CreateOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.CustomerName))
            return Result<OrderResponse>.Fail("CustomerName majburiy.");
        if (string.IsNullOrWhiteSpace(request.Email))
            return Result<OrderResponse>.Fail("Email majburiy.");
        if (request.Items is null || request.Items.Count == 0)
            return Result<OrderResponse>.Fail("Kamida bitta mahsulot qatorlari kerak.");

        foreach (var line in request.Items)
        {
            if (line.Quantity <= 0)
                return Result<OrderResponse>.Fail("Har bir qatorda quantity 0 dan katta bo‘lishi kerak.");
        }

        await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);

        var productIds = request.Items.Select(i => i.ProductId).Distinct().ToList();
        var products = await db.Products.Where(p => productIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id, cancellationToken);

        if (products.Count != productIds.Count)
            return Result<OrderResponse>.Fail("Ba’zi mahsulotlar topilmadi.", 404);

        decimal total = 0;
        var lines = new List<OrderItem>();

        foreach (var line in request.Items)
        {
            if (!products.TryGetValue(line.ProductId, out var p))
                return Result<OrderResponse>.Fail($"Mahsulot {line.ProductId} topilmadi.", 404);

            if (!p.IsActive)
                return Result<OrderResponse>.Fail($"Mahsulot {p.Name} hozir sotuvda emas.");
            if (p.Stock < line.Quantity)
                return Result<OrderResponse>.Fail($"\"{p.Name}\" uchun zaxira yetarli emas (qoldiq: {p.Stock}).");

            var unit = p.Price;
            total += unit * line.Quantity;
            lines.Add(new OrderItem
            {
                ProductId = p.Id,
                Quantity = line.Quantity,
                UnitPrice = unit,
                VariantDescription = p.AttributesSummary
            });

            p.Stock -= line.Quantity;
        }

        var order = new Order
        {
            CustomerName = request.CustomerName.Trim(),
            Email = request.Email.Trim(),
            Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim(),
            Status = OrderStatus.Pending,
            Subtotal = total,
            ShippingFee = 0,
            Discount = 0,
            Total = total,
            CreatedAtUtc = DateTime.UtcNow,
            Items = lines
        };

        db.Orders.Add(order);
        await db.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);

        var created = await db.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .FirstAsync(o => o.Id == order.Id, cancellationToken);

        var response = MapOrder(created);
        return Result<OrderResponse>.Ok(response, 201);
    }

    public async Task<Result<OrderResponse>> UpdateOrderStatusAsync(int id, OrderStatus status,
        CancellationToken cancellationToken = default)
    {
        var order = await db.Orders.Include(o => o.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
        if (order is null)
            return Result<OrderResponse>.Fail("Buyurtma topilmadi.", 404);

        if (!Enum.IsDefined(typeof(OrderStatus), status))
            return Result<OrderResponse>.Fail("Noto‘g‘ri status.");

        order.Status = status;
        await db.SaveChangesAsync(cancellationToken);

        return Result<OrderResponse>.Ok(MapOrder(order));
    }

    private static OrderResponse MapOrder(Order o)
    {
        var items = o.Items
            .Select(i => new OrderItemLineResponse(
                i.ProductId,
                i.Product?.Name ?? "(o‘chirilgan mahsulot)",
                i.Quantity,
                i.UnitPrice,
                i.VariantDescription))
            .ToList();

        return new OrderResponse(
            o.Id,
            o.UserId,
            o.CustomerName,
            o.Email,
            o.Phone,
            o.Status,
            o.Subtotal,
            o.ShippingFee,
            o.Discount,
            o.Total,
            o.PromoCodeApplied,
            o.PaymentMode,
            o.CreatedAtUtc,
            items);
    }
}
