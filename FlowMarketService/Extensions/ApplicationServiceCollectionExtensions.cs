using FlowMarketService.Services;
using FlowMarketService.Services.Interfaces;

namespace FlowMarketService.Extensions;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICatalogService, CatalogService>();
        services.AddScoped<IRewardService, RewardService>();
        services.AddScoped<ICommerceService, CommerceService>();
        services.AddScoped<IProfileService, ProfileService>();
        services.AddScoped<IMerchantService, MerchantService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<ILegalDocumentService, LegalDocumentService>();
        return services;
    }
}
