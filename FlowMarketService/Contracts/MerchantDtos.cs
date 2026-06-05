using FlowMarketService.Models;

namespace FlowMarketService.Contracts;

public record MerchantApplyRequest(string ApplicantName, string BusinessName, BusinessType BusinessType, string? TaxId);
