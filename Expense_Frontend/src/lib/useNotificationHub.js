import { useEffect, useRef } from "react"
import * as signalR from "@microsoft/signalr"

const BASE_URL = import.meta.env.VITE_API_BASE_URL

/**
 * Opens one persistent connection to the backend's NotificationHub and calls
 * onNotification(notification) the instant the server pushes a new one —
 * no polling involved. Reconnects automatically if the connection drops.
 */
export function useNotificationHub(token, onNotification) {
  const handlerRef = useRef(onNotification)
  handlerRef.current = onNotification // always call the latest version, avoids stale closures

  useEffect(() => {
    if (!token) return

    const connection = new signalR.HubConnectionBuilder()
      .withUrl(`${BASE_URL}/hubs/notifications`, {
        accessTokenFactory: () => token,
      })
      .withAutomaticReconnect()
      .build()

    connection.on("ReceiveNotification", (notification) => {
      handlerRef.current(notification)
    })

    connection.start().catch((err) => {
      console.error("SignalR connection failed:", err)
    })

    return () => {
      connection.stop()
    }
  }, [token])
}
