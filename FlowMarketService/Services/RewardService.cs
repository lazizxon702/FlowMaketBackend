using FlowMarketService.Common;
using FlowMarketService.Data;
using FlowMarketService.Models;
using FlowMarketService.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FlowMarketService.Services;

public class RewardService(AppDbContext db) : IRewardService
{
    public async Task<Result<object>> GetBalanceAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var w = await db.Wallets.AsNoTracking().FirstAsync(x => x.UserId == userId, cancellationToken);
        return Result<object>.Ok(new { coins = w.CoinBalance, creditUzs = w.CreditUzs });
    }

    public async Task<Result<object>> GetHistoryAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var rows = await db.CoinTransactions.AsNoTracking()
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAtUtc)
            .Take(100)
            .Select(t => new
            {
                t.Id,
                t.Amount,
                type = t.Type.ToString(),
                t.Description,
                t.CreatedAtUtc
            })
            .ToListAsync(cancellationToken);
        return Result<object>.Ok(rows);
    }

    public async Task<Result<object>> LuckySpinAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);
        var w = await db.Wallets.FirstAsync(x => x.UserId == userId, cancellationToken);
        if (w.CoinBalance < RewardConstants.LuckySpinCostCoins)
            return Result<object>.Fail("Spin uchun 100 tangadan kam.");

        w.CoinBalance -= RewardConstants.LuckySpinCostCoins;
        db.CoinTransactions.Add(new CoinTransaction
        {
            UserId = userId,
            Amount = -RewardConstants.LuckySpinCostCoins,
            Type = CoinTransactionType.SpinCost,
            Description = "Lucky Spin",
            CreatedAtUtc = DateTime.UtcNow
        });

        var prize = RewardConstants.SpinWheelAmounts[Random.Shared.Next(RewardConstants.SpinWheelAmounts.Length)];
        w.CoinBalance += prize;
        db.CoinTransactions.Add(new CoinTransaction
        {
            UserId = userId,
            Amount = prize,
            Type = CoinTransactionType.SpinWin,
            Description = "Lucky Spin yutug'i",
            CreatedAtUtc = DateTime.UtcNow
        });
        await db.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);
        return Result<object>.Ok(new { wonCoins = prize, newBalance = w.CoinBalance });
    }

    public async Task<Result<object>> ConvertAsync(Guid userId, decimal coins, CancellationToken cancellationToken = default)
    {
        if (coins <= 0)
            return Result<object>.Fail("Noto'g'ri miqdor.");

        await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);
        var w = await db.Wallets.FirstAsync(x => x.UserId == userId, cancellationToken);
        if (w.CoinBalance < coins)
            return Result<object>.Fail("Balans yetarli emas.");

        var uzs = coins * RewardConstants.CoinsPerUzsCredit;
        w.CoinBalance -= coins;
        w.CreditUzs += uzs;
        db.CoinTransactions.Add(new CoinTransaction
        {
            UserId = userId,
            Amount = -coins,
            Type = CoinTransactionType.ConvertCoinsToUzs,
            Description = $"UZS kreditga aylantirish ({uzs} UZS)",
            CreatedAtUtc = DateTime.UtcNow
        });
        await db.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);
        return Result<object>.Ok(new { coins = w.CoinBalance, creditUzs = w.CreditUzs });
    }

    public async Task<Result<object>> DailyCheckInAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var state = await db.UserTaskStates.FirstAsync(t =>
            t.UserId == userId && t.TaskType == EarnTaskType.DailyCheckIn, cancellationToken);
        var today = DateTime.UtcNow.Date;
        if (state.LastDailyClaimUtc is { } last && last.Date == today)
            return Result<object>.Fail("Bugun allaqachon olindi.");

        state.LastDailyClaimUtc = DateTime.UtcNow;
        var w = await db.Wallets.FirstAsync(x => x.UserId == userId, cancellationToken);
        w.CoinBalance += RewardConstants.DailyCheckInCoins;
        db.CoinTransactions.Add(new CoinTransaction
        {
            UserId = userId,
            Amount = RewardConstants.DailyCheckInCoins,
            Type = CoinTransactionType.DailyCheckIn,
            Description = "Kunlik kirish mukofoti",
            CreatedAtUtc = DateTime.UtcNow
        });
        await db.SaveChangesAsync(cancellationToken);
        return Result<object>.Ok(new { claimed = true, coins = w.CoinBalance });
    }

    public async Task<Result<object>> CompleteReviewAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var state = await db.UserTaskStates.FirstAsync(t =>
            t.UserId == userId && t.TaskType == EarnTaskType.WriteReview, cancellationToken);
        if (state.IsCompleted)
            return Result<object>.Fail("Allaqachon bajarilgan.");
        state.IsCompleted = true;
        state.CompletedAtUtc = DateTime.UtcNow;
        var w = await db.Wallets.FirstAsync(x => x.UserId == userId, cancellationToken);
        w.CoinBalance += RewardConstants.ReviewRewardCoins;
        db.CoinTransactions.Add(new CoinTransaction
        {
            UserId = userId,
            Amount = RewardConstants.ReviewRewardCoins,
            Type = CoinTransactionType.ReviewReward,
            Description = "Sharh uchun mukofot",
            CreatedAtUtc = DateTime.UtcNow
        });
        await db.SaveChangesAsync(cancellationToken);
        return Result<object>.Ok(new { completed = true });
    }

    public async Task<Result<object>> CompleteKycAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await db.Users.FirstAsync(u => u.Id == userId, cancellationToken);
        var state = await db.UserTaskStates.FirstAsync(t =>
            t.UserId == userId && t.TaskType == EarnTaskType.VerifyIdentity, cancellationToken);
        if (state.IsCompleted || user.IdentityVerified)
            return Result<object>.Fail("Allaqachon tasdiqlangan.");
        user.IdentityVerified = true;
        state.IsCompleted = true;
        state.CompletedAtUtc = DateTime.UtcNow;
        var w = await db.Wallets.FirstAsync(x => x.UserId == userId, cancellationToken);
        w.CoinBalance += RewardConstants.KycRewardCoins;
        db.CoinTransactions.Add(new CoinTransaction
        {
            UserId = userId,
            Amount = RewardConstants.KycRewardCoins,
            Type = CoinTransactionType.KycReward,
            Description = "Identifikatsiya mukofoti",
            CreatedAtUtc = DateTime.UtcNow
        });
        await db.SaveChangesAsync(cancellationToken);
        return Result<object>.Ok(new { completed = true });
    }

    public async Task<Result<object>> GetTaskStatesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var rows = await db.UserTaskStates.AsNoTracking()
            .Where(t => t.UserId == userId)
            .Select(t => new
            {
                task = t.TaskType.ToString(),
                t.IsCompleted,
                t.CompletedAtUtc,
                t.LastDailyClaimUtc
            })
            .ToListAsync(cancellationToken);
        return Result<object>.Ok(rows);
    }
}
