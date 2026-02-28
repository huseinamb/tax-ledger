# TaxLedger

A modular tax calculation engine designed to reconcile cryptocurrency transactions across multiple exchanges, currently specialized for Swedish tax regulations (K4/Section D).

## 🚀 The Vision
TaxLedger aims to be a full-stack solution where users can:
1. **Import**: Upload historical CSV/API data from multiple exchanges.
2. **Report**: Generate country-specific tax reports (Starting with Sweden).

## 🛠 Current Progress (v0.1 - Core Engine)
- **Strategy Pattern Architecture**: Built to easily add new country rules (e.g., USA/FIFO) without changing core logic.
- **Swedish GAV Calculation**: Implements the *Genomsnittsmetoden* (Average Cost Basis) including SEK fee handling.
- **Reporting**: Generates a CSV summary grouped by asset, ready for manual entry or review.

## 🛣 Roadmap
- [ ] **Frontend**: Build a React-based dashboard for transaction visualization.
- [ ] **Backend**: Implement an ASP.NET Core Web API to handle multi-user data.
- [ ] **Integrations**: Support for Binance, Coinbase, and Kraken CSV exports.
- [ ] **Digital Filing**: Export to Swedish `.sru` format for direct Skatteverket upload.

## 🧪 Quality Assurance
- Unit tested with **xUnit** to ensure mathematical accuracy on complex swaps.
- Strict data normalization to handle differing exchange formats.

## ⚙️ Getting Started
1. Clone the repo.
2. Run `dotnet test` to see the tax engine in action.
3. Check the `bin/Debug` folder for the sample K4 output after running the console app.