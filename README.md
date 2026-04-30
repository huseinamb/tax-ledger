# TaxLedger

TaxLedger helps cryptocurrency traders generate accurate tax reports from their exchange transaction history. You export your trades from an exchange, TaxLedger prices each transaction at the correct market rate, applies the tax rules for your country, and produces a ready-to-review report.

Currently supports **Binance** exports and **Swedish tax regulations (K4 / Section D)**.

> ℹ️ **Work in progress.** Tax calculation logic is verified against published Skatteverket examples. As with any tax tool, always review the output carefully before filing. Live price data is fetched from Binance (crypto prices) and Frankfurter (forex rates, sourced from ECB) — small rounding differences may occur.

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

### Test the API with Swagger

Start the API:
```bash
cd tax-ledger/TaxLedger
dotnet run --project TaxLedger.Api
```

Open your browser and navigate to:
```
https://localhost:{port}/swagger
```

Available endpoints:
- `GET /api/exchanges` — returns supported exchanges
- `GET /api/countries` — returns supported countries
- `POST /api/report/json` — upload a CSV file and get a tax report

For the report endpoint, provide:
- `file` — your Binance CSV export
- `exchange` — e.g. `Binance`
- `country` — e.g. `Sweden`
- `year` — optional, defaults to the latest year in the data

The response includes a **summary** grouped by asset (matching Skatteverket K4 format) and a full **transaction-level breakdown** for manual verification.

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
TaxLedger.Domain           # Transaction models, tax engine interfaces
TaxLedger.Application      # Pipeline orchestration, pricing contracts, factories
TaxLedger.Infrastructure   # Binance API, Frankfurter forex, price enrichment, factory implementations
TaxLedger.ExchangeAdapters # CSV parsers per exchange (currently Binance)
TaxLedger.Api              # ASP.NET Core Web API
TaxLedger.Tests            # xUnit unit and integration tests
```

Prices are fetched from public endpoints only — no API keys are stored or required anywhere in the codebase.

---

## Roadmap

- [x] Domain model — `CanonicalTransaction`, `TransactionType`, `AssetHolding`
- [x] Swedish GAV strategy — *Genomsnittsmetoden* with correct SEK fee handling
- [x] Binance CSV adapter — handles all operation name variants and n-split trades
- [x] Price enrichment pipeline — Binance 1m klines + Frankfurter forex, no API keys needed
- [x] Full end-to-end pipeline — CSV → parse → enrich → calculate → report
- [x] Tax calculation logic verified against published Skatteverket examples (unit tested)
- [x] REST API — upload CSV, select country and year, receive JSON report
- [ ] React frontend — file upload, summary display, transaction breakdown
- [ ] CSV download endpoint
- [ ] Coinbase and Kraken CSV adapters
- [ ] US FIFO tax strategy
- [ ] `.sru` export for direct Skatteverket digital filing
