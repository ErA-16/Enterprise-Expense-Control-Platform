const STATUS_STYLES = {
  Pending: "text-accent border-accent bg-accent-soft",
  Approved: "text-info border-info bg-info-soft",
  Processing: "text-info border-info bg-info-soft",
  Paid: "text-success border-success bg-success-soft",
  Rejected: "text-danger border-danger bg-danger-soft",
}

export default function StatusBadge({ status }) {
  const style = STATUS_STYLES[status] || "text-ink-soft border-line bg-white"
  return (
    <span
      className={`inline-flex items-center gap-1.5 rounded-full border px-2.5 py-0.5 text-xs font-medium ${style}`}
    >
      <span className="h-1.5 w-1.5 rounded-full bg-current" />
      {status}
    </span>
  )
}
