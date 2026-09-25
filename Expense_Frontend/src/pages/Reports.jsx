import { useEffect, useState } from "react"
import { apiFetch } from "../api/client"

export default function Reports() {
  const [reports, setReports] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState("")

  useEffect(() => {
    apiFetch("/api/Expense/reports/expenses/by-department")
      .then(setReports)
      .catch((e) => setError(e.message))
      .finally(() => setLoading(false))
  }, [])

  const max = Math.max(1, ...reports.map((r) => r.total))

  return (
    <div className="mx-auto max-w-3xl">
      <h1 className="mb-1 font-display text-lg font-semibold text-ink">
        Spending by department
      </h1>
      <p className="mb-6 text-sm text-ink-soft">Paid requests only.</p>

      {loading && <p className="text-sm text-ink-soft">Loading…</p>}
      {error && <p className="text-sm text-danger">{error}</p>}

      {!loading && !error && (
        <div className="space-y-4 rounded-lg border border-line bg-panel p-5">
          {reports.map((r) => (
            <div key={r.name}>
              <div className="mb-1 flex items-baseline justify-between">
                <span className="text-sm font-medium text-ink">{r.name}</span>
                <span className="font-display text-sm font-semibold text-ink">
                  ₦{Number(r.total).toLocaleString()}
                </span>
              </div>
              <div className="h-2 w-full overflow-hidden rounded-full bg-canvas">
                <div
                  className="h-full rounded-full bg-accent"
                  style={{ width: `${(r.total / max) * 100}%` }}
                />
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  )
}
