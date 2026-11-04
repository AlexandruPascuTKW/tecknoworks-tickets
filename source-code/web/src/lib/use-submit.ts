import { useState } from 'react'
import { ApiError } from '@/lib/api'

/** Runs a form's action, tracking whether it is pending and what went wrong (inline and as a summary). */
export function useSubmit() {
  const [pending, setPending] = useState(false)
  const [error, setError] = useState<ApiError>()

  /** Resolves true when the action worked. */
  async function run(action: () => Promise<unknown>): Promise<boolean> {
    setPending(true)
    setError(undefined)

    try {
      await action()
      return true
    } catch (e) {
      setError(e instanceof ApiError ? e : new ApiError(0, 'Something went wrong. Try again.'))
      return false
    } finally {
      setPending(false)
    }
  }

  return {
    run,
    pending,
    error,
    /** The inline message for one field, by its lower-case name. */
    fieldError: (field: string) => error?.fieldErrors[field],
    clear: () => setError(undefined),
  }
}

/** A picture chosen in a form, as the base64 the host expects (ADR-008: JPEG, PNG or WebP, up to 2 MB). */
export async function readImage(file: File): Promise<{ content: string }> {
  if (file.size > 2 * 1024 * 1024) {
    throw new ApiError(400, 'Image must be up to 2 MB.', { image: 'Image must be up to 2 MB.' })
  }

  const dataUrl = await new Promise<string>((resolve, reject) => {
    const reader = new FileReader()
    reader.onload = () => resolve(String(reader.result))
    reader.onerror = () => reject(new ApiError(400, "That file couldn't be read."))
    reader.readAsDataURL(file)
  })

  return { content: dataUrl.slice(dataUrl.indexOf(',') + 1) }
}
