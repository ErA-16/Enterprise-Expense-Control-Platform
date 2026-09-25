import { useCallback, useEffect, useRef, useState } from "react"
import { apiFetch } from "../api/client"
import { useAuth } from "../context/AuthContext"
import { useNotificationHub } from "../lib/useNotificationHub"

export default function NotificationBell() {
  const { token } = useAuth()
  const [open, setOpen] = useState(false)
  const [notifications, setNotifications] = useState([])
  const [loading, setLoading] = useState(false)
  const wrapperRef = useRef(null)

  const unreadCount = notifications.filter((n) => !n.isRead).length

  async function load() {
    setLoading(true)
    try {
      const data = await apiFetch("/api/Notification")
      setNotifications(data)
    } catch {
      // a failed notification fetch shouldn't break the rest of the page
    } finally {
      setLoading(false)
    }
  }

  // Fetch once on load, for the backlog of notifications that already existed
  // before this tab connected.
  useEffect(() => {
    load()
  }, [])

  // Then SignalR pushes anything new the instant it happens — no more polling.
  const handleNewNotification = useCallback((notification) => {
    setNotifications((prev) => [notification, ...prev])
  }, [])
  useNotificationHub(token, handleNewNotification)

  useEffect(() => {
    function onClickOutside(e) {
      if (wrapperRef.current && !wrapperRef.current.contains(e.target)) {
        setOpen(false)
      }
    }
    document.addEventListener("mousedown", onClickOutside)
    return () => document.removeEventListener("mousedown", onClickOutside)
  }, [])

  async function markRead(id) {
    setNotifications((prev) =>
      prev.map((n) => (n.id === id ? { ...n, isRead: true } : n))
    )
    try {
      await apiFetch(`/api/Notification/${id}/read`, { method: "PATCH" })
    } catch {
      load() // reconcile with the server if the update failed
    }
  }

  return (
    <div className="relative" ref={wrapperRef}>
      <button
        onClick={() => setOpen((o) => !o)}
        className="relative rounded-md border border-line bg-white p-2 text-ink-soft hover:text-ink"
        aria-label="Notifications"
      >
        <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
          <path d="M18 8a6 6 0 0 0-12 0c0 7-3 9-3 9h18s-3-2-3-9" />
          <path d="M13.73 21a2 2 0 0 1-3.46 0" />
        </svg>
        {unreadCount > 0 && (
          <span className="absolute -right-1 -top-1 flex h-4 w-4 items-center justify-center rounded-full bg-accent text-[10px] font-semibold text-white">
            {unreadCount > 9 ? "9+" : unreadCount}
          </span>
        )}
      </button>

      {open && (
        <div className="absolute right-0 z-40 mt-2 w-80 rounded-lg border border-line bg-panel shadow-lg">
          <div className="border-b border-line px-4 py-3">
            <h3 className="font-display text-sm font-semibold">Notifications</h3>
          </div>
          <div className="max-h-80 overflow-y-auto">
            {loading && (
              <p className="px-4 py-6 text-center text-sm text-ink-soft">Loading…</p>
            )}
            {!loading && notifications.length === 0 && (
              <p className="px-4 py-6 text-center text-sm text-ink-soft">
                Nothing here yet.
              </p>
            )}
            {notifications.map((n) => (
              <button
                key={n.id}
                onClick={() => markRead(n.id)}
                className={`block w-full border-b border-line px-4 py-3 text-left text-sm last:border-b-0 hover:bg-canvas ${
                  n.isRead ? "text-ink-soft" : "text-ink"
                }`}
              >
                <div className="flex items-start gap-2">
                  {!n.isRead && (
                    <span className="mt-1.5 h-1.5 w-1.5 shrink-0 rounded-full bg-accent" />
                  )}
                  <div>
                    <p>{n.message}</p>
                    <p className="mt-0.5 text-xs text-ink-soft">{n.createdAt}</p>
                  </div>
                </div>
              </button>
            ))}
          </div>
        </div>
      )}
    </div>
  )
}
