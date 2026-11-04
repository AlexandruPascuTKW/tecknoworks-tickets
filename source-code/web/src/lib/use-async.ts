import { useCallback, useEffect, useState } from 'react'
import { ApiError } from '@/lib/api'

export type AsyncState<T> = {
  data: T | undefined
  error: ApiError | undefined
  loading: boolean
  reload: () => void
}

/** Loads something when the component appears (and again when `deps` change or `reload()` is called). */
export function useAsync<T>(load: () => Promise<T>, deps: unknown[]): AsyncState<T> {
  const [data, setData] = useState<T>()
  const [error, setError] = useState<ApiError>()
  const [loading, setLoading] = useState(true)
  const [tick, setTick] = useState(0)

  useEffect(() => {
    let current = true
    setLoading(true)

    load()
      .then((result) => {
        if (!current) return
        setData(result)
        setError(undefined)
      })
      .catch((e: unknown) => {
        if (current) setError(e instanceof ApiError ? e : new ApiError(0, 'Something went wrong.'))
      })
      .finally(() => {
        if (current) setLoading(false)
      })

    return () => {
      current = false
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [...deps, tick])

  const reload = useCallback(() => setTick((t) => t + 1), [])

  return { data, error, loading, reload }
}
