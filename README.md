# TaxLedger

TaxLedger helps cryptocurrency traders generate accurate tax reports from their exchange transaction history. You export your trades from an exchange, TaxLedger prices each transaction at the correct market rate, applies the tax rules for your country, and produces a ready-to-review report.

Currently supports **Binance** exports and **Swedish tax regulations (K4 / Section D)**.

> ⚠️ **Work in progress.** The tax calculation logic has been verified against published Skatteverket examples. The full pipeline (including live price enrichment) has not yet been verified end-to-end against a confirmed dataset. Do not use for actual tax filings without reviewing the output carefully.

---

## What it does

1. **Parses** your exchange CSV export into a normalised transaction format
2. **Prices** each transaction in your local currency at the exact time it occurred — using Binance 1-minute klines for crypto prices and ECB rates via [Frankfurter](https://frankfurter.dev) for currency conversion
3. **Calculates** your taxable gains and losses using the correct method for your country
4. **Reports** a summary grouped by asset, ready to review or file

For Sweden, this means applying *Genomsnittsmetoden* (GAV — average acquisition cost) and separating gains from losses as required by Skatteverket K4 Section D.

---

## Getting started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Internet connection (used to fetch historical prices and exchange rates — no API keys needed)

### Run the tests
```bash
git clone https://github.com/huseinamb/tax-ledger.git
cd tax-ledger/TaxLedger
dotnet test
```

### Try it with your own data

Export your transaction history from Binance as a CSV file, then run:

```bash
dotnet run --project TaxLedger.Api -- path/to/your_binance_export.csv
```

This will print a K4-style tax summary to the console for the year 2024.

---

## Supported exchanges and countries

| Exchange | Status |
|----------|--------|
| Binance  | ✅ Supported |
| Coinbase | 🔲 Planned |
| Kraken   | 🔲 Planned |

| Country | Tax method | Report format |
|---------|------------|---------------|
| Sweden  | GAV — Genomsnittsmetoden | K4 Section D |
| USA     | 🔲 Planned (FIFO) | — |

---

## How it's built

The solution is structured around Clean Architecture — the tax engine has no knowledge of HTTP, files, or any specific exchange. Adding a new exchange is one new CSV adapter. Adding a new country is one new strategy class.

```
TaxLedger.Domain          # Transaction models, tax engine interfaces
TaxLedger.Application     # Pipeline orchestration, pricing contracts
TaxLedger.Infrastructure  # Binance API, Frankfurter forex, price enrichment
TaxLedger.ExchangeAdapters # CSV parsers per exchange (currently Binance)
TaxLedger.Api             # ASP.NET Core Web API (in progress)
TaxLedger.Tests           # xUnit unit and integration tests
```

Prices are fetched from public endpoints only — no API keys are stored or required anywhere in the codebase.

---

## Roadmap

- [x] Domain model — `CanonicalTransaction`, `TransactionType`, `AssetHolding`
- [x] Swedish GAV strategy — *Genomsnittsmetoden* with correct SEK fee handling
- [x] K4 report generator — grouped by asset, gains and losses separated (70% rule)
- [x] Binance CSV adapter — handles all operation name variants and n-split trades
- [x] Price enrichment pipeline — Binance 1m klines + Frankfurter forex, no API keys needed
- [x] Full end-to-end pipeline — CSV → parse → enrich → calculate → report
- [x] Tax calculation logic verified against published Skatteverket examples (unit tested)
- [ ] End-to-end verification of enriched pipeline output against a known dataset
- [ ] REST API — upload CSV, select country and year, receive report
- [ ] React frontend — file upload and report display
- [ ] Coinbase and Kraken CSV adapters
- [ ] US FIFO tax strategy
- [ ] `.sru` export for direct Skatteverket digital filing
