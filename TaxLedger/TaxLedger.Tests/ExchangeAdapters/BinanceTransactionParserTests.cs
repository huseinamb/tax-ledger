using TaxLedger.Domain.Transactions;
using TaxLedger.ExchangeAdapters.Binance;
using Xunit;

namespace TaxLedger.Tests.ExchangeAdapters;

public class BinanceTransactionParserTests
{
    private readonly BinanceTransactionParser _parser = new();

    [Fact]
    public void Parse_DepositRow_CreatesDepositTransaction()
    {
        var rows = new List<BinanceRawRow>
        {
            new() { Time = new DateTime(2024, 3, 3, 0, 0, 0), Account = "Spot", Operation = "Deposit", Coin = "USD", Change = 10000.0 }
        };

        var result = _parser.Parse(rows).ToList();

        Assert.Single(result);
        Assert.Equal(TransactionType.Deposit, result[0].Type);
        Assert.Equal("USD", result[0].AssetIn);
        Assert.Equal(10000m, result[0].AmountIn);
    }

    [Fact]
    public void Parse_WithdrawRow_CreatesWithdrawalTransaction()
    {
        var rows = new List<BinanceRawRow>
        {
            new() { Time = new DateTime(2024, 3, 3, 12, 0, 0), Account = "Spot", Operation = "Withdraw", Coin = "USD", Change = -37120.0 }
        };

        var result = _parser.Parse(rows).ToList();

        Assert.Single(result);
        Assert.Equal(TransactionType.Withdrawal, result[0].Type);
        Assert.Equal("USD", result[0].AssetOut);
        Assert.Equal(37120m, result[0].AmountOut);
    }

    [Fact]
    public void Parse_FiatWithdrawRow_CreatesWithdrawalTransaction()
    {
        // Verify "Fiat Withdraw" is also recognised as a withdrawal
        var rows = new List<BinanceRawRow>
        {
            new() { Time = new DateTime(2024, 3, 3, 12, 0, 0), Account = "Spot", Operation = "Fiat Withdraw", Coin = "USD", Change = -1000.0 }
        };

        var result = _parser.Parse(rows).ToList();

        Assert.Single(result);
        Assert.Equal(TransactionType.Withdrawal, result[0].Type);
    }

    [Fact]
    public void Parse_TradeRows_CreatesSingleTradeTransaction()
    {
        // Buy ADA with USD + fee
        var rows = new List<BinanceRawRow>
        {
            new() { Time = new DateTime(2024, 3, 3, 4, 0, 0), Account = "Spot", Operation = "Transaction Related", Coin = "ADA",  Change =  2744.88253881 },
            new() { Time = new DateTime(2024, 3, 3, 4, 0, 0), Account = "Spot", Operation = "Transaction Related", Coin = "USD",  Change = -2000.0 },
            new() { Time = new DateTime(2024, 3, 3, 4, 0, 0), Account = "Spot", Operation = "Transaction Fee",     Coin = "ADA",  Change = -2.74763017 }
        };

        var result = _parser.Parse(rows).ToList();

        Assert.Single(result);
        var tx = result[0];
        Assert.Equal(TransactionType.Trade, tx.Type);
        Assert.Equal("ADA", tx.AssetIn);
        Assert.Equal("USD", tx.AssetOut);
        Assert.Equal("ADA", tx.FeeAsset);
        Assert.True(tx.FeeAmount > 0);
    }

    [Fact]
    public void Parse_NSplitTradeRows_SumsIntoSingleTradeTransaction()
    {
        // Same trade split into 2 identical sets of rows (n=2)
        // Matches the ETH/SOL trade at 2024-03-05T06:00:00 in the sample data
        var rows = new List<BinanceRawRow>
        {
            new() { Time = new DateTime(2024, 3, 5, 6, 0, 0), Account = "Spot", Operation = "Transaction Revenue", Coin = "ETH", Change =  0.31720595 },
            new() { Time = new DateTime(2024, 3, 5, 6, 0, 0), Account = "Spot", Operation = "Transaction Sold",    Coin = "SOL", Change = -9.00460681 },
            new() { Time = new DateTime(2024, 3, 5, 6, 0, 0), Account = "Spot", Operation = "Transaction Fee",     Coin = "ETH", Change = -0.00031752 },
            // duplicate set
            new() { Time = new DateTime(2024, 3, 5, 6, 0, 0), Account = "Spot", Operation = "Transaction Revenue", Coin = "ETH", Change =  0.31720595 },
            new() { Time = new DateTime(2024, 3, 5, 6, 0, 0), Account = "Spot", Operation = "Transaction Sold",    Coin = "SOL", Change = -9.00460681 },
            new() { Time = new DateTime(2024, 3, 5, 6, 0, 0), Account = "Spot", Operation = "Transaction Fee",     Coin = "ETH", Change = -0.00031752 },
        };

        var result = _parser.Parse(rows).ToList();

        // Should produce ONE trade with summed amounts
        Assert.Single(result);
        var tx = result[0];
        Assert.Equal(TransactionType.Trade, tx.Type);
        Assert.Equal("ETH", tx.AssetIn);
        Assert.Equal("SOL", tx.AssetOut);

        // Amounts should be summed: 0.31720595 * 2 = 0.6344119
        Assert.Equal(Convert.ToDecimal(0.31720595 * 2), tx.AmountIn);
        Assert.Equal(Convert.ToDecimal(9.00460681 * 2), tx.AmountOut);
    }

    [Fact]
    public void Parse_CsvFile_ParsesWithoutErrors()
    {
        // End-to-end: read the real sample CSV and parse it
        var csvPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "TestData", "1_simulated_binance.csv");

        var rows = BinanceCsvReader.ReadFromFile(csvPath);
        var result = _parser.Parse(rows).ToList();

        Assert.NotEmpty(result);
        // All transactions must have a valid timestamp
        Assert.All(result, tx => Assert.True(tx.Timestamp > DateTime.MinValue));
        // All fiat values are zero at this stage (enrichment not yet applied)
        Assert.All(result, tx => Assert.Equal(0m, tx.FiatValueAtTimestamp));
    }
}
