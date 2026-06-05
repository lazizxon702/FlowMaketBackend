namespace FlowMarketService.Services;

public static class RewardConstants
{
    public const decimal LuckySpinCostCoins = 100;
    public const decimal CoinsPerUzsCredit = 1m;
    public const decimal ReferralBonusCoins = 2000;
    public const decimal ReviewRewardCoins = 50;
    public const decimal KycRewardCoins = 500;
    public const decimal DailyCheckInCoins = 10;
    public const decimal PurchaseCashbackPercent = 0.05m;

    public static readonly int[] SpinWheelAmounts = [250, 5000, 100, 2000, 1000, 500];
}
