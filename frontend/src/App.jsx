// App.jsx
// The root component. Manages the report state and
// orchestrates UploadForm, SummaryTable and TransactionTable.

import { useState } from 'react'
import UploadForm from './components/UploadForm'
import SummaryTable from './components/SummaryTable'
import TransactionTable from './components/TransactionTable'
import './App.css'

// TODO: move to environment variable
const API_BASE = 'https://localhost:7148'

function App() {
  const [report, setReport] = useState(null)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState(null)

  const handleFormSubmit = async ({ file, exchange, country, year }) => {
    setLoading(true)
    setError(null)
    setReport(null)

    try {
      const formData = new FormData()
      formData.append('file', file)

      const params = new URLSearchParams({ exchange, country })
      if (year) params.append('year', year)

      const response = await fetch(
        `${API_BASE}/api/report/json?${params}`,
        { method: 'POST', body: formData }
      )

      if (!response.ok) {
        const errorText = await response.text()
        throw new Error(errorText)
      }

      const data = await response.json()
      setReport(data)
    } catch (err) {
      setError(err.message)
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="app">
      <header className="app-header">
        <h1>TaxLedger</h1>
        <p>Generate your crypto tax report from your exchange transaction history.</p>
      </header>

      <main className="app-main">
        <section className="card card-narrow">
          <h2>Upload Transactions</h2>
          <UploadForm onSubmit={handleFormSubmit} loading={loading} />
        </section>

        {error && (
          <div className="error-box">
            <strong>Error:</strong> {error}
          </div>
        )}

        {report && (
          <>
            <section className="card">
              <h2>Report — {report.taxYear}</h2>
              <div className="report-meta">
                <span>{report.exchange}</span>
                <span>{report.country}</span>
                <span>{report.currency}</span>
                <span>{report.taxableEvents} taxable events</span>
              </div>
            </section>

            <section className="card">
              <h2>Summary by Asset</h2>
              <p className="section-hint">
                Grouped by asset as required by Skatteverket K4 Section D.
                Gains and losses are listed separately — losses are 70% deductible.
              </p>
              <SummaryTable summary={report.summary} currency={report.currency} />
            </section>

            <section className="card">
              <h2>Transaction Breakdown</h2>
              <p className="section-hint">
                Full transaction-level detail for manual verification.
              </p>
              <TransactionTable
                transactions={report.transactions}
                currency={report.currency}
              />
            </section>
          </>
        )}
      </main>
    </div>
  )
}

export default App