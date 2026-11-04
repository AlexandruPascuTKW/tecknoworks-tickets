// Shapes the Events endpoints answer with (EVT-V1 to EVT-V5).

export type Venue = { id: string; venueName: string; address: string; capacity: number }

export type PerformanceDto = { id: string; date: string; doorsTime: string; startTime: string }

export type TicketTypeDto = { id: string; ticketTypeName: string; price: number; quantity: number; onSale: boolean }

/** EVT-V2 */
export type OrganizerEvent = {
  id: string
  eventName: string
  venueName: string
  eventStatus: 'Draft' | 'Published'
  performanceDates: string[]
}

/** EVT-V3 */
export type EventTeam = {
  id: string
  organizerId: string
  eventName: string
  description: string | null
  imageId: string | null
  venueId: string
  venueName: string
  address: string
  capacity: number
  eventStatus: 'Draft' | 'Published'
  performances: PerformanceDto[]
  ticketTypes: TicketTypeDto[]
}

/** EVT-V4 */
export type WhatsOnItem = {
  id: string
  eventName: string
  imageId: string | null
  venueName: string
  performanceDates: string[]
}

/** EVT-V5 */
export type PublicEvent = {
  id: string
  eventName: string
  description: string | null
  imageId: string | null
  venueName: string
  address: string
  performances: PerformanceDto[]
  ticketTypes: { id: string; ticketTypeName: string; price: number }[]
}
