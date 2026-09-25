import { createContext, useContext, useState, useCallback } from "react"
import { apiFetch } from "../api/client"
import { decodeJwt } from "../lib/jwt"
import { CLAIM_KEYS } from "../lib/claims"

const AuthContext = createContext(null)

function userFromToken(token) {
  const payload = decodeJwt(token)
  if (!payload) return null
  return {
    id: Number(payload[CLAIM_KEYS.id]),
    email: payload[CLAIM_KEYS.email],
    role: payload[CLAIM_KEYS.role],
    departmentId: Number(payload[CLAIM_KEYS.departmentId]),
    firstName: payload[CLAIM_KEYS.firstName],
    lastName: payload[CLAIM_KEYS.lastName],
  }
}

export function AuthProvider({ children }) {
  // sessionStorage instead of localStorage on purpose — each browser tab gets
  // its own storage, so you can be logged in as different users in different
  // tabs. (Note: this also means the cross-tab "storage" event doesn't apply
  // here anymore — that event only ever fires for localStorage changes.)
  const [token, setToken] = useState(() => sessionStorage.getItem("token"))
  const [user, setUser] = useState(() => {
    const existing = sessionStorage.getItem("token")
    return existing ? userFromToken(existing) : null
  })

  const login = useCallback(async (email, password) => {
    const result = await apiFetch("/api/Auth/login", {
      method: "POST",
      body: { email, password },
    })
    sessionStorage.setItem("token", result.token)
    setToken(result.token)
    setUser(userFromToken(result.token))
    return userFromToken(result.token)
  }, [])

  const signup = useCallback(async (payload) => {
    await apiFetch("/api/Auth/register", { method: "POST", body: payload })
  }, [])

  const logout = useCallback(() => {
    sessionStorage.removeItem("token")
    setToken(null)
    setUser(null)
  }, [])

  return (
    <AuthContext.Provider value={{ token, user, login, signup, logout }}>
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth() {
  const context = useContext(AuthContext)
  if (!context) throw new Error("useAuth must be used inside AuthProvider")
  return context
}
