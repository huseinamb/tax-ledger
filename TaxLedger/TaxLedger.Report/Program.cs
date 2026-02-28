using System;
using System.Collections.Generic;
using System.Linq;
using TaxLedger.Domain.TaxEngine;
using TaxLedger.Domain.Transactions;

Console.WriteLine("=== TaxLedger POC Report ===");

List<CanonicalTransaction> transactions = SyntheticDataset.GetSampleTransactions();

// Initialize the calculator
var calculator = new AverageCostCalculator();

// Dictionary to store realized gains per asset
var realizedGains = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

// Process transactions
foreach (var tx in transactions.OrderBy(t => t.Timestamp))
{
    // Before processing, capture the holdings snapshot for reporting if needed
    var previousHoldings = calculator.Holdings.ToDictionary(h => h.Key, h => h.Value.TotalAmount);

    // Process the transaction
    try
    {
        calculator.ProcessTransaction(tx);
    }
    catch (InvalidOperationException ex)
    {
        Console.WriteLine($"Warning: {ex.Message} (Transaction at {tx.Timestamp})");
        continue;
    }

    // If transaction was taxable, track gain
    if (tx.Type == TransactionType.Trade)
    {
        bool taxable = tx.AssetOut != tx.AssetIn; // swap or sell
        if (taxable)
        {
            decimal gain = tx.FiatValueAtTimestamp - previousHoldings.GetValueOrDefault(tx.AssetIn, 0m);
            if (!realizedGains.ContainsKey(tx.AssetIn))
                realizedGains[tx.AssetIn] = 0m;

            realizedGains[tx.AssetIn] += gain;
        }
    }
}

// Print realized gains
Console.WriteLine("\n--- Realized Gains ---");
foreach (var kv in realizedGains)
{
    Console.WriteLine($"{kv.Key}: {kv.Value} SEK");
}

// Print remaining holdings
Console.WriteLine("\n--- Remaining Holdings ---");
foreach (var kv in calculator.Holdings)
{
    Console.WriteLine($"{kv.Key}: {kv.Value.TotalAmount} units, TotalCost {kv.Value.TotalCost} SEK");
}

Console.WriteLine("\n=== End of Report ===");