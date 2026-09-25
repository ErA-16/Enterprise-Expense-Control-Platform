import { useEffect, useState } from "react"
import { apiFetch } from "../api/client"
import StatusBadge from "../components/StatusBadge"
import Modal from "../components/Modal"
import RequestHistoryModal from "../components/RequestHistoryModal"
import { EmptyState } from "./EmployeeDashboard"

export default function ManagerDashboard() {
  const [allRequests, setAllRequests] = useState([])
  const [loading, setLoading] = useState(true)
  const [tab, setTab] = useState("pending")
  const [reviewing, setReviewing] = useState(null)
  const [historyId, setHistoryId] = useState(null)

  // The backend endpoint returns every request in the manager's department,
  // not just pending ones — the tabs below just split that one list by status.
  async function load() {
    setLoading(true)
    try {
      setAllRequests(await apiFetch("/api/Expense/manager/pending-requests"))
    } catch {
      setAllRequests([])
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    load()
  }, [])

  const pending = allRequests.filter((r) => r.status === "Pending")
  const history = allRequests.filter((r) => r.status !== "Pending")

  return (
    <div className="mx-auto max-w-3xl">
      <div className="mb-4 flex gap-1 rounded-md border border-line bg-white p-1" style={{ width: "fit-content" }}>
        <TabButton active={tab === "pending"} onClick={() => setTab("pending")}>
          Pending ({pending.length})
        </TabButton>
        <TabButton active={tab === "history"} onClick={() => setTab("history")}>
          History
        </TabButton>
      </div>

      {loading && <p className="text-sm text-ink-soft">Loading…</p>}

      {!loading && tab === "pending" && (
        <>
          {pending.length === 0 && (
            <EmptyState message="No pending requests from your department right now." />
          )}
          <div className="space-y-2">
            {pending.map((r) => (
              <div
                key={r.id}
                className="flex items-center justify-between rounded-lg border border-line bg-panel px-4 py-3"
              >
                <div>
                  <p className="text-sm font-medium text-ink">{r.title}</p>
                  <p className="text-xs text-ink-soft">
                    ₦{Number(r.amount).toLocaleString()} · {r.reason}
                  </p>
                </div>
                <div className="flex items-center gap-2">
                  <StatusBadge status={r.status} />
                  <button
                    onClick={() => setReviewing(r)}
                    className="rounded-md bg-ink px-3 py-1.5 text-xs font-medium text-white"
                  >
                    Review
                  </button>
                </div>
              </div>
            ))}
          </div>
        </>
      )}

      {!loading && tab === "history" && (
        <>
          {history.length === 0 && (
            <EmptyState message="Nothing processed yet — approved, rejected, and paid requests will show up here." />
          )}
          <div className="space-y-2">
            {history.map((r) => (
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
        </>
      )}

      {reviewing && (
        <ReviewModal
          request={reviewing}
          onClose={() => setReviewing(null)}
          onDone={() => {
            setReviewing(null)
            load()
          }}
        />
      )}

      {historyId && (
        <RequestHistoryModal requestId={historyId} onClose={() => setHistoryId(null)} />
      )}
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

function ReviewModal({ request, onClose, onDone }) {
  const [comment, setComment] = useState("")
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState("")

  async function decide(isApproved) {
    setBusy(true)
    setError("")
    try {
      await apiFetch(`/api/Expense/manager/${request.id}/process`, {
        method: "POST",
        body: { isApproved, managerComment: comment },
      })
      onDone()
    } catch (err) {
      setError(err.message)
      setBusy(false)
    }
  }

  return (
    <Modal title={request.title} onClose={onClose}>
      <p className="mb-3 text-sm text-ink-soft">{request.reason}</p>
      <p className="mb-4 font-display text-xl font-semibold text-ink">
        ₦{Number(request.amount).toLocaleString()}
      </p>

      {error && <p className="mb-3 text-sm text-danger">{error}</p>}

      <label className="mb-1 block text-sm font-medium text-ink-soft">
        Comment (optional)
      </label>
      <textarea
        value={comment}
        onChange={(e) => setComment(e.target.value)}
        rows={2}
        maxLength={250}
        className="mb-4 w-full rounded-md border border-line bg-white px-3 py-2 text-sm outline-none focus:border-accent"
      />

      <div className="flex gap-2">
        <button
          disabled={busy}
          onClick={() => decide(true)}
          className="flex-1 rounded-md bg-success px-3 py-2 text-sm font-medium text-white disabled:opacity-60"
        >
          Approve
        </button>
        <button
          disabled={busy}
          onClick={() => decide(false)}
          className="flex-1 rounded-md bg-danger px-3 py-2 text-sm font-medium text-white disabled:opacity-60"
        >
          Reject
        </button>
      </div>
    </Modal>
  )
}
