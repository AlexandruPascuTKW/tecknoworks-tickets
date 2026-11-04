// A sliver of the lending front end: just enough for sdlc-scenario-manager's check_labels.py to
// find the labels the example scenarios (scenarios/LND/lending.md) quote.

const MAX_RENEWALS = 2;
const STATUS = { Active: "Active", Overdue: "Overdue", Returned: "Returned", Lost: "Lost" };
const FINE_STATUS = { Outstanding: "Outstanding", Paid: "Paid", Waived: "Waived" };

export function LoanDesk({ member, loans, fines, canWaive }) {
  return (
    <main>
      <h1>Loan desk</h1>
      <label>Member card <input name="card" /></label>
      <button>Find member</button>
      {member && member.outstanding === 0 && <p>No outstanding fines</p>}

      <label>Item barcode <input name="lend" /></label>
      <button>Lend</button>
      <Toast ok="Item lent" />

      <label>Return item <input name="return" /></label>
      <button>Check in</button>
      <Toast ok="Item returned" />
      <Toast ok="Set aside for hold" detail="Ready" />

      <Tabs tabs={["Loans", "Fines"]} />
      {loans.map((loan) => (
        <Row key={loan.id} status={STATUS[loan.status]}>
          <span>Renewals: {loan.renewals} of {MAX_RENEWALS}</span>
          <button>Renew</button>
        </Row>
      ))}
      <Toast ok="Loan renewed" refused="Renewal refused: another member is waiting for this title" />

      {fines.map((fine) => (
        <Row key={fine.id} status={FINE_STATUS[fine.status]}>
          {canWaive && <button>Waive fine</button>}
        </Row>
      ))}
    </main>
  );
}

export function MyLoans() {
  return <h1>My loans</h1>;
}
