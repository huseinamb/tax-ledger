using Microsoft.AspNetCore.Mvc;
using TaxLedger.Application;
using TaxLedger.Application.Factories;
using TaxLedger.Application.Pricing;

namespace TaxLedger.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportController : ControllerBase
{
    private readonly IExchangeParserFactory _exchangeFactory;
    private readonly ICountryStrategyFactory _countryFactory;
    private readonly IPriceEnrichmentService _enrichmentService;

    public ReportController(
        IExchangeParserFactory exchangeFactory,
        ICountryStrategyFactory countryFactory,
        IPriceEnrichmentService enrichmentService)
    {
        _exchangeFactory = exchangeFactory;
        _countryFactory = countryFactory;
        _enrichmentService = enrichmentService;
    }

    /// <summary>
    /// Generates a tax report in JSON format.
    /// </summary>
    /// <param name="file">CSV export from your exchange</param>
    /// <param name="exchange">Exchange name (e.g. Binance)</param>
    /// <param name="country">Country name (e.g. Sweden)</param>
    /// <param name="year">Tax year — defaults to latest year found in the data</param>
    [HttpPost("json")]
    public async Task<IActionResult> GenerateJsonReport(
        IFormFile file,
        [FromQuery] string exchange,
        [FromQuery] string country,
        [FromQuery] int? year)
    {
        // ── Validate exchange ──────────────────────────────────────────────────
        var supportedExchanges = _exchangeFactory.GetSupportedExchanges();
        if (!supportedExchanges.Any(e => e.Equals(exchange, StringComparison.OrdinalIgnoreCase)))
            return BadRequest($"Exchange '{exchange}' is not supported. " +
                              $"Supported: {string.Join(", ", supportedExchanges)}");

        // ── Validate country ───────────────────────────────────────────────────
        var supportedCountries = _countryFactory.GetSupportedCountries();
        if (!supportedCountries.Any(c => c.Equals(country, StringComparison.OrdinalIgnoreCase)))
            return BadRequest($"Country '{country}' is not supported. " +
                              $"Supported: {string.Join(", ", supportedCountries)}");

        // ── Validate file ──────────────────────────────────────────────────────
        if (file == null || file.Length == 0)
            return BadRequest("Please upload a valid CSV file.");

        // ── Parse ──────────────────────────────────────────────────────────────
        List<TaxLedger.Domain.Transactions.CanonicalTransaction> transactions;
        try
        {
            using var stream = file.OpenReadStream();
            transactions = _exchangeFactory.Parse(exchange, stream).ToList();
        }
        catch (Exception ex)
        {
            return BadRequest($"Failed to parse CSV: {ex.Message}");
        }

        if (!transactions.Any())
            return BadRequest("No transactions found in the uploaded file.");

        // ── Determine tax year ─────────────────────────────────────────────────
        var targetYear = year ?? transactions.Max(t => t.Timestamp.Year);

        // ── Validate year ──────────────────────────────────────────────────────
        if (targetYear < 2009)
            return BadRequest("Year cannot be earlier than 2009 (Bitcoin launch year).");

        if (targetYear > DateTime.UtcNow.Year)
            return BadRequest("Year cannot be in the future.");

        // ── Determine currency for country ─────────────────────────────────────
        var currencyCode = country.ToLower() switch
        {
            "sweden" => "SEK",
            _ => "USD"
        };

        // ── Enrich ─────────────────────────────────────────────────────────────
        List<TaxLedger.Domain.Transactions.CanonicalTransaction> enriched;
        try
        {
            enriched = (await _enrichmentService.EnrichAsync(transactions, currencyCode)).ToList();
        }
        catch (Exception ex)
        {
            return StatusCode(502, $"Failed to fetch market prices: {ex.Message}");
        }

        // ── Calculate ──────────────────────────────────────────────────────────
        var strategy = _countryFactory.GetStrategy(country);
        var reporter = _countryFactory.GetReporter(country);
        var taxService = new TaxService(strategy, reporter);
        var results = taxService.CalculateTax(enriched, targetYear).ToList();

        // ── Summarize ──────────────────────────────────────────────────────────────
        var summary = TaxReportSummarizer.Summarize(results);

        // ── Build response ─────────────────────────────────────────────────────────
        var response = new
        {
            Exchange = exchange,
            Country = country,
            TaxYear = targetYear,
            Currency = currencyCode,
            TaxableEvents = results.Count,
            TotalSalePrice = results.Sum(r => r.SalePrice),
            TotalCostBasis = results.Sum(r => r.PurchasePrice),
            TotalGainLoss = results.Sum(r => r.GainLoss),
            Summary = summary.Select(s => new
            {
                s.Asset,
                s.TotalSalePrice,
                s.TotalCostBasis,
                s.TotalGain,
                s.TotalLoss
            }),
            Transactions = results.Select(r => new
            {
                r.Asset,
                Timestamp = r.OriginTransaction.Timestamp,
                SalePrice = r.SalePrice,
                CostBasis = r.PurchasePrice,
                GainLoss = r.GainLoss
            })
        };

        return Ok(response);
    }
}