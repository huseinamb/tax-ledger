using TaxLedger.Application.Factories;
using TaxLedger.Domain.Reporting;
using TaxLedger.Domain.TaxEngine;
using TaxLedger.Domain.TaxEngine.Strategies;
using TaxLedger.Domain.Reporting.Countries;

namespace TaxLedger.Infrastructure.Factories;

public sealed class CountryStrategyFactory : ICountryStrategyFactory
{
    private static readonly List<string> SupportedCountries = new() { "Sweden" };

    public IEnumerable<string> GetSupportedCountries() => SupportedCountries;

    public ITaxCalculationStrategy GetStrategy(string country) =>
        country.ToLower() switch
        {
            "sweden" => new SwedishAverageCostStrategy(),
            _ => throw new NotSupportedException($"Country '{country}' is not supported.")
        };

    public ITaxReportGenerator GetReporter(string country) =>
        country.ToLower() switch
        {
            "sweden" => new SwedishK4ReportGenerator(),
            _ => throw new NotSupportedException($"Country '{country}' is not supported.")
        };
}