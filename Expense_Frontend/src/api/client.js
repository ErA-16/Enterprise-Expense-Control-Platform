const BASE_URL = import.meta.env.VITE_API_BASE_URL

function getToken() {
  return sessionStorage.getItem("token")
}

// Your backend returns errors in a couple of different shapes depending on the
// endpoint — sometimes a raw string (BadRequest("...")), sometimes { message }.
// This pulls a readable message out of either.
async function extractErrorMessage(response) {
  const contentType = response.headers.get("content-type") || ""
  if (contentType.includes("application/json")) {
    const body = await response.json().catch(() => null)
    if (typeof body === "string") return body
    if (body?.message) return body.message
    if (body?.Message) return body.Message
    if (body?.title) return body.title // ASP.NET Core validation problem details
  } else {
    const text = await response.text().catch(() => "")
    if (text) return text
  }
  return `Request failed (${response.status})`
}

/**
 * @param {string} path - e.g. "/api/expense/myrequests"
 * @param {object} options
 * @param {"GET"|"POST"|"PATCH"|"DELETE"} [options.method]
 * @param {object} [options.body] - plain object, sent as JSON
 * @param {FormData} [options.formData] - sent as multipart/form-data instead of JSON
 */
export async function apiFetch(path, { method = "GET", body, formData } = {}) {
  const headers = {}
  const token = getToken()
  if (token) headers["Authorization"] = `Bearer ${token}`

  let requestBody
  if (formData) {
    requestBody = formData // browser sets the multipart Content-Type + boundary itself
  } else if (body !== undefined) {
    headers["Content-Type"] = "application/json"
    requestBody = JSON.stringify(body)
  }

  const response = await fetch(`${BASE_URL}${path}`, {
    method,
    headers,
    body: requestBody,
  })

  if (!response.ok) {
    throw new Error(await extractErrorMessage(response))
  }

  if (response.status === 204) return null // NoContent
  const contentType = response.headers.get("content-type") || ""
  if (contentType.includes("application/json")) return response.json()
  return null
}
