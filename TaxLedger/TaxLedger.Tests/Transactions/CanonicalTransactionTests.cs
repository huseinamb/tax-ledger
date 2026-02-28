using System;
using TaxLedger.Domain.Transactions;
using Xunit;

namespace TaxLedger.Tests.Transactions
{
    public class CanonicalTransactionTests
    {
        [Fact]
        public void Constructor_ValidTradeTransaction_ShouldCreateInstance()
        {
            var tx = new CanonicalTransaction(
                timestamp: DateTime.UtcNow,
                type: TransactionType.Trade,
                assetIn: "BTC",
                amountIn: 0.5m,
                assetOut: "SEK",
                amountOut: 250000m,
                feeAsset: "BTC",
                feeAmount: 0.001m,
                fiatValueAtTimestamp: 250000m,
                fiatValueAtTimestampCurrency: "SEK"
            );

            Assert.NotNull(tx);
            Assert.Equal(TransactionType.Trade, tx.Type);
            Assert.Equal("BTC", tx.AssetIn);
            Assert.Equal("SEK", tx.AssetOut);
        }

        [Fact]
        public void Constructor_DepositOnly_ShouldCreateInstance()
        {
            var tx = new CanonicalTransaction(
                timestamp: DateTime.UtcNow,
                type: TransactionType.Deposit,
                assetIn: "ETH",
                amountIn: 1.0m,
                assetOut: null,
                amountOut: 0m,
                feeAsset: null,
                feeAmount: 0m,
                fiatValueAtTimestamp: 20000m,
                fiatValueAtTimestampCurrency: "SEK"
            );

            Assert.NotNull(tx);
            Assert.Equal(TransactionType.Deposit, tx.Type);
            Assert.Equal("ETH", tx.AssetIn);
            Assert.Null(tx.AssetOut);
        }

        [Theory]
        [InlineData(-1.0, 0)]   // Negative AmountIn
        [InlineData(0, -1.0)]   // Negative AmountOut
        [InlineData(0, 0, -0.001)] // Negative FeeAmount
        public void Constructor_InvalidAmounts_ShouldThrowArgumentException(
            decimal amountIn = 0,
            decimal amountOut = 0,
            decimal feeAmount = 0)
        {
            Assert.Throws<ArgumentException>(() =>
                new CanonicalTransaction(
                    timestamp: DateTime.UtcNow,
                    type: TransactionType.Trade,
                    assetIn: "BTC",
                    amountIn: amountIn,
                    assetOut: "SEK",
                    amountOut: amountOut,
                    feeAsset: "BTC",
                    feeAmount: feeAmount,
                    fiatValueAtTimestamp: 1000m,
                    fiatValueAtTimestampCurrency: "SEK"
                ));
        }

        [Fact]
        public void Constructor_MissingAssets_ShouldThrowArgumentException()
        {
            Assert.Throws<ArgumentException>(() =>
                new CanonicalTransaction(
                    timestamp: DateTime.UtcNow,
                    type: TransactionType.Trade,
                    assetIn: null,
                    amountIn: 0,
                    assetOut: null,
                    amountOut: 0,
                    feeAsset: null,
                    feeAmount: 0,
                    fiatValueAtTimestamp: 0,
                    fiatValueAtTimestampCurrency: "SEK"
                ));
        }

        [Fact]
        public void Constructor_FutureTimestamp_ShouldThrowArgumentException()
        {
            Assert.Throws<ArgumentException>(() =>
                new CanonicalTransaction(
                    timestamp: DateTime.UtcNow.AddHours(2),
                    type: TransactionType.Trade,
                    assetIn: "BTC",
                    amountIn: 0.5m,
                    assetOut: "SEK",
                    amountOut: 250000m,
                    feeAsset: "BTC",
                    feeAmount: 0.001m,
                    fiatValueAtTimestamp: 250000m,
                    fiatValueAtTimestampCurrency: "SEK"
                ));
        }
    }
}