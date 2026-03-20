using TaxLedger.Domain.Transactions;

public interface IExchangeTransactionParser<TRawRow>
{
    IEnumerable<CanonicalTransaction> Parse(IEnumerable<TRawRow> rows);
}