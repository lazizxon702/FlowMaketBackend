namespace FlowMarketService.Models;

public enum CoinTransactionType
{
    SpinCost = 0,
    SpinWin = 1,
    PurchaseCashback = 2,
    ReferralBonus = 3,
    ReviewReward = 4,
    KycReward = 5,
    DailyCheckIn = 6,
    ConvertCoinsToUzs = 7,
    Redeem = 8,
    AdminAdjust = 9,
    OrderDebit = 10
}

public enum EarnTaskType
{
    DailyCheckIn = 0,
    WriteReview = 1,
    VerifyIdentity = 2
}

public enum MerchantApplicationStatus
{
    Pending = 0,
    Verified = 1,
    Rejected = 2
}

public enum DocumentReviewStatus
{
    Pending = 0,
    Verified = 1,
    Missing = 2
}

public enum ContractStatus
{
    PendingSignature = 0,
    Active = 1,
    Archived = 2,
    Expired = 3,
    ActionRequired = 4
}

public enum OrderPaymentKind
{
    Card = 0,
    CashOnDelivery = 1,
    Wallet = 2
}

public enum SupportTicketStatus
{
    Open = 0,
    InProgress = 1,
    Closed = 2
}

public enum ActivityType
{
    NewOrder = 0,
    ListingApproved = 1,
    StockAlert = 2,
    NewReview = 3
}

public enum BusinessType
{
    Electronics = 0,
    Furniture = 1,
    Groceries = 2,
    Fashion = 3,
    Textiles = 4,
    Other = 99
}
