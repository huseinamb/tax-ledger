namespace TaxLedger.ExchangeAdapters.Binance
{
    public sealed class BinanceRawRow
    {
        public required DateTime Time { get; init; }
        public required string Account { get; init; }
        public required string Operation { get; init; }
        public required string Coin { get; init; }
        public required double Change { get; init; }
    }
}