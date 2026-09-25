import { useEffect, useState } from "react"
import { apiFetch } from "../api/client"
import StatusBadge from "../components/StatusBadge"
import RequestHistoryModal from "../components/RequestHistoryModal"

export default function EmployeeDashboard() {
  const [requests, setRequests] = useState([])
  const [loading, setLoading] = useState(true)
  const [historyId, setHistoryId] = useState(null)

  async function loadRequests() {
    setLoading(true)
    try {
      const data = await apiFetch("/api/Expense/myrequests")
      setRequests(data)
    } catch {
      setRequests([]) // a 404 here just means "no requests yet" — treat it as empty, not an error
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    loadRequests()
  }, [])

  return (
    <div className="mx-auto max-w-3xl space-y-8">
      <SubmitRequestForm onSubmitted={loadRequests} />

      <section>
        <h2 className="mb-3 font-display text-lg font-semibold text-ink">My requests</h2>
        {loading && <p className="text-sm text-ink-soft">Loading…</p>}
        {!loading && requests.length === 0 && (
          <EmptyState message="Nothing submitted yet — your first request will show up here." />
        )}
        <div className="space-y-2">
          {requests.map((r) => (
            <button
              key={r.id}
              onClick={() => setHistoryId(r.id)}
              className="flex w-full items-center justify-between rounded-lg border border-line bg-panel px-4 py-3 text-left hover:border-ink-soft"
            >
              <div>
                <p className="text-sm font-medium text-ink">{r.title}</p>
                <p className="text-xs text-ink-soft">
                  ₦{Number(r.amount).toLocaleString()} · {r.dateAndTime}
                </p>
              </div>
              <StatusBadge status={r.status} />
            </button>
          ))}
        </div>
      </section>

      {historyId && (
        <RequestHistoryModal requestId={historyId} onClose={() => setHistoryId(null)} />
      )}
    </div>
  )
}

function SubmitRequestForm({ onSubmitted }) {
  const [title, setTitle] = useState("")
  const [reason, setReason] = useState("")
  const [amount, setAmount] = useState("")
  const [file, setFile] = useState(null)
  const [suggestion, setSuggestion] = useState(null)
  const [aiBusy, setAiBusy] = useState(false)
  const [submitBusy, setSubmitBusy] = useState(false)
  const [error, setError] = useState("")
  const [success, setSuccess] = useState("")

  async function handleAiAssist() {
    if (!title) {
      setError("Add a title first so the assistant has something to work with.")
      return
    }
    setError("")
    setAiBusy(true)
    try {
      const result = await apiFetch("/api/Ai/assist", {
        method: "POST",
        body: { title, reason, amount: Number(amount) || 0 },
      })
      setSuggestion(result)
    } catch (err) {
      setError(err.message)
    } finally {
      setAiBusy(false)
    }
  }

  function acceptSuggestion() {
    setTitle(suggestion.suggestedTitle)
    setReason(suggestion.suggestedReason)
    setSuggestion(null)
  }

  async function handleSubmit(e) {
    e.preventDefault()
    setError("")
    setSuccess("")
    setSubmitBusy(true)
    try {
      const formData = new FormData()
      formData.append("title", title)
      formData.append("reason", reason)
      formData.append("amount", amount)
      if (file) formData.append("receipt", file)

      await apiFetch("/api/Expense/submit-request", { method: "POST", formData })

      setTitle("")
      setReason("")
      setAmount("")
      setFile(null)
      setSuggestion(null)
      setSuccess("Request submitted.")
      onSubmitted()
    } catch (err) {
      setError(err.message)
    } finally {
      setSubmitBusy(false)
    }
  }

  return (
    <section className="rounded-lg border border-line bg-panel p-5">
      <h2 className="mb-4 font-display text-lg font-semibold text-ink">Submit a request</h2>

      {error && (
        <p className="mb-3 rounded-md border border-danger bg-danger-soft px-3 py-2 text-sm text-danger">
          {error}
        </p>
      )}
      {success && (
        <p className="mb-3 rounded-md border border-success bg-success-soft px-3 py-2 text-sm text-success">
          {success}
        </p>
      )}

      <form onSubmit={handleSubmit} className="space-y-3">
        <div>
          <label className="mb-1 block text-sm font-medium text-ink-soft">Title</label>
          <input
            value={title}
            onChange={(e) => setTitle(e.target.value)}
            required
            maxLength={100}
            placeholder='e.g. 27" Monitor'
            className="w-full rounded-md border border-line bg-white px-3 py-2 text-sm outline-none focus:border-accent"
          />
        </div>

        <div>
          <label className="mb-1 block text-sm font-medium text-ink-soft">Reason</label>
          <textarea
            value={reason}
            onChange={(e) => setReason(e.target.value)}
            maxLength={500}
            rows={3}
            placeholder="Why do you need this?"
            className="w-full rounded-md border border-line bg-white px-3 py-2 text-sm outline-none focus:border-accent"
          />
        </div>

        <div className="grid grid-cols-2 gap-3">
          <div>
            <label className="mb-1 block text-sm font-medium text-ink-soft">Amount (₦)</label>
            <input
              type="number"
              min="0.01"
              step="0.01"
              value={amount}
              onChange={(e) => setAmount(e.target.value)}
              required
              className="w-full rounded-md border border-line bg-white px-3 py-2 text-sm outline-none focus:border-accent"
            />
          </div>
          <div>
            <label className="mb-1 block text-sm font-medium text-ink-soft">
              Receipt (optional)
            </label>
            <input
              type="file"
              accept="image/jpeg,image/png,application/pdf"
              onChange={(e) => setFile(e.target.files[0] ?? null)}
              className="w-full rounded-md border border-line bg-white px-2.5 py-1.5 text-xs outline-none file:mr-2 file:rounded file:border-0 file:bg-canvas file:px-2 file:py-1 file:text-xs"
            />
          </div>
        </div>

        {suggestion && (
          <div className="rounded-md border border-accent bg-accent-soft p-3 text-sm">
            <p className="mb-1 font-medium text-ink">AI suggestion</p>
            <p className="text-ink">
              <span className="text-ink-soft">Title: </span>
              {suggestion.suggestedTitle}
            </p>
            <p className="mt-1 text-ink">
              <span className="text-ink-soft">Reason: </span>
              {suggestion.suggestedReason}
            </p>
            <div className="mt-2 flex gap-2">
              <button
                type="button"
                onClick={acceptSuggestion}
                className="rounded-md bg-ink px-3 py-1 text-xs font-medium text-white"
              >
                Use this
              </button>
              <button
                type="button"
                onClick={() => setSuggestion(null)}
                className="rounded-md border border-line px-3 py-1 text-xs font-medium text-ink-soft"
              >
                Keep mine
              </button>
            </div>
          </div>
        )}

        <div className="flex items-center gap-2 pt-1">
          <button
            type="button"
            onClick={handleAiAssist}
            disabled={aiBusy}
            className="rounded-md border border-line bg-white px-3 py-2 text-sm font-medium text-ink-soft hover:text-ink disabled:opacity-60"
          >
            {aiBusy ? "Thinking…" : "Ask AI to polish this"}
          </button>
          <button
            type="submit"
            disabled={submitBusy}
            className="ml-auto rounded-md bg-accent px-4 py-2 text-sm font-medium text-white hover:opacity-90 disabled:opacity-60"
          >
            {submitBusy ? "Submitting…" : "Submit request"}
          </button>
        </div>
      </form>
    </section>
  )
}

export function EmptyState({ message }) {
  return (
    <div className="rounded-lg border border-dashed border-line bg-panel px-4 py-8 text-center text-sm text-ink-soft">
      {message}
    </div>
  )
}
