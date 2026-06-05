using FlowMarket.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FlowMarket.Application.Abstractions;

public interface IAppDbContext
{
    DbSet<Product> Products { get; }
    DbSet<Order> Orders { get; }
    DbSet<OrderItem> OrderItems { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
