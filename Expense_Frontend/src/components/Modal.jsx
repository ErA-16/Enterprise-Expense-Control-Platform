import { useEffect } from "react"

export default function Modal({ title, onClose, children, wide = false }) {
  useEffect(() => {
    function onKeyDown(e) {
      if (e.key === "Escape") onClose()
    }
    document.addEventListener("keydown", onKeyDown)
    return () => document.removeEventListener("keydown", onKeyDown)
  }, [onClose])

  return (
    <div
      className="fixed inset-0 z-50 flex items-center justify-center bg-ink/40 p-4"
      onClick={onClose}
    >
      <div
        className={`max-h-[85vh] w-full ${wide ? "max-w-2xl" : "max-w-md"} overflow-y-auto rounded-lg border border-line bg-panel p-6 shadow-xl`}
        onClick={(e) => e.stopPropagation()}
        role="dialog"
        aria-modal="true"
        aria-label={title}
      >
        <div className="mb-4 flex items-start justify-between gap-4">
          <h2 className="font-display text-lg font-semibold text-ink">{title}</h2>
          <button
            onClick={onClose}
            aria-label="Close"
            className="text-ink-soft hover:text-ink"
          >
            ✕
          </button>
        </div>
        {children}
      </div>
    </div>
  )
}
