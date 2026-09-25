import { NavLink, useNavigate } from "react-router-dom"
import { useAuth } from "../context/AuthContext"
import NotificationBell from "./NotificationBell"

const NAV_BY_ROLE = {
  Employee: [{ to: "/employee", label: "My requests" }],
  Manager: [
    { to: "/employee", label: "My requests" },
    { to: "/manager", label: "Team queue" },
    { to: "/reports", label: "Reports" },
  ],
  Finance: [
    { to: "/finance", label: "Finance queue" },
    { to: "/reports", label: "Reports" },
  ],
  Admin: [{ to: "/reports", label: "Reports" }],
}

export default function Layout({ children }) {
  const { user, logout } = useAuth()
  const navigate = useNavigate()
  const links = NAV_BY_ROLE[user?.role] ?? []

  function handleLogout() {
    logout()
    navigate("/login")
  }

  return (
    <div className="flex min-h-screen">
      <aside className="flex w-60 shrink-0 flex-col border-r border-line bg-panel">
        <div className="px-5 py-6">
          <p className="font-display text-lg font-semibold text-ink">Expense Control</p>
          <p className="mt-0.5 text-xs text-ink-soft">
            {user?.firstName} {user?.lastName} · {user?.role}
          </p>
        </div>
        <nav className="flex flex-1 flex-col gap-1 px-3">
          {links.map((link) => (
            <NavLink
              key={link.to}
              to={link.to}
              className={({ isActive }) =>
                `rounded-md px-3 py-2 text-sm font-medium ${
                  isActive
                    ? "bg-accent-soft text-accent"
                    : "text-ink-soft hover:bg-canvas hover:text-ink"
                }`
              }
            >
              {link.label}
            </NavLink>
          ))}
        </nav>
        <div className="border-t border-line px-3 py-4">
          <button
            onClick={handleLogout}
            className="w-full rounded-md px-3 py-2 text-left text-sm font-medium text-ink-soft hover:bg-canvas hover:text-ink"
          >
            Sign out
          </button>
        </div>
      </aside>

      <div className="flex-1">
        <header className="flex items-center justify-end border-b border-line bg-panel px-6 py-3">
          <NotificationBell />
        </header>
        <main className="p-6">{children}</main>
      </div>
    </div>
  )
}
