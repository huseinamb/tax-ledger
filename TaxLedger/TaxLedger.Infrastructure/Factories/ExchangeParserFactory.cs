using System.Globalization;
using TaxLedger.Application.Factories;
using TaxLedger.Domain.Transactions;
using TaxLedger.ExchangeAdapters.Binance;



namespace TaxLedger.Infrastructure.Factories;

public sealed class ExchangeParserFactory : IExchangeParserFactory
{
    private static readonly List<string> SupportedExchanges = new() { "Binance" };

    public IEnumerable<string> GetSupportedExchanges() => SupportedExchanges;

    public IEnumerable<CanonicalTransaction> Parse(string exchange, Stream fileStream)
    {
        if (!SupportedExchanges.Any(e => e.Equals(exchange, StringComparison.OrdinalIgnoreCase)))
            throw new NotSupportedException(
                $"Exchange '{exchange}' is not supported. " +
                $"Supported: {string.Join(", ", SupportedExchanges)}");

        return exchange.ToLower() switch
        {
            "binance" => ParseBinance(fileStream),
            _ => throw new NotSupportedException($"Exchange '{exchange}' is not supported.")
        };
    }

    private static IEnumerable<CanonicalTransaction> ParseBinance(Stream fileStream)
    {
        var rows = BinanceCsvReader.ReadFromStream(fileStream);
        var parser = new BinanceTransactionParser();
        return parser.Parse(rows);
    }
}