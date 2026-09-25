import { useEffect, useState } from "react"
import { useNavigate } from "react-router-dom"
import { useAuth } from "../context/AuthContext"
import { apiFetch } from "../api/client"

const ROLE_HOME = {
  Employee: "/employee",
  Manager: "/employee",
  Finance: "/finance",
  Admin: "/reports",
}

export default function AuthPage() {
  const [mode, setMode] = useState("login")
  const { login, signup } = useAuth()
  const navigate = useNavigate()
  const [error, setError] = useState("")
  const [busy, setBusy] = useState(false)

  async function handleLogin(e) {
    e.preventDefault()
    setError("")
    setBusy(true)
    const form = new FormData(e.target)
    try {
      const user = await login(form.get("email"), form.get("password"))
      navigate(ROLE_HOME[user.role] ?? "/")
    } catch (err) {
      setError(err.message)
    } finally {
      setBusy(false)
    }
  }

  async function handleSignup(e) {
    e.preventDefault()
    setError("")
    setBusy(true)
    const form = new FormData(e.target)
    try {
      await signup({
        firstName: form.get("firstName"),
        lastName: form.get("lastName"),
        email: form.get("email"),
        password: form.get("password"),
        departmentId: Number(form.get("departmentId")),
      })
      setMode("login")
    } catch (err) {
      setError(err.message)
    } finally {
      setBusy(false)
    }
  }

  return (
    <div className="flex min-h-screen items-center justify-center bg-canvas px-4">
      <div className="w-full max-w-sm">
        <p className="mb-8 text-center font-display text-2xl font-semibold text-ink">
          Expense Control
        </p>

        <div className="mb-6 flex rounded-md border border-line bg-white p-1">
          <button
            onClick={() => setMode("login")}
            className={`flex-1 rounded px-3 py-1.5 text-sm font-medium ${
              mode === "login" ? "bg-ink text-white" : "text-ink-soft"
            }`}
          >
            Log in
          </button>
          <button
            onClick={() => setMode("signup")}
            className={`flex-1 rounded px-3 py-1.5 text-sm font-medium ${
              mode === "signup" ? "bg-ink text-white" : "text-ink-soft"
            }`}
          >
            Sign up
          </button>
        </div>

        {error && (
          <p className="mb-4 rounded-md border border-danger bg-danger-soft px-3 py-2 text-sm text-danger">
            {error}
          </p>
        )}

        {mode === "login" ? (
          <form onSubmit={handleLogin} className="space-y-3">
            <Field label="Email" name="email" type="email" required />
            <Field label="Password" name="password" type="password" required />
            <SubmitButton busy={busy}>Log in</SubmitButton>
          </form>
        ) : (
          <SignupForm onSubmit={handleSignup} busy={busy} />
        )}
      </div>
    </div>
  )
}

function SignupForm({ onSubmit, busy }) {
  const [departments, setDepartments] = useState([])
  const [deptError, setDeptError] = useState(false)

  useEffect(() => {
    apiFetch("/api/Department")
      .then(setDepartments)
      .catch(() => setDeptError(true))
  }, [])

  return (
    <form onSubmit={onSubmit} className="space-y-3">
      <div className="grid grid-cols-2 gap-3">
        <Field label="First name" name="firstName" required />
        <Field label="Last name" name="lastName" required />
      </div>
      <Field label="Email" name="email" type="email" required />
      <Field label="Password" name="password" type="password" required minLength={6} />
      <div>
        <label className="mb-1 block text-sm font-medium text-ink-soft">Department</label>
        {deptError ? (
          <p className="text-sm text-danger">
            Couldn't load departments — an admin needs to create one first.
          </p>
        ) : (
          <select
            name="departmentId"
            required
            className="w-full rounded-md border border-line bg-white px-3 py-2 text-sm outline-none focus:border-accent"
          >
            <option value="">Select a department…</option>
            {departments.map((d) => (
              <option key={d.id} value={d.id}>
                {d.name}
              </option>
            ))}
          </select>
        )}
      </div>
      <SubmitButton busy={busy}>Create account</SubmitButton>
    </form>
  )
}

function Field({ label, ...props }) {
  return (
    <div>
      <label className="mb-1 block text-sm font-medium text-ink-soft">{label}</label>
      <input
        {...props}
        className="w-full rounded-md border border-line bg-white px-3 py-2 text-sm outline-none focus:border-accent"
      />
    </div>
  )
}

function SubmitButton({ busy, children }) {
  return (
    <button
      type="submit"
      disabled={busy}
      className="w-full rounded-md bg-ink py-2 text-sm font-medium text-white hover:bg-ink-soft disabled:opacity-60"
    >
      {busy ? "Please wait…" : children}
    </button>
  )
}
