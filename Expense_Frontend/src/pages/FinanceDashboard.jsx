import { useEffect, useState } from "react"
import { apiFetch } from "../api/client"
import StatusBadge from "../components/StatusBadge"
import Modal from "../components/Modal"
import RequestHistoryModal from "../components/RequestHistoryModal"
import { EmptyState } from "./EmployeeDashboard"

export default function FinanceDashboard() {
  const [tab, setTab] = useState("review")

  return (
    <div className="mx-auto max-w-3xl">
      <div className="mb-4 flex gap-1 rounded-md border border-line bg-white p-1" style={{ width: "fit-content" }}>
        <TabButton active={tab === "review"} onClick={() => setTab("review")}>
          Awaiting review
        </TabButton>
        <TabButton active={tab === "processing"} onClick={() => setTab("processing")}>
          Ready to pay
        </TabButton>
        <TabButton active={tab === "paid"} onClick={() => setTab("paid")}>
          Paid history
        </TabButton>
      </div>

      {tab === "review" && <ReviewQueue />}
      {tab === "processing" && <ProcessingQueue />}
      {tab === "paid" && <PaidHistory />}
    </div>
  )
}

function TabButton({ active, onClick, children }) {
  return (
    <button
      onClick={onClick}
      className={`rounded px-3 py-1.5 text-sm font-medium ${
        active ? "bg-ink text-white" : "text-ink-soft"
      }`}
    >
      {children}
    </button>
  )
}

function ReviewQueue() {
  const [requests, setRequests] = useState([])
  const [loading, setLoading] = useState(true)
  const [acting, setActing] = useState(null)

  async function load() {
    setLoading(true)
    try {
      setRequests(await apiFetch("/api/Expense/finance/pending-requests"))
    } catch {
      setRequests([])
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    load()
  }, [])

  async function decide(id, isApproved) {
    setActing(id)
    try {
      await apiFetch(`/api/Expense/finance/${id}/process`, {
        method: "POST",
        body: { isApproved },
      })
      load()
    } finally {
      setActing(null)
    }
  }

  if (loading) return <p className="text-sm text-ink-soft">Loading…</p>
  if (requests.length === 0) {
    return <EmptyState message="Nothing waiting on finance review right now." />
  }

  return (
    <div className="space-y-2">
      {requests.map((r) => (
        <div
          key={r.id}
          className="flex items-center justify-between rounded-lg border border-line bg-panel px-4 py-3"
        >
          <div>
            <p className="text-sm font-medium text-ink">{r.title}</p>
            <p className="text-xs text-ink-soft">₦{Number(r.amount).toLocaleString()}</p>
          </div>
          <div className="flex gap-2">
            <button
              disabled={acting === r.id}
              onClick={() => decide(r.id, true)}
              className="rounded-md bg-success px-3 py-1.5 text-xs font-medium text-white disabled:opacity-60"
            >
              Approve for payment
            </button>
            <button
              disabled={acting === r.id}
              onClick={() => decide(r.id, false)}
              className="rounded-md bg-danger px-3 py-1.5 text-xs font-medium text-white disabled:opacity-60"
            >
              Reject
            </button>
          </div>
        </div>
      ))}
    </div>
  )
}

function ProcessingQueue() {
  const [requests, setRequests] = useState([])
  const [loading, setLoading] = useState(true)
  const [paying, setPaying] = useState(null)

  async function load() {
    setLoading(true)
    try {
      setRequests(await apiFetch("/api/Expense/requests/processing-requests"))
    } catch {
      setRequests([])
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    load()
  }, [])

  if (loading) return <p className="text-sm text-ink-soft">Loading…</p>
  if (requests.length === 0) {
    return <EmptyState message="Nothing ready to pay right now." />
  }

  return (
    <div className="space-y-2">
      {requests.map((r) => (
        <div
          key={r.id}
          className="flex items-center justify-between rounded-lg border border-line bg-panel px-4 py-3"
        >
          <div>
            <p className="text-sm font-medium text-ink">{r.title}</p>
            <p className="text-xs text-ink-soft">₦{Number(r.amount).toLocaleString()}</p>
          </div>
          <button
            onClick={() => setPaying(r)}
            className="rounded-md bg-accent px-3 py-1.5 text-xs font-medium text-white"
          >
            Record payment
          </button>
        </div>
      ))}

      {paying && (
        <PayModal
          request={paying}
          onClose={() => setPaying(null)}
          onDone={() => {
            setPaying(null)
            load()
          }}
        />
      )}
    </div>
  )
}

// New: relies on GET /api/Expense/finance/paid-requests, which needs to be added
// to the backend first — see IExpenseService/ExpenseService/ExpenseController.
function PaidHistory() {
  const [requests, setRequests] = useState([])
  const [loading, setLoading] = useState(true)
  const [historyId, setHistoryId] = useState(null)

  useEffect(() => {
    apiFetch("/api/Expense/finance/paid-requests")
      .then(setRequests)
      .catch(() => setRequests([]))
      .finally(() => setLoading(false))
  }, [])

  if (loading) return <p className="text-sm text-ink-soft">Loading…</p>
  if (requests.length === 0) {
    return <EmptyState message="Nothing paid yet." />
  }

  return (
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
              ₦{Number(r.amount).toLocaleString()} · {r.paymentMethod} · {r.paidAt}
            </p>
          </div>
          <StatusBadge status={r.status} />
        </button>
      ))}
      {historyId && (
        <RequestHistoryModal requestId={historyId} onClose={() => setHistoryId(null)} />
      )}
    </div>
  )
}

function PayModal({ request, onClose, onDone }) {
  const [method, setMethod] = useState("BankTransfer")
  const [file, setFile] = useState(null)
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState("")

  async function handlePay(e) {
    e.preventDefault()
    if (!file) {
      setError("Proof of payment is required.")
      return
    }
    setBusy(true)
    setError("")
    try {
      const formData = new FormData()
      formData.append("paymentMethod", method)
      formData.append("paymentProof", file)
      await apiFetch(`/api/Expense/finance/${request.id}/pay`, { method: "POST", formData })
      onDone()
    } catch (err) {
      setError(err.message)
      setBusy(false)
    }
  }

  return (
    <Modal title={`Pay: ${request.title}`} onClose={onClose}>
      <p className="mb-4 font-display text-xl font-semibold text-ink">
        ₦{Number(request.amount).toLocaleString()}
      </p>

      {error && <p className="mb-3 text-sm text-danger">{error}</p>}

      <form onSubmit={handlePay} className="space-y-3">
        <div>
          <label className="mb-1 block text-sm font-medium text-ink-soft">Payment method</label>
          <select
            value={method}
            onChange={(e) => setMethod(e.target.value)}
            className="w-full rounded-md border border-line bg-white px-3 py-2 text-sm outline-none focus:border-accent"
          >
            <option value="BankTransfer">Bank transfer</option>
            <option value="Cash">Cash</option>
          </select>
        </div>
        <div>
          <label className="mb-1 block text-sm font-medium text-ink-soft">
            Proof of payment
          </label>
          <input
            type="file"
            accept="image/jpeg,image/png,application/pdf"
            onChange={(e) => setFile(e.target.files[0] ?? null)}
            required
            className="w-full rounded-md border border-line bg-white px-2.5 py-1.5 text-xs outline-none file:mr-2 file:rounded file:border-0 file:bg-canvas file:px-2 file:py-1 file:text-xs"
          />
        </div>
        <button
          type="submit"
          disabled={busy}
          className="w-full rounded-md bg-accent py-2 text-sm font-medium text-white disabled:opacity-60"
        >
          {busy ? "Recording…" : "Confirm payment"}
        </button>
      </form>
    </Modal>
  )
}
