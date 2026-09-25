import { Routes, Route, Navigate } from "react-router-dom"
import { useAuth } from "./context/AuthContext"
import ProtectedRoute from "./components/ProtectedRoute"
import Layout from "./components/Layout"
import AuthPage from "./pages/AuthPage"
import EmployeeDashboard from "./pages/EmployeeDashboard"
import ManagerDashboard from "./pages/ManagerDashboard"
import FinanceDashboard from "./pages/FinanceDashboard"
import Reports from "./pages/Reports"

const ROLE_HOME = {
  Employee: "/employee",
  Manager: "/employee",
  Finance: "/finance",
  Admin: "/reports",
}

function Home() {
  const { user } = useAuth()
  if (!user) return <Navigate to="/login" replace />
  return <Navigate to={ROLE_HOME[user.role] ?? "/login"} replace />
}

export default function App() {
  return (
    <Routes>
      <Route path="/login" element={<AuthPage />} />
      <Route path="/" element={<Home />} />

      <Route
        path="/employee"
        element={
          <ProtectedRoute roles={["Employee", "Manager"]}>
            <Layout>
              <EmployeeDashboard />
            </Layout>
          </ProtectedRoute>
        }
      />

      <Route
        path="/manager"
        element={
          <ProtectedRoute roles={["Manager"]}>
            <Layout>
              <ManagerDashboard />
            </Layout>
          </ProtectedRoute>
        }
      />

      <Route
        path="/finance"
        element={
          <ProtectedRoute roles={["Finance"]}>
            <Layout>
              <FinanceDashboard />
            </Layout>
          </ProtectedRoute>
        }
      />

      <Route
        path="/reports"
        element={
          <ProtectedRoute roles={["Manager", "Finance", "Admin"]}>
            <Layout>
              <Reports />
            </Layout>
          </ProtectedRoute>
        }
      />

      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  )
}
