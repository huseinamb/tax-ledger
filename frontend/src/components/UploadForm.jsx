// UploadForm.jsx
// This component is responsible for collecting user input:
// the CSV file, exchange, country and optional year.
// It receives an "onSubmit" function from App.jsx and calls it
// with the form data when the user clicks "Generate Report".
import { useState } from 'react'
function UploadForm({ onSubmit, loading }) {
    // Local state for each form field
    const [file, setFile] = useState(null)
    const [exchange, setExchange] = useState('Binance')
    const [country, setCountry] = useState('Sweden')
    const [year, setYear] = useState('')

    const handleSubmit = (e) => {
        e.preventDefault() // prevent page reload on form submit
        if (!file) return
        onSubmit({ file, exchange, country, year })
    }

    return (
        <form onSubmit={handleSubmit} className="upload-form">
            <div className="form-group">
                <label>Exchange</label>
                {/* TODO: replace with dynamic list from GET /api/exchanges */}
                <select value={exchange} onChange={(e) => setExchange(e.target.value)}>
                    <option value="Binance">Binance</option>
                </select>
            </div>

            <div className="form-group">
                <label>Country</label>
                {/* TODO: replace with dynamic list from GET /api/countries */}
                <select value={country} onChange={(e) => setCountry(e.target.value)}>
                    <option value="Sweden">Sweden</option>
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