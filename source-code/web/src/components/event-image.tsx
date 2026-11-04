import { ImageOff } from 'lucide-react'
import { useEffect, useState } from 'react'
import { fetchImageUrl } from '@/lib/api'
import { cn } from '@/lib/utils'

/**
 * An Event's picture (EVT-F6). The public sees Published Events' pictures straight from the URL; a Draft's
 * picture is only for its Team, so `authenticated` fetches it with the person's token instead.
 */
export function EventImage({
  imageId,
  authenticated = false,
  alt,
  className,
}: {
  imageId: string | null | undefined
  authenticated?: boolean
  alt: string
  className?: string
}) {
  const [blobUrl, setBlobUrl] = useState<string>()
  const [failed, setFailed] = useState(false)

  useEffect(() => {
    if (!imageId || !authenticated) return

    let url: string | undefined
    let current = true
    setFailed(false)

    fetchImageUrl(imageId)
      .then((created) => {
        url = created
        if (current) setBlobUrl(created)
      })
      .catch(() => current && setFailed(true))

    return () => {
      current = false
      if (url) URL.revokeObjectURL(url)
    }
  }, [imageId, authenticated])

  const src = authenticated ? blobUrl : imageId ? `/api/WhatsOn/Image/${imageId}` : undefined

  if (!imageId || failed || (!src && !authenticated)) {
    return (
      <div className={cn('flex items-center justify-center bg-muted text-muted-foreground', className)} role="img" aria-label={alt}>
        <ImageOff className="size-6" />
      </div>
    )
  }

  if (!src) return <div className={cn('animate-pulse bg-muted', className)} aria-hidden="true" />

  return <img src={src} alt={alt} className={cn('object-cover', className)} onError={() => setFailed(true)} />
}
