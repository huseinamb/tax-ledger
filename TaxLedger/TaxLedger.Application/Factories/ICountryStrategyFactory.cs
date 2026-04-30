using TaxLedger.Domain.TaxEngine;
using TaxLedger.Domain.Reporting;

namespace TaxLedger.Application.Factories;

public interface ICountryStrategyFactory
{
    IEnumerable<string> GetSupportedCountries();
    ITaxCalculationStrategy GetStrategy(string country);
}