using System.Collections.Generic;
using TaxLedger.Domain.Transactions;

namespace TaxLedger.Domain.TaxEngine
{
    public class AverageCostCalculator
    {
        private readonly Dictionary<string, (decimal Amount, decimal TotalCost)> _holdings
            = new();

        private readonly Dictionary<string, decimal> _realizedGains = new();

        public void AddHolding(string asset, decimal amount, decimal totalCost)
        {
            if (_holdings.ContainsKey(asset))
            {
                var old = _holdings[asset];
                _holdings[asset] = (old.Amount + amount, old.TotalCost + totalCost);
            }
            else
            {
                _holdings[asset] = (amount, totalCost);
            }
        }

        public (decimal Amount, decimal TotalCost) GetHoldings(string asset)
        {
            return _holdings.TryGetValue(asset, out var h) ? h : (0m, 0m);
        }

        // New method for tests
        public decimal GetRealizedGain(string asset)
        {
            return _realizedGains.TryGetValue(asset, out var g) ? g : 0m;
        }

        // Example: add realized gain whenever disposing an asset
        private void RecordGain(string asset, decimal gain)
        {
            if (_realizedGains.ContainsKey(asset))
                _realizedGains[asset] += gain;
            else
                _realizedGains[asset] = gain;
        }

        public void ProcessTransaction(CanonicalTransaction tx)
        {
            switch (tx.Type)
            {
                case TransactionType.Trade:
                    if (tx.AssetIn != "FIAT") // selling crypto or swapping
                    {
                        decimal gain = DisposeAsset(tx.AssetIn, tx.AmountIn, tx.FiatValueAtTimestamp);
                        RecordGain(tx.AssetIn, gain);
                    }
                    AddHolding(tx.AssetOut, tx.AmountOut, tx.FiatValueAtTimestamp);
                    break;
            }
        }

        private decimal DisposeAsset(string asset, decimal amount, decimal fiatReceived)
        {
            if (!_holdings.ContainsKey(asset))
                throw new System.InvalidOperationException($"No holdings available for {asset}.");

            var holding = _holdings[asset];
            if (holding.Amount < amount)
                throw new System.InvalidOperationException($"Not enough {asset} to dispose.");

            decimal averageCostPerUnit = holding.TotalCost / holding.Amount;
            decimal totalCostRemoved = averageCostPerUnit * amount;
            _holdings[asset] = (holding.Amount - amount, holding.TotalCost - totalCostRemoved);

            return fiatReceived - totalCostRemoved; // realized gain
        }
    }
}