using TaxLedger.Domain.Transactions;

namespace TaxLedger.Application.Parsing;

public interface IExchangeTransactionParser<TRawRow>
{
    IEnumerable<CanonicalTransaction> Parse(IEnumerable<TRawRow> rows);
}