// SummaryTable.jsx
// Displays the per-asset summary matching Skatteverket K4 format.
// Receives the summary array from the API response as a prop.

function SummaryTable({ summary, currency }) {
    const totalGain = summary.reduce((sum, row) => sum + row.totalGain, 0)
    const totalLoss = summary.reduce((sum, row) => sum + row.totalLoss, 0)

    return (
        <div className="table-wrapper">
            <table className="report-table">
                <thead>
                    <tr>
                        <th>Asset</th>
                        <th>Sale Price ({currency})</th>
                        <th>Cost Basis ({currency})</th>
                        <th>Gain ({currency})</th>
                        <th>Loss ({currency})</th>
                    </tr>
                </thead>
                <tbody>
                    {summary.map((row) => (
                        <tr key={row.asset}>
                            <td><strong>{row.asset}</strong></td>
                            <td>{row.totalSalePrice.toFixed(0)}</td>
                            <td>{row.totalCostBasis.toFixed(0)}</td>
                            <td className="gain">{row.totalGain.toFixed(0)}</td>
                            <td className="loss">{row.totalLoss.toFixed(0)}</td>
                        </tr>
                    ))}
                </tbody>
                <tfoot>
                    <tr className="totals-row">
                        <td><strong>Total</strong></td>
                        <td></td>
                        <td></td>
                        <td className="gain"><strong>{totalGain.toFixed(0)}</strong></td>
                        <td className="loss"><strong>{totalLoss.toFixed(0)}</strong></td>
                    </tr>
                </tfoot>
            </table>
        </div>
    )
}

export default SummaryTable