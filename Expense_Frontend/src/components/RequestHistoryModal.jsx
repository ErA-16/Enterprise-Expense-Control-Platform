import { useEffect, useState } from "react"
import Modal from "./Modal"
import { apiFetch } from "../api/client"

const ACTION_LABEL = {
  Created: "Submitted",
  Approved: "Approved",
  Rejected: "Rejected",
  Processing: "Sent to processing",
  Paid: "Paid",
}

export default function RequestHistoryModal({ requestId, onClose }) {
  const [history, setHistory] = useState(null)
  const [error, setError] = useState(null)

  useEffect(() => {
    apiFetch(`/api/Expense/${requestId}/history`)
      .then(setHistory)
      .catch((e) => setError(e.message))
  }, [requestId])

  return (
    <Modal title={`Request #${requestId} history`} onClose={onClose}>
      {error && <p className="text-sm text-danger">{error}</p>}
      {!error && !history && <p className="text-sm text-ink-soft">Loading…</p>}
      {history && (
        <ol className="relative ml-2 border-l border-line pl-6">
          {history.map((event, i) => (
            <li key={i} className="mb-5 last:mb-0">
              <span className="absolute -ml-[29px] mt-1 h-2.5 w-2.5 rounded-full border-2 border-accent bg-panel" />
              <p className="text-sm font-medium text-ink">
                {ACTION_LABEL[event.action] ?? event.action}
                <span className="font-normal text-ink-soft"> — {event.actorName}</span>
              </p>
              <p className="text-xs text-ink-soft">{event.timestamp}</p>
            </li>
          ))}
        </ol>
      )}
    </Modal>
  )
}
