// TransactionTable.jsx
// Displays the full transaction-level breakdown.
// Useful for manual verification of individual trades.

function TransactionTable({ transactions, currency }) {
    return (
        <div className="table-wrapper">
            <table className="report-table">
                <thead>
                    <tr>
                        <th>Timestamp</th>
                        <th>Asset</th>
                        <th>Sale Price ({currency})</th>
                        <th>Cost Basis ({currency})</th>
                        <th>Gain/Loss ({currency})</th>
                    </tr>
                </thead>
                <tbody>
                    {transactions.map((tx, i) => (
                        <tr key={i}>
                            <td>{new Date(tx.timestamp).toLocaleString()}</td>
                            <td><strong>{tx.asset}</strong></td>
                            <td>{tx.salePrice.toFixed(0)}</td>
                            <td>{tx.costBasis.toFixed(0)}</td>
                            <td className={tx.gainLoss >= 0 ? 'gain' : 'loss'}>
                                {tx.gainLoss.toFixed(0)}
                            </td>
                        </tr>
                    ))}
                </tbody>
            </table>
        </div>
    )
}

export default TransactionTable