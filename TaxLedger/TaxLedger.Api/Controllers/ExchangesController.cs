using Microsoft.AspNetCore.Mvc;
using TaxLedger.Application.Factories;

namespace TaxLedger.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExchangesController : ControllerBase
{
    private readonly IExchangeParserFactory _factory;

    public ExchangesController(IExchangeParserFactory factory)
    {
        _factory = factory;
    }

    /// <summary>
    /// Returns the list of supported exchanges.
    /// </summary>
    [HttpGet]
    public IActionResult GetSupportedExchanges()
    {
        return Ok(_factory.GetSupportedExchanges());
    }
}