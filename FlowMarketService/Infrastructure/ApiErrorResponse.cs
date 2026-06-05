namespace FlowMarketService.Infrastructure;

/// <summary>
/// Barcha xatolik javoblari uchun bir xil shakl — frontendda xatolarni ko‘rsatish va supportga traceId berish oson.
/// </summary>
public sealed record ApiErrorResponse(string Error, string TraceId, string Code);
