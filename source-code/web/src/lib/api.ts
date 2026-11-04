// A thin fetch wrapper for the .NET host. Everything under /api is proxied to it in development.

const TOKEN_KEY = 'tecknoworkstickets.token'

export function getToken(): string | null {
  try {
    return localStorage.getItem(TOKEN_KEY)
  } catch {
    return null
  }
}

export function setToken(token: string | null) {
  try {
    if (token) localStorage.setItem(TOKEN_KEY, token)
    else localStorage.removeItem(TOKEN_KEY)
  } catch {
    // Storage can be unavailable (private windows); the session then lasts until the page closes.
  }
}

/** Called when the host says the token is no longer good, so the app can sign the person out. */
let onUnauthorized: (() => void) | undefined
export function setUnauthorizedHandler(handler: (() => void) | undefined) {
  onUnauthorized = handler
}

export class ApiError extends Error {
  readonly status: number
  /** Messages for single fields, keyed by lower-case field name ("email", "password", ...). */
  readonly fieldErrors: Record<string, string>

  constructor(status: number, message: string, fieldErrors: Record<string, string> = {}) {
    super(message)
    this.status = status
    this.fieldErrors = fieldErrors
  }
}

function toApiError(status: number, body: unknown): ApiError {
  const fieldErrors: Record<string, string> = {}
  let message = ''

  if (Array.isArray(body)) {
    // The host answers refusals as a list of plain messages.
    message = body.map(String).join(' ')
  } else if (body && typeof body === 'object') {
    const details = body as { errors?: Record<string, string[] | string>; detail?: string; title?: string }

    if (details.errors) {
      for (const [field, messages] of Object.entries(details.errors)) {
        fieldErrors[field.toLowerCase()] = Array.isArray(messages) ? messages[0] : String(messages)
      }
      message = Object.values(fieldErrors)[0] ?? ''
    } else {
      message = details.detail ?? details.title ?? ''
    }
  }

  if (status === 404) {
    message = "We couldn't find that. It may not exist, or it may not be yours to see."
  } else if (status === 401) {
    message = 'Sign in to continue.'
  } else if (status >= 500) {
    message = 'Something went wrong on our side. Try again in a moment.'
  } else if (!message) {
    message = 'That did not work. Check what you entered and try again.'
  }

  return new ApiError(status, message, fieldErrors)
}

async function request<T>(method: string, path: string, body?: unknown): Promise<T> {
  const headers: Record<string, string> = {}
  if (body !== undefined) headers['Content-Type'] = 'application/json'

  const token = getToken()
  if (token) headers.Authorization = `Bearer ${token}`

  let response: Response
  try {
    response = await fetch(`/api${path}`, {
      method,
      headers,
      body: body === undefined ? undefined : JSON.stringify(body),
    })
  } catch {
    throw new ApiError(0, "Can't reach the server. Check your connection and try again.")
  }

  if (response.status === 401 && token) onUnauthorized?.()

  const text = await response.text()
  let data: unknown
  try {
    data = text ? JSON.parse(text) : undefined
  } catch {
    data = undefined
  }

  if (!response.ok) throw toApiError(response.status, data)

  return data as T
}

export const api = {
  get: <T>(path: string) => request<T>('GET', path),
  post: <T = void>(path: string, body?: unknown) => request<T>('POST', path, body ?? {}),
}

/** Fetches a picture with the person's token, for Draft Events whose pictures only their Team may see. */
export async function fetchImageUrl(imageId: string): Promise<string> {
  const token = getToken()
  const response = await fetch(`/api/WhatsOn/Image/${imageId}`, {
    headers: token ? { Authorization: `Bearer ${token}` } : {},
  })

  if (!response.ok) throw new ApiError(response.status, "That picture isn't available.")

  return URL.createObjectURL(await response.blob())
}
