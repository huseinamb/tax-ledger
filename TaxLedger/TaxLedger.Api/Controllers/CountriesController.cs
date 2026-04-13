using Microsoft.AspNetCore.Mvc;
using TaxLedger.Application.Factories;

namespace TaxLedger.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CountriesController : ControllerBase
{
    private readonly ICountryStrategyFactory _factory;

    public CountriesController(ICountryStrategyFactory factory)
    {
        _factory = factory;
    }

    /// <summary>
    /// Returns the list of supported countries.
    /// </summary>
    [HttpGet]
    public IActionResult GetSupportedCountries()
    {
        return Ok(_factory.GetSupportedCountries());
    }
}