import { useState, useEffect } from 'react'

const API_BASE = 'https://localhost:7148'

function UploadForm({ onSubmit, loading }) {
    const [file, setFile] = useState(null)
    const [exchange, setExchange] = useState('')
    const [country, setCountry] = useState('')
    const [year, setYear] = useState('')

    // Dynamic lists from the API
    const [exchanges, setExchanges] = useState([])
    const [countries, setCountries] = useState([])

    // Fetch supported exchanges and countries when the component loads
    // useEffect runs once after the component is first rendered
    useEffect(() => {
        fetch(`${API_BASE}/api/exchanges`)
            .then(res => res.json())
            .then(data => {
                setExchanges(data)
                setExchange(data[0] ?? '') // default to first option
            })
            .catch(() => setExchanges(['Binance'])) // fallback if API is down

        fetch(`${API_BASE}/api/countries`)
            .then(res => res.json())
            .then(data => {
                setCountries(data)
                setCountry(data[0] ?? '')
            })
            .catch(() => setCountries(['Sweden']))
    }, []) // empty array means "run once on mount"

    const handleSubmit = (e) => {
        e.preventDefault()
        if (!file) return
        onSubmit({ file, exchange, country, year })
    }

    return (
        <form onSubmit={handleSubmit} className="upload-form">
            <div className="form-group">
                <label>Exchange</label>
                <select value={exchange} onChange={(e) => setExchange(e.target.value)}>
                    {exchanges.map(ex => (
                        <option key={ex} value={ex}>{ex}</option>
                    ))}
                </select>
            </div>

            <div className="form-group">
                <label>Country</label>
                <select value={country} onChange={(e) => setCountry(e.target.value)}>
                    {countries.map(c => (
                        <option key={c} value={c}>{c}</option>
                    ))}
                </select>
            </div>

            <div className="form-group">
                <label>Tax Year <span className="optional">(optional)</span></label>
                <input
                    type="number"
                    placeholder="defaults to latest year in data"
                    value={year}
                    min="2009"
                    max={new Date().getFullYear()}
                    onChange={(e) => setYear(e.target.value)}
                    autoComplete="off"
                />
            </div>

            <div className="form-group">
                <label>Transaction CSV</label>
                <div
                    className="file-drop"
                    onClick={() => document.getElementById('csv-input').click()}
                >
                    {file ? (
                        <span className="file-name">📄 {file.name}</span>
                    ) : (
                        <span>Click to select your CSV export</span>
                    )}
                    <input
                        id="csv-input"
                        type="file"
                        accept=".csv"
                        style={{ display: 'none' }}
                        onChange={(e) => setFile(e.target.files[0])}
                    />
                </div>
            </div>

            <button
                type="submit"
                disabled={!file || loading}
                className="submit-btn"
            >
                {loading ? 'Generating report...' : 'Generate Report'}
            </button>
        </form>
    )
}

export default UploadForm