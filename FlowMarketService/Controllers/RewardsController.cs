using FlowMarketService.Contracts;
using FlowMarketService.Infrastructure;
using FlowMarketService.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowMarketService.Controllers;

[ApiController]
[Authorize]
[Route("api/rewards")]
public class RewardsController(IRewardService rewards) : ControllerBase
{
    [HttpGet("balance")]
    public async Task<IActionResult> GetBalance(CancellationToken cancellationToken)
    {
        var uid = HttpContext.GetUserId()!.Value;
        var r = await rewards.GetBalanceAsync(uid, cancellationToken);
        return this.ToActionResult(r);
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory(CancellationToken cancellationToken)
    {
        var uid = HttpContext.GetUserId()!.Value;
        var r = await rewards.GetHistoryAsync(uid, cancellationToken);
        return this.ToActionResult(r);
    }

    [HttpPost("spin")]
    public async Task<IActionResult> LuckySpin(CancellationToken cancellationToken)
    {
        var uid = HttpContext.GetUserId()!.Value;
        var r = await rewards.LuckySpinAsync(uid, cancellationToken);
        return this.ToActionResult(r);
    }

    [HttpPost("convert")]
    public async Task<IActionResult> Convert([FromBody] ConvertCoinsRequest body, CancellationToken cancellationToken)
    {
        var uid = HttpContext.GetUserId()!.Value;
        var r = await rewards.ConvertAsync(uid, body.Coins, cancellationToken);
        return this.ToActionResult(r);
    }

    [HttpPost("tasks/daily-checkin")]
    public async Task<IActionResult> DailyCheckIn(CancellationToken cancellationToken)
    {
        var uid = HttpContext.GetUserId()!.Value;
        var r = await rewards.DailyCheckInAsync(uid, cancellationToken);
        return this.ToActionResult(r);
    }

    [HttpPost("tasks/review")]
    public async Task<IActionResult> CompleteReview(CancellationToken cancellationToken)
    {
        var uid = HttpContext.GetUserId()!.Value;
        var r = await rewards.CompleteReviewAsync(uid, cancellationToken);
        return this.ToActionResult(r);
    }

    [HttpPost("tasks/kyc")]
    public async Task<IActionResult> CompleteKyc(CancellationToken cancellationToken)
    {
        var uid = HttpContext.GetUserId()!.Value;
        var r = await rewards.CompleteKycAsync(uid, cancellationToken);
        return this.ToActionResult(r);
    }

    [HttpGet("tasks")]
    public async Task<IActionResult> GetTaskStates(CancellationToken cancellationToken)
    {
        var uid = HttpContext.GetUserId()!.Value;
        var r = await rewards.GetTaskStatesAsync(uid, cancellationToken);
        return this.ToActionResult(r);
    }
}
