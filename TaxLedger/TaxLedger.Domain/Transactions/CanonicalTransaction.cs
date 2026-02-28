using System;

namespace TaxLedger.Domain.Transactions
{
    public sealed class CanonicalTransaction
    {
        public Guid Id { get; }
        public DateTime Timestamp { get; }

        public TransactionType Type { get; }

        public string? AssetIn { get; }
        public decimal AmountIn { get; }

        public string? AssetOut { get; }
        public decimal AmountOut { get; }

        public string? FeeAsset { get; }
        public decimal FeeAmount { get; }

        public decimal FiatValueAtTimestamp { get; }
        public string FiatValueAtTimestampCurrency { get; }

        public CanonicalTransaction(
            DateTime timestamp,
            TransactionType type,
            string? assetIn,
            decimal amountIn,
            string? assetOut,
            decimal amountOut,
            string? feeAsset,
            decimal feeAmount,
            decimal fiatValueAtTimestamp,
            string fiatValueAtTimestampCurrency
        )
        {
            // Basic null checks for assets
            if (type == TransactionType.Trade || type == TransactionType.Deposit || type == TransactionType.Withdrawal)
            {
                if (string.IsNullOrWhiteSpace(assetIn) && string.IsNullOrWhiteSpace(assetOut))
                {
                    throw new ArgumentException("At least one of AssetIn or AssetOut must be provided.");
                }
            }

            // Amount checks
            if (amountIn < 0) throw new ArgumentException("AmountIn cannot be negative.");
            if (amountOut < 0) throw new ArgumentException("AmountOut cannot be negative.");
            if (feeAmount < 0) throw new ArgumentException("FeeAmount cannot be negative.");

            // Fiat value check
            if (fiatValueAtTimestamp < 0) throw new ArgumentException("Fiat value cannot be negative.");

            // Optional timestamp check
            if (timestamp > DateTime.UtcNow.AddMinutes(5))
                throw new ArgumentException("Timestamp cannot be in the far future.");

            Id = Guid.NewGuid();
            Timestamp = timestamp;

            Type = type;

            AssetIn = assetIn;
            AmountIn = amountIn;

            AssetOut = assetOut;
            AmountOut = amountOut;

            FeeAsset = feeAsset;
            FeeAmount = feeAmount;

            FiatValueAtTimestamp = fiatValueAtTimestamp;
            FiatValueAtTimestampCurrency = fiatValueAtTimestampCurrency;
        }
    }
}