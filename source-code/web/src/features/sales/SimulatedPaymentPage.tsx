import { FlaskConical, Loader2 } from 'lucide-react'
import { Link, useParams } from 'react-router'
import { Loadable, RowsSkeleton } from '@/components/async-states'
import { FormError } from '@/components/form-dialog'
import { StatusBadge } from '@/components/status-badge'
import { Alert, AlertDescription, AlertTitle } from '@/components/ui/alert'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardFooter, CardHeader, CardTitle } from '@/components/ui/card'
import { api } from '@/lib/api'
import { formatMoney } from '@/lib/format'
import { useAsync } from '@/lib/use-async'
import { useSubmit } from '@/lib/use-submit'

type Payment = { providerReference: string; orderId: string; amount: number; status: 'Open' | 'Paid' | 'Declined' | 'Refunded' }

/**
 * ADR-004 — the simulated Payment provider's hosted page, used until a real provider is chosen. It stands
 * where the provider's own page would: it is the only place a card is "entered", and it can be left open
 * past the Hold. It exists in Development only.
 */
export function SimulatedPaymentPage() {
  const { reference } = useParams()
  const payment = useAsync(() => api.get<Payment>(`/SimulatedPayments/Details/${reference}`), [reference])
  const submit = useSubmit()

  async function pay(approve: boolean) {
    if (await submit.run(() => api.post('/SimulatedPayments/Complete', { providerReference: reference, approve }))) {
      payment.reload()
    }
  }

  return (
    <div className="mx-auto grid w-full max-w-md gap-4 pt-4 md:pt-10">
      <Alert>
        <FlaskConical />
        <AlertTitle>Simulated payment provider</AlertTitle>
        <AlertDescription>Test mode — this stands in for the real provider's page. No money moves.</AlertDescription>
      </Alert>

      <Loadable state={payment} skeleton={<RowsSkeleton rows={1} height="h-48" />}>
        {(data) => (
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center justify-between text-lg">
                Pay by card
                <StatusBadge status={data.status === 'Open' ? 'Pending payment' : data.status} />
              </CardTitle>
            </CardHeader>
            <CardContent className="grid gap-4">
              <p className="text-3xl font-semibold tabular-nums text-heading">{formatMoney(data.amount)}</p>
              <p className="text-xs text-muted-foreground">Payment reference {data.providerReference}</p>
              <FormError error={submit.error} />

              {data.status === 'Open' && (
                <p className="text-sm text-muted-foreground">
                  Choose a test card. The real page would ask for card details here — they would never reach TecknoworksTickets.
                </p>
              )}
              {data.status === 'Paid' && <p className="text-sm">This payment was approved.</p>}
              {data.status === 'Declined' && <p className="text-sm">This card was declined. Go back to the order to try again.</p>}
              {data.status === 'Refunded' && (
                <p className="text-sm">
                  This payment was approved after the order's hold ran out and its tickets were gone, so the whole payment has been refunded.
                </p>
              )}
            </CardContent>
            <CardFooter className="flex-col items-stretch gap-2">
              {data.status === 'Open' ? (
                <>
                  <Button size="lg" onClick={() => pay(true)} disabled={submit.pending}>
                    {submit.pending && <Loader2 className="animate-spin" />}
                    Pay with test card (approved)
                  </Button>
                  <Button size="lg" variant="outline" onClick={() => pay(false)} disabled={submit.pending}>
                    Pay with test card (declined)
                  </Button>
                </>
              ) : (
                <Button asChild size="lg">
                  <Link to={`/orders/${data.orderId}`}>Back to your order</Link>
                </Button>
              )}
            </CardFooter>
          </Card>
        )}
      </Loadable>
    </div>
  )
}
