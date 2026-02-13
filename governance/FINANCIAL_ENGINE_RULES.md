# MarcoERP – Financial Engine Rules

**Deliverable 4: Posting Workflow, Journal Integrity, Fiscal Year/Period Lock, VAT Responsibility**

---

## 1. Document Scope

This document defines the operational rules for the financial engine — the processing pipeline that ensures every financial transaction is valid, balanced, authorized, and permanently recorded.

---

## 2. Posting Workflow Definition

### 2.1 Transaction Lifecycle

Every financial transaction in MarcoERP passes through a strict lifecycle:

```
┌──────────┐     ┌───────────┐     ┌──────────┐     ┌──────────────┐
│  CREATE   │────►│  VALIDATE │────►│   POST   │────►│  IMMUTABLE   │
│  (Draft)  │     │  (Draft)  │     │ (Posted) │     │  (Permanent) │
└──────────┘     └───────────┘     └──────────┘     └──────────────┘
      │                │                                     │
      │  Can edit      │  Can edit                           │  Cannot edit
      │  Can delete    │  Can delete                         │  Can only reverse
      │  Can renumber  │  Cannot renumber                    │  Can only adjust
      ▼                ▼                                     ▼
   [DRAFT STATE]   [VALIDATED STATE]                   [POSTED STATE]
```

### 2.2 Status Definitions

| Status    | Code | Description                                              | Editable | Deletable |
|-----------|------|----------------------------------------------------------|----------|-----------|
| Draft     | 0    | Created but not yet validated or posted                  | Yes      | Yes       |
| Posted    | 1    | Validated and permanently recorded                       | No       | No        |
| Reversed  | 2    | A reversal entry has been posted to cancel this entry    | No       | No        |

### 2.3 Posting Workflow Steps

The posting process executes the following steps **atomically** (all or nothing):

| Step | Action                                                              | Layer        |
|------|---------------------------------------------------------------------|--------------|
| 1    | **User initiates Post** — clicks Post button on draft document      | WpfUI        |
| 2    | **Confirmation dialog** — shows summary, asks for confirmation      | WpfUI        |
| 3    | **Load draft entity** — retrieve from database with all lines       | Application  |
| 4    | **Period check** — verify posting date falls in an open period      | Application  |
| 5    | **Fiscal year check** — verify the fiscal year is active            | Application  |
| 6    | **Balance check** — verify Total Debits = Total Credits             | Domain       |
| 7    | **Account validation** — all accounts exist, are active, allow posting | Domain    |
| 8    | **Business rule check** — domain-specific rules (no negative cash, etc.) | Domain   |
| 9    | **Authorization check** — user has permission to post this type     | Application  |
| 10   | **Assign posting metadata** — PostedAt, PostedBy, Status = Posted   | Application  |
| 11   | **Generate final number** — sequential, unique journal number       | Application  |
| 12   | **Persist** — save all changes in a single transaction              | Persistence  |
| 13   | **Audit log** — record the posting action with all details          | Infrastructure|
| 14   | **Return result** — success with posted number, or failure with reasons | Application|
| 15   | **Display result** — show success or error to user                  | WpfUI        |

### 2.4 Posting Failure Handling

If **any** step fails:

| Failure Point        | Action                                               |
|----------------------|------------------------------------------------------|
| Period closed         | Reject with message: "Period {month/year} is closed" |
| Fiscal year inactive  | Reject with message: "Fiscal year is not active"     |
| Balance mismatch      | Reject with message: "Debit/Credit mismatch: {diff}" |
| Invalid account       | Reject with message: "{Account} is invalid/inactive" |
| Business rule fail    | Reject with specific rule violation message          |
| Unauthorized          | Reject with message: "Insufficient permissions"      |
| Database error        | Rollback entire transaction, log error, show generic message |

**No partial posting.** Either everything succeeds or nothing changes.

---

## 3. Journal Integrity Rules

### 3.1 Structural Integrity

| Rule ID | Rule                                                                     |
|---------|--------------------------------------------------------------------------|
| JNL-01  | Every journal entry must have at least 2 lines.                          |
| JNL-02  | Every journal entry must balance: `SUM(Debit) = SUM(Credit)`.           |
| JNL-03  | No journal line may have both Debit > 0 and Credit > 0.                 |
| JNL-04  | No journal line may have both Debit = 0 and Credit = 0.                 |
| JNL-05  | Journal number is unique within a fiscal year.                           |
| JNL-06  | Journal numbers are sequential and unique for posted entries. Gaps may occur if posting fails after number assignment.|
| JNL-07  | Journal date must fall within the fiscal year boundaries.                |
| JNL-08  | Journal date must fall within an open (unlocked) period.                 |

### 3.2 Referential Integrity

| Rule ID | Rule                                                                     |
|---------|--------------------------------------------------------------------------|
| JNL-09  | Every journal line must reference a valid, active account.               |
| JNL-10  | Account must allow posting (not a parent/group account).                 |
| JNL-11  | Source document reference must be valid if provided.                      |
| JNL-12  | Fiscal year reference must match the journal date.                       |
| JNL-13  | Period reference must match the journal date's month.                    |
| JNL-14  | Warehouse reference (if provided) must be a valid, active warehouse.     |
| JNL-15  | Cost center reference (if provided) must be valid and active.            |

### 3.3 Behavioral Integrity

| Rule ID | Rule                                                                     |
|---------|--------------------------------------------------------------------------|
| JNL-16  | Draft journals can be edited freely.                                     |
| JNL-17  | Posted journals are **completely immutable**.                             |
| JNL-18  | Reversed journals retain all original data — the reversal is a new entry.|
| JNL-19  | Auto-generated journals (from invoices, inventory) follow same rules.    |
| JNL-20  | Manual journals and auto-generated journals are distinguished by SourceType. |

---

## 4. Fiscal Year Rules

### 4.1 Fiscal Year Lifecycle

```
┌──────────┐     ┌──────────┐     ┌──────────────┐
│  CREATE   │────►│  ACTIVE  │────►│   CLOSED     │
│  (Setup)  │     │ (Current)│     │ (Permanent)  │
└──────────┘     └──────────┘     └──────────────┘
                      │
                      │ Only one fiscal year
                      │ can be ACTIVE at a time
                      ▼
```

### 4.2 Fiscal Year Rules

| Rule ID | Rule                                                                     |
|---------|--------------------------------------------------------------------------|
| FY-00   | Fiscal year is ALWAYS a calendar year: January 1 to December 31.         |
| FY-01   | Start date is always January 1. End date is always December 31.          |
| FY-02   | Only **one** fiscal year can be in Active status at any time.            |
| FY-03   | A fiscal year has exactly 12 monthly periods.                            |
| FY-04   | Fiscal year dates cannot overlap with another fiscal year.               |
| FY-05   | A new fiscal year can only be created after (or during) the current one. |
| FY-06   | Fiscal year closure requires all periods to be locked first.             |
| FY-07   | Fiscal year closure requires a trial balance check (Debits = Credits).   |
| FY-08   | Fiscal year closure generates closing entries automatically:             |
|         | - Revenue and Expense accounts closed to Retained Earnings              |
|         | - Balance sheet accounts carried forward to next year                   |
| FY-09   | Fiscal year closure is **irreversible**.                                 |
| FY-10   | After closure, no posting allowed to any period in that fiscal year.     |
| FY-11   | Opening balances for the new year are derived from the closing balances. |

### 4.3 Fiscal Year Closure Checklist

| Step | Action                                                              |
|------|---------------------------------------------------------------------|
| 1    | Verify all 12 periods are locked                                    |
| 2    | Verify trial balance is balanced                                    |
| 3    | Verify all draft transactions are either posted or deleted          |
| 4    | Generate closing entries (Revenue/Expense → Retained Earnings)      |
| 5    | Post closing entries                                                |
| 6    | Mark fiscal year as Closed                                          |
| 7    | Create opening balances in the new fiscal year                      |
| 8    | Audit log the closure event with full detail                        |

---

## 5. Period Lock Policy

### 5.1 Period Structure

| Property       | Value                                              |
|----------------|----------------------------------------------------|
| Periods per Year | 12 (monthly)                                     |
| Period Format  | YYYY-MM (e.g., 2026-01)                            |
| Lock Granularity | Per month                                        |

### 5.2 Period Status

| Status   | Description                                      | Posting Allowed |
|----------|--------------------------------------------------|-----------------|
| Open     | Normal operations, transactions can be posted    | Yes             |
| Locked   | Period closed, no new postings allowed            | No              |

### 5.3 Period Lock Rules

| Rule ID | Rule                                                                     |
|---------|--------------------------------------------------------------------------|
| PER-01  | Periods are locked sequentially — cannot lock March before February.     |
| PER-02  | Locking a period requires all drafts in that period to be resolved.      |
| PER-03  | Resolved means: posted, deleted, or moved to a different period.         |
| PER-04  | Period lock is performed by an authorized user (Accountant or Admin).    |
| PER-05  | Period unlock is allowed **only** by Admin, and only for the most recent locked period. |
| PER-06  | Period unlock is logged in the audit trail with justification required.  |
| PER-07  | Bulk period lock (lock all remaining open periods) is available for year-end. |
| PER-08  | A posting attempt to a locked period is rejected at the Application layer.|
| PER-09  | The error message includes which period is locked and who to contact.    |

### 5.4 Period Lock Verification Point

```
Application Service: PostJournal()
  └── Check: journalDate falls within which period?
       └── Is that period Open?
            ├── Yes → continue posting workflow
            └── No  → REJECT: "Cannot post to locked period {YYYY-MM}"
```

---

## 6. Automatic Journal Generation

Certain business events generate journal entries automatically:

| Source Event           | Generated Journal                                    | Auto-Post? |
|------------------------|------------------------------------------------------|------------|
| Sales Invoice Posted   | Debit AR, Credit Revenue, Credit VAT Output          | Yes        |
| Purchase Invoice Posted| Debit Expense/Inventory, Debit VAT Input, Credit AP  | Yes        |
| Cash Receipt           | Debit Cash, Credit AR (or Revenue)                   | Yes        |
| Cash Payment           | Debit AP (or Expense), Credit Cash                   | Yes        |
| Inventory Receipt      | Debit Inventory, Credit Goods in Transit/AP           | Yes        |
| Inventory Delivery     | Debit COGS, Credit Inventory                         | Yes        |
| Warehouse Transfer     | Debit Dest Inventory, Credit Source Inventory         | Yes        |
| Inventory Adjustment   | Debit/Credit Inventory, Credit/Debit Adjustment Acct | Yes        |

### Auto-Generation Rules

| Rule ID | Rule                                                                     |
|---------|--------------------------------------------------------------------------|
| AGJ-01  | Auto-generated journals follow all the same rules as manual journals.    |
| AGJ-02  | Auto-generated journals are linked to their source document.             |
| AGJ-03  | Auto-generated journals carry the SourceType to identify their origin.   |
| AGJ-04  | Auto-generated journals are posted in the same transaction as the source.|
| AGJ-05  | If auto-generation fails, the source posting also fails (atomic).        |
| AGJ-06  | Auto-generated journal numbers follow the same sequential pattern.       |

---

## 7. VAT Calculation Responsibility

| Responsibility                   | Layer        | Justification                          |
|----------------------------------|--------------|----------------------------------------|
| VAT rate lookup                  | Domain       | Business data, part of entity logic    |
| VAT amount calculation           | Application  | Centralized in `ILineCalculationService` — pure math with no infrastructure deps. Keeps calculation testable and co-located with line/invoice total orchestration. |
| VAT line generation in journal   | Application  | Orchestration of journal creation      |
| VAT account mapping              | Domain       | Business rule: which account gets VAT  |
| VAT report data aggregation      | Application  | Query orchestration                    |
| VAT display formatting           | WpfUI        | Presentation concern                   |

> **Design Note:** Although VAT calculation is pure arithmetic (could live in Domain), it is placed in `Application/Services/Common/LineCalculationService` alongside discount, profit, and unit conversion calculations. This centralizes ALL line math in one service per governance rule DEV-15. The service has zero infrastructure dependencies and is fully unit-testable.

---

## 8. Balance Verification Checkpoints

Balance verification happens at multiple points:

| Checkpoint            | When                                  | Action on Failure           |
|-----------------------|---------------------------------------|-----------------------------|
| DTO Validation        | Before saving draft                   | Warn user, allow save anyway|
| Domain Validation     | Before posting                        | Block posting               |
| Pre-Persist Check     | Application service, before save      | Block save, return error    |
| Database Constraint   | On INSERT/UPDATE (check constraint)   | Throw exception, rollback   |
| Trial Balance Report  | On demand                             | Show discrepancy report     |

---

## Version History

| Version | Date       | Change Description                    |
|---------|------------|---------------------------------------|
| 1.0     | 2026-02-08 | Initial Phase 1 governance release    |
| 1.1     | 2026-02-13 | Clarified §7: VAT calculation layer is Application (ILineCalculationService), not Domain. Added design note. |
