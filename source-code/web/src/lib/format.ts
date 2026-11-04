const money = new Intl.NumberFormat('en-IE', { style: 'currency', currency: 'EUR' })

/** €28.00 — every amount on the platform is in euros to two decimals. */
export function formatMoney(amount: number): string {
  return money.format(amount)
}

/** "2026-12-04" → "Fri 4 Dec 2026". Dates are local at the Venue, so no time zone is applied. */
export function formatDate(date: string): string {
  const [year, month, day] = date.split('T')[0].split('-').map(Number)

  return new Date(year, month - 1, day).toLocaleDateString('en-GB', {
    weekday: 'short',
    day: 'numeric',
    month: 'short',
    year: 'numeric',
  })
}

/** "19:30:00" → "19:30". */
export function formatTime(time: string): string {
  return time.slice(0, 5)
}

/** A moment the host sent in UTC, shown in the viewer's own time. */
export function formatMoment(utc: string): string {
  return new Date(utc).toLocaleString('en-GB', {
    day: 'numeric',
    month: 'short',
    hour: '2-digit',
    minute: '2-digit',
  })
}

export function formatClock(utc: string): string {
  return new Date(utc).toLocaleTimeString('en-GB', { hour: '2-digit', minute: '2-digit', second: '2-digit' })
}

/** Date and Start time of a Performance that the host sends as one local date-time. */
export function formatLocalDateTime(local: string): string {
  const [date, time] = local.split('T')

  return `${formatDate(date)}, ${formatTime(time)}`
}
