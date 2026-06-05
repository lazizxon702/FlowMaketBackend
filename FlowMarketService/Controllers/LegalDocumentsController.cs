using FlowMarketService.Infrastructure;
using FlowMarketService.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowMarketService.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/legal")]
public class LegalDocumentsController(ILegalDocumentService legal) : ControllerBase
{
    [HttpGet("documents")]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var r = await legal.ListAsync(cancellationToken);
        return this.ToActionResult(r);
    }
}
