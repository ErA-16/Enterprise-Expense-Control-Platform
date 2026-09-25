// Decodes the payload of a JWT so the frontend can read the role/id/department
// claims your JwtService put in it. This does NOT verify the signature — that's
// the backend's job on every request. The frontend only reads it for routing/display.
export function decodeJwt(token) {
  try {
    const payload = token.split(".")[1]
    const normalized = payload.replace(/-/g, "+").replace(/_/g, "/")
    const padded = normalized.padEnd(
      normalized.length + ((4 - (normalized.length % 4)) % 4),
      "="
    )
    const json = decodeURIComponent(
      atob(padded)
        .split("")
        .map((c) => "%" + c.charCodeAt(0).toString(16).padStart(2, "0"))
        .join("")
    )
    return JSON.parse(json)
  } catch {
    return null
  }
}
