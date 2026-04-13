using TaxLedger.Domain.Transactions;

namespace TaxLedger.Application.Factories;

public interface IExchangeParserFactory
{
    IEnumerable<string> GetSupportedExchanges();
    IEnumerable<CanonicalTransaction> Parse(string exchange, Stream fileStream);

}