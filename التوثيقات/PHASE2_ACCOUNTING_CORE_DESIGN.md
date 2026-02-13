# MarcoERP — Phase 2: Accounting Core Design

## Architectural Design Document — Accounting Engine Structure

**Phase:** 2 of 6  
**Scope:** Accounting Core Design ONLY (no UI, no EF, no SQL, no migrations)  
**Date:** 2026-02-08  
**Status:** Design Phase  
**Depends On:** Phase 1 Governance (LOCKED)

---

## Document Purpose

This document defines the **logical architecture** of the MarcoERP accounting engine. It describes entities, relationships, rules, workflows, and interaction patterns at the design level. No implementation code is produced in this phase.

All decisions herein are validated against the Phase 1 governance documents:

- `ACCOUNTING_PRINCIPLES.md`
- `FINANCIAL_ENGINE_RULES.md`
- `ARCHITECTURE.md`
- `DATABASE_POLICY.md`
- `RECORD_PROTECTION_POLICY.md`
- `SECURITY_POLICY.md`
- `SOLUTION_STRUCTURE.md`
- `RISK_PREVENTION_FRAMEWORK.md`

---

## SECTION 1 — Chart of Accounts Structure Design

## 1.1 Numbering System

MarcoERP uses **Egyptian-style hierarchical numbering**. The account code is a numeric string whose structure encodes the hierarchy position:

```text
Level 1 (Category):     1000             ← 4 digits, thousands
Level 2 (Group):        1100             ← 4 digits, hundreds under parent
Level 3 (Sub-group):    1110             ← 4 digits, tens under parent
Level 4 (Leaf):         1111             ← 4 digits, units under parent

```

### Numbering Ranges by Account Type

| Account Type              | Level 1 Code | Range       |
| ------------------------- | ------------ | ----------- |
| Assets                    | 1000         | 1000 – 1999 |
| Liabilities               | 2000         | 2000 – 2999 |
| Equity                    | 3000         | 3000 – 3999 |
| Revenue                   | 4000         | 4000 – 4999 |
| Cost of Goods Sold (COGS) | 5000         | 5000 – 5999 |
| Expenses                  | 6000         | 6000 – 6999 |
| Other Income              | 7000         | 7000 – 7999 |
| Other Expenses            | 8000         | 8000 – 8999 |

### Hierarchy Example

```text
1000  Assets                          (Level 1 — Category)
├── 1100  Current Assets              (Level 2 — Group)
│   ├── 1110  Cash and Banks          (Level 3 — Sub-group)
│   │   ├── 1111  Main Cash           (Level 4 — Leaf ✓ POSTING)
│   │   ├── 1112  Petty Cash          (Level 4 — Leaf ✓ POSTING)
│   │   └── 1113  Bank Account        (Level 4 — Leaf ✓ POSTING)
│   ├── 1120  Accounts Receivable     (Level 3 — Sub-group / Control)
│   │   ├── 1121  AR - Trade          (Level 4 — Leaf ✓ POSTING)
│   │   └── 1122  AR - Other          (Level 4 — Leaf ✓ POSTING)
│   └── 1130  Inventory               (Level 3 — Sub-group / Control)
│       ├── 1131  Inventory - WH01    (Level 4 — Leaf ✓ POSTING)
│       └── 1132  Inventory - WH02    (Level 4 — Leaf ✓ POSTING)
├── 1200  Non-Current Assets          (Level 2 — Group)
│   ├── 1210  Fixed Assets            (Level 3 — Sub-group)
│   │   ├── 1211  Equipment           (Level 4 — Leaf ✓ POSTING)
│   │   └── 1212  Furniture           (Level 4 — Leaf ✓ POSTING)
│   └── 1220  Accumulated Depreciation(Level 3 — Sub-group)
│       ├── 1221  Acc Dep - Equipment  (Level 4 — Leaf ✓ POSTING)
│       └── 1222  Acc Dep - Furniture  (Level 4 — Leaf ✓ POSTING)

```

### Code Derivation Logic

The parent code of any account is derived by structure, not stored lookup:

| Account Code | Derived Parent | Rule                                        |
| ------------ | -------------- | ------------------------------------------- |
| 1111         | 1110           | Replace last non-zero digit group with zero |
| 1110         | 1100           | Replace last non-zero digit group with zero |
| 1100         | 1000           | Replace last non-zero digit group with zero |
| 1000         | (none — root)  | Level 1 accounts have no parent             |

**Validation:** A child's code must start with the parent's significant digits.  
`1131` is valid under `1130`. `1231` is NOT valid under `1130`.

## 1.2 Account Entity Structure

| Property        | Type         | Required | Description                                                                 |
| --------------- | ------------ | -------- | --------------------------------------------------------------------------- |
| Id              | int          | Yes      | Primary key (identity)                                                      |
| AccountCode     | string(4)    | Yes      | Unique 4-digit numeric code (e.g., "1111")                                  |
| AccountNameAr   | string(200)  | Yes      | Arabic account name                                                         |
| AccountNameEn   | string(200)  | No       | English account name (optional)                                             |
| AccountType     | enum         | Yes      | Asset, Liability, Equity, Revenue, COGS, Expense, OtherIncome, OtherExpense |
| NormalBalance   | enum         | Yes      | Debit or Credit (derived from AccountType, immutable)                       |
| ParentAccountId | int?         | No       | FK to parent Account (null for Level 1)                                     |
| Level           | int          | Yes      | Hierarchy depth: 1, 2, 3, or 4                                              |
| IsLeaf          | bool         | Yes      | True if no children exist — only leaves allow posting                       |
| AllowPosting    | bool         | Yes      | True only for leaf accounts                                                 |
| IsActive        | bool         | Yes      | False = deactivated, excluded from new transactions                         |
| IsSystemAccount | bool         | Yes      | True = locked, cannot be deleted or have type changed                       |
| CurrencyCode    | string(3)    | Yes      | ISO 4217, system-wide single currency                                       |
| Description     | string(500)  | No       | Optional description                                                        |
| CreatedAt       | datetime     | Yes      | UTC timestamp                                                               |
| CreatedBy       | string(100)  | Yes      | Creating user                                                               |
| ModifiedAt      | datetime?    | No       | UTC timestamp                                                               |
| ModifiedBy      | string(100)? | No       | Modifying user                                                              |
| RowVersion      | byte[]       | Yes      | Optimistic concurrency token                                                |
| IsDeleted       | bool         | Yes      | Soft delete flag                                                            |
| DeletedAt       | datetime?    | No       | UTC deletion timestamp                                                      |
| DeletedBy       | string(100)? | No       | Deleting user                                                               |

### Account Entity Invariants (Domain-Enforced)

| Invariant ID | Rule                                                                       |
| ------------ | -------------------------------------------------------------------------- |
| ACC-INV-01   | AccountCode must be exactly 4 numeric characters.                          |
| ACC-INV-02   | AccountType cannot change once any posting references this account.        |
| ACC-INV-03   | NormalBalance is always derived from AccountType, never set independently. |
| ACC-INV-04   | Level must be between 1 and 4 inclusive.                                   |
| ACC-INV-05   | If Level = 1, ParentAccountId must be null.                                |
| ACC-INV-06   | If Level > 1, ParentAccountId must reference a valid parent at Level - 1.  |
| ACC-INV-07   | AllowPosting is true ONLY when IsLeaf = true.                              |
| ACC-INV-08   | IsSystemAccount = true prevents deletion and type modification.            |
| ACC-INV-09   | An account with children cannot be marked as IsLeaf = true.                |
| ACC-INV-10   | An account with posted transactions cannot be deleted — only deactivated.  |
| ACC-INV-11   | Deactivated accounts cannot receive new postings.                          |

## 1.3 System Accounts (Auto-Generated on First Run)

The following accounts are seeded on first application run. They carry `IsSystemAccount = true` and cannot be deleted.

### Assets (1xxx)

| Code | Name                       | Level | Leaf | Notes                       |
| ---- | -------------------------- | ----- | ---- | --------------------------- |
| 1000 | Assets                     | 1     | No   | Root asset category         |
| 1100 | Current Assets             | 2     | No   |                             |
| 1110 | Cash and Banks             | 3     | No   | Parent for cashboxes        |
| 1111 | Main Cash                  | 4     | Yes  | Default cashbox             |
| 1120 | Accounts Receivable        | 3     | No   | AR control account parent   |
| 1121 | AR — Trade Receivables     | 4     | Yes  | Customer control account    |
| 1130 | Inventory                  | 3     | No   | Inventory control parent    |
| 1131 | Inventory — Main Warehouse | 4     | Yes  | Default warehouse inventory |
| 1140 | VAT Input                  | 3     | No   |                             |
| 1141 | VAT Input Receivable       | 4     | Yes  | Purchase VAT debit          |
| 1200 | Non-Current Assets         | 2     | No   |                             |

### Liabilities (2xxx)

| Code | Name                       | Level | Leaf | Notes                     |
| ---- | -------------------------- | ----- | ---- | ------------------------- |
| 2000 | Liabilities                | 1     | No   | Root liability category   |
| 2100 | Current Liabilities        | 2     | No   |                           |
| 2110 | Accounts Payable           | 3     | No   | AP control account parent |
| 2111 | AP — Trade Payables        | 4     | Yes  | Supplier control account  |
| 2120 | VAT Output                 | 3     | No   |                           |
| 2121 | VAT Output Payable         | 4     | Yes  | Sales VAT credit          |
| 2130 | Accrued Expenses           | 3     | No   |                           |
| 2131 | Accrued Expenses — General | 4     | Yes  |                           |

### Equity (3xxx)

| Code | Name                        | Level | Leaf | Notes                 |
| ---- | --------------------------- | ----- | ---- | --------------------- |
| 3000 | Equity                      | 1     | No   | Root equity category  |
| 3100 | Owner's Equity              | 2     | No   |                       |
| 3110 | Paid-in Capital             | 3     | No   |                       |
| 3111 | Capital                     | 4     | Yes  |                       |
| 3120 | Retained Earnings           | 3     | No   |                       |
| 3121 | Retained Earnings — Current | 4     | Yes  | Year-end close target |

### Revenue (4xxx)

| Code | Name                      | Level | Leaf | Notes                 |
| ---- | ------------------------- | ----- | ---- | --------------------- |
| 4000 | Revenue                   | 1     | No   | Root revenue category |
| 4100 | Sales Revenue             | 2     | No   |                       |
| 4110 | Product Sales             | 3     | No   |                       |
| 4111 | Sales — General           | 4     | Yes  |                       |
| 4120 | Sales Returns             | 3     | No   |                       |
| 4121 | Sales Returns — General   | 4     | Yes  | Contra-revenue        |
| 4130 | Sales Discounts           | 3     | No   |                       |
| 4131 | Sales Discounts — General | 4     | Yes  | Contra-revenue        |

### COGS (5xxx)

| Code | Name               | Level | Leaf | Notes                   |
| ---- | ------------------ | ----- | ---- | ----------------------- |
| 5000 | Cost of Goods Sold | 1     | No   | Root COGS category      |
| 5100 | COGS — Products    | 2     | No   |                         |
| 5110 | COGS — Direct      | 3     | No   |                         |
| 5111 | COGS — General     | 4     | Yes  | Auto-generated on sales |

### Expenses (6xxx)

| Code | Name                         | Level | Leaf | Notes                         |
| ---- | ---------------------------- | ----- | ---- | ----------------------------- |
| 6000 | Expenses                     | 1     | No   | Root expense category         |
| 6100 | Operating Expenses           | 2     | No   |                               |
| 6110 | Salaries & Wages             | 3     | No   |                               |
| 6111 | Salaries                     | 4     | Yes  |                               |
| 6120 | Rent                         | 3     | No   |                               |
| 6121 | Office Rent                  | 4     | Yes  |                               |
| 6130 | Utilities                    | 3     | No   |                               |
| 6131 | Utilities — General          | 4     | Yes  |                               |
| 6140 | Rounding Differences         | 3     | No   |                               |
| 6141 | Rounding Account             | 4     | Yes  | Per RND-03                    |
| 6150 | Inventory Adjustments        | 3     | No   |                               |
| 6151 | Inventory Adjustment Account | 4     | Yes  | Gains/losses from stock count |

### Other Income (7xxx)

| Code | Name                   | Level | Leaf | Notes                      |
| ---- | ---------------------- | ----- | ---- | -------------------------- |
| 7000 | Other Income           | 1     | No   | Root other income category |
| 7100 | Miscellaneous Income   | 2     | No   |                            |
| 7110 | Other Income — General | 3     | No   |                            |
| 7111 | Other Income — Misc    | 4     | Yes  |                            |

### Other Expenses (8xxx)

| Code | Name                     | Level | Leaf | Notes                        |
| ---- | ------------------------ | ----- | ---- | ---------------------------- |
| 8000 | Other Expenses           | 1     | No   | Root other expenses category |
| 8100 | Non-Operating Expenses   | 2     | No   |                              |
| 8110 | Other Expenses — General | 3     | No   |                              |
| 8111 | Other Expenses — Misc    | 4     | Yes  |                              |

## 1.4 Chart of Accounts Auto-Seed Rules

| Rule ID | Rule                                                                   |
| ------- | ---------------------------------------------------------------------- |
| SEED-01 | System accounts are generated on first application run only.           |
| SEED-02 | Seed checks for existence before inserting (idempotent).               |
| SEED-03 | All seeded accounts carry `IsSystemAccount = true`.                    |
| SEED-04 | All seeded accounts carry `IsActive = true`.                           |
| SEED-05 | Seed runs inside a transaction — all or nothing.                       |
| SEED-06 | Seed is implemented in the Persistence layer (`Seeds/` folder).        |
| SEED-07 | Users may add additional accounts (Levels 2–4) under system parents.   |
| SEED-08 | Users may NOT delete or rename system accounts.                        |
| SEED-09 | Users may add additional Level 1 categories within the defined ranges. |

---

## SECTION 2 — Account Type & Hierarchy Rules

## 2.1 Account Type Enumeration

| Enum Value | Name         | Code Range | Natural Balance | Increases By | Decreases By |
| ---------- | ------------ | ---------- | --------------- | ------------ | ------------ |
| 0          | Asset        | 1xxx       | Debit           | Debit        | Credit       |
| 1          | Liability    | 2xxx       | Credit          | Credit       | Debit        |
| 2          | Equity       | 3xxx       | Credit          | Credit       | Debit        |
| 3          | Revenue      | 4xxx       | Credit          | Credit       | Debit        |
| 4          | COGS         | 5xxx       | Debit           | Debit        | Credit       |
| 5          | Expense      | 6xxx       | Debit           | Debit        | Credit       |
| 6          | OtherIncome  | 7xxx       | Credit          | Credit       | Debit        |
| 7          | OtherExpense | 8xxx       | Debit           | Debit        | Credit       |

### NormalBalance Derivation Rule

```text
If AccountType ∈ {Asset, COGS, Expense, OtherExpense} → NormalBalance = Debit
If AccountType ∈ {Liability, Equity, Revenue, OtherIncome} → NormalBalance = Credit

```

This is computed in the **Domain entity constructor** and is not independently settable.

## 2.2 Hierarchy Rules

| Rule ID | Rule                                                                                         |
| ------- | -------------------------------------------------------------------------------------------- |
| HIER-01 | Maximum 4 hierarchy levels. No exceptions.                                                   |
| HIER-02 | A child's AccountType MUST match its parent's AccountType.                                   |
| HIER-03 | A child's AccountCode must fall within the parent's numeric range.                           |
| HIER-04 | A parent account automatically has `AllowPosting = false`.                                   |
| HIER-05 | When a leaf account gets its first child, it becomes a parent (AllowPosting flips to false). |
| HIER-06 | If AllowPosting flips from true to false, any existing postings remain valid.                |
| HIER-07 | Removing the last child of a parent does NOT auto-flip it back to leaf.                      |
| HIER-08 | Re-enabling posting on a former parent requires explicit user action.                        |

### Hierarchy Depth Validation

| Level | Parent Requirement                 | AccountCode Pattern |
| ----- | ---------------------------------- | ------------------- |
| 1     | No parent (ParentAccountId = null) | `X000` (e.g., 1000) |
| 2     | Parent must be Level 1             | `XX00` (e.g., 1100) |
| 3     | Parent must be Level 2             | `XXX0` (e.g., 1110) |
| 4     | Parent must be Level 3             | `XXXX` (e.g., 1111) |

## 2.3 Posting Enforcement Rules

| Rule ID | Rule                                                                                    |
| ------- | --------------------------------------------------------------------------------------- |
| POST-01 | **Only leaf accounts with AllowPosting = true can receive journal postings.**           |
| POST-02 | Attempting to post to a parent/group account is rejected at the Domain layer.           |
| POST-03 | Attempting to post to a deactivated account (IsActive = false) is rejected.             |
| POST-04 | The posting validation occurs inside the JournalEntry entity's Validate() method.       |
| POST-05 | Parent accounts display aggregated balances from their children (computed, not stored). |
| POST-06 | Balance queries on parent accounts SUM all descendant leaf balances.                    |

## 2.4 Control Account Pattern

Control accounts are parent accounts that aggregate sub-ledger balances. In MarcoERP:

| Control Account     | Code | Sub-Ledger Entries               |
| ------------------- | ---- | -------------------------------- |
| Accounts Receivable | 1120 | Individual customer transactions |
| Accounts Payable    | 2110 | Individual supplier transactions |
| Inventory           | 1130 | Per-warehouse inventory balances |
| VAT Input           | 1140 | Purchase VAT entries             |
| VAT Output          | 2120 | Sales VAT entries                |

**Design Rule:** Posting happens to the leaf accounts under the control account (e.g., `1121` AR — Trade), never to the control account itself (`1120`). The control account balance is always the sum of its children.

---

## SECTION 3 — Journal Engine Logical Design

## 3.1 JournalEntry Entity Structure

| Property        | Type         | Required | Description                                                                                              |
| --------------- | ------------ | -------- | -------------------------------------------------------------------------------------------------------- |
| Id              | int          | Yes      | Primary key (identity)                                                                                   |
| JournalNumber   | string(20)   | No       | Final sequential code, assigned on posting (null while draft)                                            |
| DraftCode       | string(20)   | Yes      | Temporary code: `DRAFT-{GUID:8}`, replaced on posting                                                    |
| JournalDate     | date         | Yes      | Transaction date (must fall within open period)                                                          |
| PostingDate     | datetime?    | No       | UTC timestamp when posted (null while draft)                                                             |
| Description     | string(500)  | Yes      | Narrative describing the transaction                                                                     |
| ReferenceNumber | string(100)  | No       | External reference (invoice number, etc.)                                                                |
| Status          | enum         | Yes      | Draft = 0, Posted = 1, Reversed = 2                                                                      |
| SourceType      | enum         | Yes      | Manual, SalesInvoice, PurchaseInvoice, CashReceipt, CashPayment, Inventory, Adjustment, Opening, Closing |
| SourceId        | int?         | No       | FK to originating document (null for manual entries)                                                     |
| FiscalYearId    | int          | Yes      | FK to FiscalYear                                                                                         |
| FiscalPeriodId  | int          | Yes      | FK to FiscalPeriod                                                                                       |
| CostCenterId    | int?         | No       | Optional cost center tag                                                                                 |
| ReversedEntryId | int?         | No       | FK to original entry (if this IS a reversal)                                                             |
| ReversalEntryId | int?         | No       | FK to reversal entry (if this WAS reversed)                                                              |
| AdjustedEntryId | int?         | No       | FK to original entry (if this is an adjustment)                                                          |
| ReversalReason  | string(500)  | No       | Mandatory if this is a reversal                                                                          |
| PostedBy        | string(100)  | No       | User who posted                                                                                          |
| Lines           | collection   | Yes      | Navigation to JournalEntryLine (minimum 2)                                                               |
| CreatedAt       | datetime     | Yes      | UTC                                                                                                      |
| CreatedBy       | string(100)  | Yes      |                                                                                                          |
| ModifiedAt      | datetime?    | No       | UTC                                                                                                      |
| ModifiedBy      | string(100)? | No       |                                                                                                          |
| RowVersion      | byte[]       | Yes      | Optimistic concurrency token                                                                             |
| IsDeleted       | bool         | Yes      | Soft delete (drafts only)                                                                                |
| DeletedAt       | datetime?    | No       |                                                                                                          |
| DeletedBy       | string(100)? | No       |                                                                                                          |

## 3.2 JournalEntryLine Entity Structure

| Property       | Type          | Required | Description                                           |
| -------------- | ------------- | -------- | ----------------------------------------------------- |
| Id             | int           | Yes      | Primary key (identity)                                |
| JournalEntryId | int           | Yes      | FK to parent JournalEntry                             |
| LineNumber     | int           | Yes      | Sequential within entry (1, 2, 3, …)                  |
| AccountId      | int           | Yes      | FK to Account (must be active leaf with AllowPosting) |
| DebitAmount    | decimal(18,2) | Yes      | Debit amount (0.00 if credit line). Non-negative.     |
| CreditAmount   | decimal(18,2) | Yes      | Credit amount (0.00 if debit line). Non-negative.     |
| Description    | string(500)   | No       | Line-level narrative                                  |
| CostCenterId   | int?          | No       | Optional (overrides header cost center)               |
| WarehouseId    | int?          | No       | Warehouse reference (inventory-related entries)       |
| CreatedAt      | datetime      | Yes      | UTC                                                   |
| CreatedBy      | string(100)   | Yes      |                                                       |

## 3.3 JournalEntry Domain Invariants

| Invariant ID | Rule                                                                               |
| ------------ | ---------------------------------------------------------------------------------- |
| JE-INV-01    | A JournalEntry must have at least 2 lines. (JNL-01)                                |
| JE-INV-02    | SUM(DebitAmount) must equal SUM(CreditAmount) across all lines. (DEB-01)           |
| JE-INV-03    | No line may have both DebitAmount > 0 and CreditAmount > 0. (JNL-03)               |
| JE-INV-04    | No line may have both DebitAmount = 0 and CreditAmount = 0. (JNL-04)               |
| JE-INV-05    | Negative amounts are forbidden on any line. (DEB-04)                               |
| JE-INV-06    | Each line must reference exactly one valid, active, leaf account. (DEB-05, JNL-10) |
| JE-INV-07    | JournalDate must fall within the FiscalYear date range. (JNL-07)                   |
| JE-INV-08    | JournalDate must fall within the referenced FiscalPeriod. (JNL-08)                 |
| JE-INV-09    | FiscalPeriod must be in Open status. (JNL-08)                                      |
| JE-INV-10    | FiscalYear must be in Active status. (FY-02)                                       |
| JE-INV-11    | Once Status = Posted, no field may be modified. (JNL-17)                           |
| JE-INV-12    | A Reversed entry cannot be reversed again. (REV-09)                                |
| JE-INV-13    | Description is mandatory (non-empty, non-whitespace).                              |

### Validation Method Placement

```text
Domain Layer:
  JournalEntry.Validate()
    → Checks: JE-INV-01 through JE-INV-06, JE-INV-11, JE-INV-12, JE-INV-13
    → Returns: list of validation failures (never throws)

Application Layer:
  PostJournalService.Post()
    → Checks: JE-INV-07 through JE-INV-10 (requires repository lookups)
    → Calls: JournalEntry.Validate() first
    → Checks: authorization
    → Coordinates: number generation, persistence, audit

```

## 3.4 Journal Status Transition Rules

```text
           ┌───────────────────────────────────────────┐
           │                                           │
           ▼                                           │
    ┌──────────┐     ┌──────────┐     ┌──────────┐    │
    │  DRAFT   │────►│  POSTED  │────►│ REVERSED │    │
    │  (0)     │     │  (1)     │     │  (2)     │    │
    └──────────┘     └──────────┘     └──────────┘    │
         │                │                            │
         │ Can be:        │ Can be:                    │
         │ - Edited       │ - Reversed (new entry) ────┘
         │ - Soft deleted │ - Adjusted (new entry)
         │ - Posted       │ - Printed
         │                │ Cannot be:
         │                │ - Edited
         │                │ - Deleted
         │                │ - Re-posted
         ▼                ▼

```

**Allowed Transitions:**

| From   | To       | Trigger                | Conditions                        |
| ------ | -------- | ---------------------- | --------------------------------- |
| Draft  | Posted   | PostJournal command    | All validations pass              |
| Draft  | Deleted  | SoftDelete command     | Only drafts can be deleted        |
| Posted | Reversed | ReverseJournal command | Period open, not already reversed |

**Forbidden Transitions:**

| From     | To       | Reason                                      |
| -------- | -------- | ------------------------------------------- |
| Posted   | Draft    | Immutability — posted records never regress |
| Reversed | Draft    | Immutability                                |
| Reversed | Posted   | Immutability                                |
| Reversed | Reversed | Cannot reverse a reversal (REV-09)          |

## 3.5 Posting Workflow (Step-by-Step)

Per `FINANCIAL_ENGINE_RULES.md` Section 2.3, enriched with design details:

```text
Step 1:  [Application] Receive PostJournalCommand (contains JournalEntry Id)
Step 2:  [Application] Load JournalEntry with all Lines from repository
Step 3:  [Application] Verify Status == Draft (reject if not)
Step 4:  [Application] Load FiscalPeriod for JournalDate
Step 5:  [Application] Verify FiscalPeriod.Status == Open
Step 6:  [Application] Load FiscalYear
Step 7:  [Application] Verify FiscalYear.Status == Active
Step 8:  [Domain]      Call JournalEntry.Validate()
           ├── Check: minimum 2 lines
           ├── Check: SUM(Debits) == SUM(Credits)
           ├── Check: no line with both Debit > 0 and Credit > 0
           ├── Check: no line with both Debit == 0 and Credit == 0
           ├── Check: no negative amounts
           ├── Check: all accounts active, leaf, AllowPosting
           └── Return: validation result (pass/fail with reasons)
Step 9:  [Application] If validation fails → return errors, abort
Step 10: [Application] Check user authorization for posting
Step 11: [Application] Call ICodeGenerator to assign JournalNumber
Step 12: [Domain]      Call JournalEntry.Post(journalNumber, postedBy, postedAt)
           ├── Set Status = Posted
           ├── Set JournalNumber
           ├── Set PostedBy, PostedAt
           └── Clear DraftCode (or retain for audit)
Step 13: [Application] Persist via IUnitOfWork (single transaction)
Step 14: [Application] Log to audit trail via IAuditLogger
Step 15: [Application] Return PostResult (success + JournalNumber, or failure + reasons)

```

**Atomicity:** Steps 11–14 execute within a single database transaction. If any step fails, the entire transaction rolls back. The consumed sequence number creates a gap — this is expected and documented (SEQ-05).

---

## SECTION 4 — Fiscal Year & Period Model

## 4.1 FiscalYear Entity Structure

| Property   | Type         | Required | Description                           |
| ---------- | ------------ | -------- | ------------------------------------- |
| Id         | int          | Yes      | Primary key (identity)                |
| Year       | int          | Yes      | Calendar year (e.g., 2026). Unique.   |
| StartDate  | date         | Yes      | Always January 1 of Year              |
| EndDate    | date         | Yes      | Always December 31 of Year            |
| Status     | enum         | Yes      | Setup = 0, Active = 1, Closed = 2     |
| ClosedAt   | datetime?    | No       | UTC timestamp of closure              |
| ClosedBy   | string(100)? | No       | User who closed                       |
| Periods    | collection   | Yes      | Navigation to 12 FiscalPeriod records |
| CreatedAt  | datetime     | Yes      |                                       |
| CreatedBy  | string(100)  | Yes      |                                       |
| ModifiedAt | datetime?    | No       |                                       |
| ModifiedBy | string(100)? | No       |                                       |
| RowVersion | byte[]       | Yes      | Optimistic concurrency token          |

### FiscalYear Invariants

| Invariant ID | Rule                                                                    |
| ------------ | ----------------------------------------------------------------------- |
| FY-INV-01    | StartDate is always January 1 of Year. (FY-01)                          |
| FY-INV-02    | EndDate is always December 31 of Year. (FY-01)                          |
| FY-INV-03    | Only ONE FiscalYear may have Status = Active at any time. (FY-02)       |
| FY-INV-04    | A FiscalYear has exactly 12 Periods. (FY-03)                            |
| FY-INV-05    | Year values must not overlap with any existing FiscalYear. (FY-04)      |
| FY-INV-06    | Closure requires all 12 periods to be Locked. (FY-06)                   |
| FY-INV-07    | Closure requires trial balance to balance. (FY-07)                      |
| FY-INV-08    | Closure is irreversible (Status cannot transition from Closed). (FY-09) |
| FY-INV-09    | After closure, no posting to any period in this year. (FY-10)           |

### FiscalYear Status Transitions

```text
    ┌──────────┐     ┌──────────┐     ┌──────────┐
    │  SETUP   │────►│  ACTIVE  │────►│  CLOSED  │
    │  (0)     │     │  (1)     │     │  (2)     │
    └──────────┘     └──────────┘     └──────────┘
                          │
                     Only ONE active
                     at any time

```

| From   | To     | Conditions                                                        |
| ------ | ------ | ----------------------------------------------------------------- |
| Setup  | Active | No other Active year exists. All 12 periods created.              |
| Active | Closed | All 12 periods Locked. Trial balance balanced. No pending drafts. |
| Closed | (none) | Irreversible.                                                     |

## 4.2 FiscalPeriod Entity Structure

| Property     | Type         | Required | Description                                  |
| ------------ | ------------ | -------- | -------------------------------------------- |
| Id           | int          | Yes      | Primary key (identity)                       |
| FiscalYearId | int          | Yes      | FK to FiscalYear                             |
| PeriodNumber | int          | Yes      | 1 through 12                                 |
| Year         | int          | Yes      | Calendar year (denormalized for convenience) |
| Month        | int          | Yes      | 1 through 12                                 |
| StartDate    | date         | Yes      | First day of the month                       |
| EndDate      | date         | Yes      | Last day of the month                        |
| Status       | enum         | Yes      | Open = 0, Locked = 1                         |
| LockedAt     | datetime?    | No       | UTC timestamp of lock                        |
| LockedBy     | string(100)? | No       | User who locked                              |
| CreatedAt    | datetime     | Yes      |                                              |
| CreatedBy    | string(100)  | Yes      |                                              |
| RowVersion   | byte[]       | Yes      | Optimistic concurrency token                 |

### FiscalPeriod Invariants

| Invariant ID | Rule                                                                          |
| ------------ | ----------------------------------------------------------------------------- |
| FP-INV-01    | PeriodNumber must be 1–12.                                                    |
| FP-INV-02    | StartDate and EndDate must correctly represent the calendar month.            |
| FP-INV-03    | Periods are locked sequentially (cannot lock March before February). (PER-01) |
| FP-INV-04    | Locking requires all drafts in that period to be resolved. (PER-02)           |
| FP-INV-05    | Unlock is Admin-only, and only for the most recent locked period. (PER-05)    |
| FP-INV-06    | A posting to a Locked period is rejected. (PER-08)                            |

## 4.3 Fiscal Year Closure Workflow

```text
Step 1:  [Application] Verify all 12 periods are Locked
Step 2:  [Application] Verify no Draft journal entries exist in this fiscal year
Step 3:  [Application] Run trial balance check (SUM debits = SUM credits)
Step 4:  [Domain]      Generate closing entries:
           ├── For each Revenue account: Debit Revenue, Credit Retained Earnings
           ├── For each Expense account: Credit Expense, Debit Retained Earnings
           ├── For each COGS account:    Credit COGS, Debit Retained Earnings
           ├── For each OtherIncome:     Debit OtherIncome, Credit Retained Earnings
           └── For each OtherExpense:    Credit OtherExpense, Debit Retained Earnings
Step 5:  [Application] Post closing entries (SourceType = Closing)
Step 6:  [Application] Mark FiscalYear.Status = Closed
Step 7:  [Application] Generate opening balances in new fiscal year (Section 7)
Step 8:  [Application] Audit log the closure with full detail

```

**Closing Entry Logic:**

- Balance sheet accounts (Assets, Liabilities, Equity) carry forward — no closing entry needed.
- Income statement accounts (Revenue, COGS, Expenses, OtherIncome, OtherExpense) are closed to `3121 Retained Earnings — Current`.
- Net result: if Revenue > Expenses, Retained Earnings increases (credit); if Expenses > Revenue, Retained Earnings decreases (debit).

---

## SECTION 5 — Inventory & COGS Accounting Flow

## 5.1 Inventory Costing Method

| Property            | Value                                       |
| ------------------- | ------------------------------------------- |
| Method              | Weighted Average Cost (WAC) — ONLY          |
| Cost Precision      | 4 decimal places                            |
| Financial Precision | 2 decimal places                            |
| Rounding            | Banker's Rounding (MidpointRounding.ToEven) |
| Scope               | Per-product, per-warehouse                  |
| Negative Stock      | Forbidden (configurable: warn or block)     |

## 5.2 Weighted Average Cost Calculation

### On Purchase Receipt (Inventory In)

```text
New WAC = (Existing Quantity × Current WAC + Received Quantity × Purchase Unit Cost)
          ÷ (Existing Quantity + Received Quantity)

New WAC is rounded to 4 decimal places (Banker's Rounding)

```

**Example:**

```text
Current:  100 units @ 10.0000 = 1,000.00
Receipt:   50 units @ 12.0000 =   600.00
New WAC: (100 × 10.0000 + 50 × 12.0000) ÷ 150 = 1600 ÷ 150 = 10.6667 (4 dec)
New Stock: 150 units @ 10.6667

```

### On Sales Delivery (Inventory Out)

```text
COGS Amount = Sold Quantity × Current WAC (4 decimals)
COGS Amount is rounded to 2 decimal places for the journal entry
WAC does NOT change on inventory out

```

**Example:**

```text
Current:  150 units @ 10.6667
Sold:      30 units
COGS:      30 × 10.6667 = 320.001 → rounded to 320.00
Remaining: 120 units @ 10.6667

```

## 5.3 Inventory Movement → Journal Entry Mapping

| Inventory Event          | Debit Account                  | Credit Account                 | Amount Basis          |
| ------------------------ | ------------------------------ | ------------------------------ | --------------------- |
| Purchase Receipt         | 1131 Inventory — Main WH       | 2111 AP — Trade Payables       | Purchase cost (2 dec) |
| Sales Delivery           | 5111 COGS — General            | 1131 Inventory — Main WH       | Qty × WAC (→ 2 dec)   |
| Warehouse Transfer       | 113X Inventory — Dest WH       | 113Y Inventory — Source WH     | Qty × WAC (→ 2 dec)   |
| Inventory Adjustment (+) | 1131 Inventory                 | 6151 Inventory Adjustment Acct | Qty × WAC (→ 2 dec)   |
| Inventory Adjustment (-) | 6151 Inventory Adjustment Acct | 1131 Inventory                 | Qty × WAC (→ 2 dec)   |
| Stock Count Gain         | 1131 Inventory                 | 6151 Inventory Adjustment Acct | Qty × WAC (→ 2 dec)   |
| Stock Count Loss         | 6151 Inventory Adjustment Acct | 1131 Inventory                 | Qty × WAC (→ 2 dec)   |

## 5.4 COGS Auto-Generation Flow

When a sales delivery is posted, the COGS journal entry is generated **atomically** within the same transaction:

```text
Step 1:  [Application] Sales delivery posted (reduces inventory quantity)
Step 2:  [Domain]      Retrieve current WAC for the product in the source warehouse
Step 3:  [Domain]      Calculate COGS: Quantity × WAC → round to 2 decimals
Step 4:  [Domain]      Validate: sufficient stock exists (quantity ≥ sold quantity)
Step 5:  [Application] Generate JournalEntry:
           ├── SourceType = Inventory
           ├── Line 1: Debit COGS account (5111) for COGS amount
           └── Line 2: Credit Inventory account (113X) for COGS amount
Step 6:  [Application] Auto-post the COGS journal entry
Step 7:  [Application] Both the sales delivery AND COGS entry persist in one transaction

```

**Atomicity:** If the COGS journal generation fails (e.g., insufficient stock), the entire sales delivery is rolled back. (AGJ-05)

## 5.5 Per-Warehouse Inventory Ledger

Each warehouse maintains its own:

- **Quantity balance** per product
- **WAC per product** (4 decimal precision)
- **GL sub-account** under the Inventory control account (1130)

```text
1130  Inventory (Control — no posting)
├── 1131  Inventory — Main Warehouse      (Leaf — posting ✓)
├── 1132  Inventory — Branch Warehouse    (Leaf — posting ✓)
└── 1133  Inventory — Showroom            (Leaf — posting ✓)

```

**Reconciliation check:** `SUM(all warehouse inventory leaf balances) = Inventory control account (1130) computed balance`

## 5.6 Rounding Difference Handling for Inventory

When extended cost (Qty × WAC) produces rounding differences at the 2-decimal financial level:

```text
WAC = 10.6667
Qty = 3
Exact: 3 × 10.6667 = 32.0001
Rounded (2 dec): 32.00
Difference: 0.0001 → accumulated in rounding account (6141)

```

**Rule:** Rounding differences are posted to `6141 Rounding Account` only when the cumulative difference for a transaction exceeds 0.01. Below that threshold, the 2-decimal rounded value is used directly.

---

## SECTION 6 — VAT Accounting Logic

## 6.1 VAT Model

| Property           | Value                                                |
| ------------------ | ---------------------------------------------------- |
| VAT Model          | Single VAT Control Account                           |
| VAT Input Account  | 1141 VAT Input Receivable (Asset)                    |
| VAT Output Account | 2121 VAT Output Payable (Liability)                  |
| VAT Pricing        | Configurable: Inclusive or Exclusive per transaction |
| VAT Precision      | 2 decimal places (same as financial amounts)         |
| VAT Rounding       | Banker's Rounding, per line then totaled             |

## 6.2 VAT Calculation Rules

### VAT-Exclusive Pricing

```text
Line VAT Amount = Line Net Amount × VAT Rate
Line Total = Line Net Amount + Line VAT Amount

Each line's VAT is rounded to 2 decimals individually (RND-05: line-level rounding).
Total VAT = SUM(rounded line VAT amounts)

```

### VAT-Inclusive Pricing

```text
Line VAT Amount = Line Total × (VAT Rate ÷ (1 + VAT Rate))
Line Net Amount = Line Total - Line VAT Amount

Each line's VAT is rounded to 2 decimals individually.
Total VAT = SUM(rounded line VAT amounts)

```

## 6.3 VAT Journal Entry Patterns

### Sales Invoice (VAT-Exclusive Example)

```text
Sale: Product X, Net 1,000.00, VAT 14%

Journal Entry:
  Debit   1121 AR — Trade         1,140.00    (total including VAT)
  Credit  4111 Sales — General    1,000.00    (net amount)
  Credit  2121 VAT Output Payable   140.00    (VAT amount)

```

### Purchase Invoice (VAT-Exclusive Example)

```text
Purchase: Product Y, Net 500.00, VAT 14%

Journal Entry:
  Debit   1131 Inventory — Main WH   500.00    (net amount, increases asset)
  Debit   1141 VAT Input Receivable    70.00    (VAT amount, recoverable)
  Credit  2111 AP — Trade Payables    570.00    (total including VAT)

```

### VAT Settlement

At the end of a VAT period, the net VAT position is settled:

```text
If VAT Output > VAT Input (VAT liability):
  Debit   2121 VAT Output Payable     [output amount]
  Credit  1141 VAT Input Receivable   [input amount]
  Credit  Cash/Bank                   [difference — amount owed to tax authority]

If VAT Input > VAT Output (VAT refund):
  Debit   2121 VAT Output Payable     [output amount]
  Debit   Cash/Bank                   [difference — refund expected]
  Credit  1141 VAT Input Receivable   [input amount]

```

## 6.4 VAT Rate Management

| Property     | Description                                                      |
| ------------ | ---------------------------------------------------------------- |
| Storage      | VAT rates stored as master data (VatRate entity)                 |
| Fields       | Id, Name, Rate (decimal), IsDefault, IsActive                    |
| Assignment   | Each product/service has an assigned VAT category                |
| Default      | System has one default VAT rate (e.g., 14%)                      |
| Zero-rate    | VAT-exempt items explicitly use rate = 0%                        |
| Immutability | VAT rate changes do not retroactively affect posted transactions |

## 6.5 VAT Domain Responsibilities

| Responsibility                 | Layer       | Justification                            |
| ------------------------------ | ----------- | ---------------------------------------- |
| VAT rate lookup                | Domain      | Business data, part of entity logic      |
| VAT amount calculation         | Domain      | Pure calculation, no external dependency |
| VAT line generation in journal | Application | Orchestration of multi-line journal      |
| VAT account mapping            | Domain      | Business rule: which account gets VAT    |
| VAT period reporting           | Application | Query orchestration                      |

---

## SECTION 7 — Opening Balance Design

## 7.1 Opening Balance Mechanism

Opening balances represent the starting financial position of a new fiscal year. They are derived from the closing balances of the previous fiscal year.

### Opening Balance Entry Structure

```text
SourceType: Opening
JournalDate: January 1 of the new fiscal year
FiscalPeriod: Period 1 (January) of the new fiscal year
Status: Auto-posted

```

## 7.2 Opening Balance Generation Rules

| Rule ID | Rule                                                                                                                 |
| ------- | -------------------------------------------------------------------------------------------------------------------- |
| OB-01   | Opening balances are generated ONLY as part of fiscal year closure.                                                  |
| OB-02   | Opening balances carry forward Balance Sheet accounts only (Assets, Liabilities, Equity).                            |
| OB-03   | Income statement accounts (Revenue, COGS, Expenses) start at zero — their balances were closed to Retained Earnings. |
| OB-04   | Each balance sheet leaf account with a non-zero balance gets one opening entry line.                                 |
| OB-05   | The opening balance entry must balance (total debits = total credits).                                               |
| OB-06   | Opening balance entries carry SourceType = Opening.                                                                  |
| OB-07   | Opening balance entries are auto-posted (no draft state).                                                            |
| OB-08   | Opening balance generation is atomic with fiscal year closure.                                                       |
| OB-09   | If the new fiscal year does not exist yet, it is created as part of the closure process.                             |

## 7.3 Opening Balance Generation Flow

```text
Step 1:  [Application] Fiscal year closure completes (closing entries posted)
Step 2:  [Application] Create new FiscalYear (if not exists) with Status = Setup
Step 3:  [Application] Create 12 FiscalPeriods for the new year (all Open)
Step 4:  [Application] For each balance sheet leaf account:
           ├── Calculate closing balance (sum of all posted debits - credits, or vice versa)
           ├── If balance ≠ 0:
           │     If NormalBalance = Debit and balance > 0 → Debit line
           │     If NormalBalance = Debit and balance < 0 → Credit line (unusual but valid)
           │     If NormalBalance = Credit and balance > 0 → Credit line
           │     If NormalBalance = Credit and balance < 0 → Debit line (unusual but valid)
           └── Skip if balance = 0
Step 5:  [Domain]      Validate opening balance entry (must balance)
Step 6:  [Application] Auto-post opening balance entry in Period 1 of new year
Step 7:  [Application] Activate new FiscalYear (Status = Active)
Step 8:  [Application] Audit log the opening balance generation

```

## 7.4 First-Year Opening Balances

For the very first fiscal year (no previous year to close), opening balances are entered **manually** by the user:

| Rule ID     | Rule                                                                 |
| ----------- | -------------------------------------------------------------------- |
| OB-FIRST-01 | Manual opening balance entry created as a standard journal entry.    |
| OB-FIRST-02 | SourceType = Opening.                                                |
| OB-FIRST-03 | Must balance (debits = credits).                                     |
| OB-FIRST-04 | Typically: Debit asset accounts, Credit liability + equity accounts. |
| OB-FIRST-05 | Posted in Period 1 of the first fiscal year.                         |
| OB-FIRST-06 | Follows all standard journal posting rules.                          |

---

## SECTION 8 — Concurrency & Transaction Integrity Model

## 8.1 Concurrency Strategy

| Property          | Value                                            |
| ----------------- | ------------------------------------------------ |
| Concurrency Model | Optimistic Concurrency                           |
| Token             | RowVersion (SQL Server timestamp/rowversion)     |
| Scope             | All editable entities                            |
| Detection         | Compare RowVersion on save — mismatch = conflict |
| Resolution        | Reject conflicting save, report to user          |

### Entities with Concurrency Tokens

| Entity       | RowVersion Required | Reason                                  |
| ------------ | ------------------- | --------------------------------------- |
| Account      | Yes                 | Multi-user account editing              |
| JournalEntry | Yes                 | Concurrent draft editing before posting |
| FiscalYear   | Yes                 | Year status changes                     |
| FiscalPeriod | Yes                 | Period lock/unlock operations           |
| Product      | Yes                 | Price and stock changes                 |
| Customer     | Yes                 | Master data editing                     |
| Supplier     | Yes                 | Master data editing                     |
| Warehouse    | Yes                 | Configuration changes                   |
| VatRate      | Yes                 | Rate modifications                      |
| CodeSequence | Yes                 | Concurrent code generation              |

## 8.2 Concurrency Conflict Handling

```text
Scenario: Two users edit the same draft journal entry

User A: Loads JournalEntry #42 (RowVersion = 0x0001)
User B: Loads JournalEntry #42 (RowVersion = 0x0001)

User A: Saves changes → RowVersion becomes 0x0002 → SUCCESS
User B: Saves changes → RowVersion mismatch (expected 0x0001, actual 0x0002) → CONFLICT

Resolution for User B:

  1. Application catches concurrency exception

  2. Returns error: "This record was modified by another user. Please reload and try again."

  3. User B must reload the latest version and re-apply changes

  4. No automatic merge — explicit user action required

```

### Concurrency Rules

| Rule ID | Rule                                                                          |
| ------- | ----------------------------------------------------------------------------- |
| CONC-01 | RowVersion is present on ALL entities listed above.                           |
| CONC-02 | Persistence layer configures RowVersion as concurrency token.                 |
| CONC-03 | On conflict, the entire save operation is rejected (no partial saves).        |
| CONC-04 | Conflict error message is user-friendly (no stack trace or technical detail). |
| CONC-05 | The application does NOT auto-retry on conflict (user must decide).           |
| CONC-06 | Concurrency check happens at the Persistence layer (EF Core handles it).      |
| CONC-07 | Posting operations use Serializable isolation level for maximum safety.       |

## 8.3 Transaction Integrity

### Isolation Levels by Operation

| Operation                    | Isolation Level           | Reason                                     |
| ---------------------------- | ------------------------- | ------------------------------------------ |
| Read queries (lists, search) | Read Committed            | Standard reads, no dirty reads             |
| Draft creation/editing       | Read Committed            | Low contention expected                    |
| Journal posting              | Serializable              | Prevent double-posting, sequence conflicts |
| Fiscal period lock/unlock    | Serializable              | Critical state change                      |
| Fiscal year closure          | Serializable              | Multi-step critical operation              |
| Code sequence increment      | Serializable              | Uniqueness guarantee                       |
| Inventory WAC recalculation  | Serializable              | Cost integrity during concurrent receipts  |
| Report generation            | Read Committed (Snapshot) | Consistent read without blocking writes    |

### Transaction Boundaries

```text
Unit of Work Scope:
  ┌─────────────────────────────────────────┐
  │ Application Service Method              │
  │                                         │
  │   Begin Transaction                     │
  │     ├── Load entities                   │
  │     ├── Execute domain logic            │
  │     ├── Persist changes                 │
  │     ├── Generate audit log              │
  │     └── Commit (or Rollback on failure) │
  │                                         │
  └─────────────────────────────────────────┘

```

| Rule ID    | Rule                                                                                    |
| ---------- | --------------------------------------------------------------------------------------- |
| TRX-INT-01 | One transaction per use case (no nested transactions). (TRX-06)                         |
| TRX-INT-02 | Application layer initiates, Persistence layer executes. (TRX-02)                       |
| TRX-INT-03 | Posting is atomic — all or nothing. (TRX-03)                                            |
| TRX-INT-04 | Auto-generated journals (COGS, VAT) are within the same transaction as source. (AGJ-04) |
| TRX-INT-05 | Audit log writes are within the same transaction as the business operation.             |
| TRX-INT-06 | Timeout: 30 seconds for standard ops, 60 seconds for year-end closure.                  |

## 8.4 Sequence Number Concurrency

Journal number and document code generation must handle concurrent users:

```text
Approach: Database-level SEQUENCE object (per RECORD_PROTECTION_POLICY SEQ-03)

Step 1: Before posting, Application calls ICodeGenerator.GetNextCode(documentType, fiscalYearId)
Step 2: ICodeGenerator reads from CodeSequence table with Serializable isolation
Step 3: CurrentSequence is incremented atomically
Step 4: The new number is returned to the caller
Step 5: If posting fails after this point, the number is consumed (gap occurs — SEQ-04)

Why not application-level locking?

  - Multiple app instances may run (multi-user)

  - Database-level atomicity is the only reliable guarantee

  - SQL Server SEQUENCE or UPDATE with OUTPUT is the mechanism

```

---

## SECTION 9 — Architectural Risks & Mitigation

## 9.1 Risk Register

| Risk ID | Risk Description                                                   | Severity | Probability | Impact                      |
| ------- | ------------------------------------------------------------------ | -------- | ----------- | --------------------------- |
| R2-01   | Weighted Average Cost rounding drift over high-volume transactions | Medium   | Medium      | Financial inaccuracy        |
| R2-02   | Concurrent posting creating duplicate journal numbers              | High     | Low         | Data integrity              |
| R2-03   | COGS auto-generation failure orphaning sales transactions          | High     | Low         | Data inconsistency          |
| R2-04   | Fiscal year closure generating incorrect opening balances          | Critical | Low         | Financial corruption        |
| R2-05   | Chart of Accounts hierarchy corruption (orphaned accounts)         | Medium   | Low         | Data integrity              |
| R2-06   | Period lock race condition (posting while locking)                 | Medium   | Medium      | Process integrity           |
| R2-07   | VAT rounding differences accumulating over many invoices           | Low      | High        | Minor financial discrepancy |
| R2-08   | Inventory going negative due to concurrent sales                   | High     | Medium      | Business rule violation     |
| R2-09   | Opening balance entry not balancing after year-end close           | Critical | Low         | Accounting violation        |
| R2-10   | Reversal creating entries in already-locked period                 | High     | Low         | Period integrity            |

## 9.2 Mitigation Strategies

### R2-01: WAC Rounding Drift

| Mitigation                                                                   |
| ---------------------------------------------------------------------------- |
| Use 4-decimal precision for unit cost (INV-06).                              |
| Use Banker's Rounding consistently (RND-02).                                 |
| Post rounding differences to dedicated account (6141) when they exceed 0.01. |
| Periodic reconciliation report: inventory GL balance vs. computed WAC × Qty. |

### R2-02: Duplicate Journal Numbers

| Mitigation                                                                   |
| ---------------------------------------------------------------------------- |
| Use SQL Server SEQUENCE object for atomic number generation (SEQ-03).        |
| Serializable isolation during posting transaction (TRX-05).                  |
| Unique database constraint on JournalNumber within FiscalYear.               |
| If constraint violation occurs, retry with new number (application handles). |

### R2-03: COGS Orphan Risk

| Mitigation                                                                   |
| ---------------------------------------------------------------------------- |
| COGS entry and sales delivery share the same transaction (AGJ-04, AGJ-05).   |
| If COGS generation fails, the entire sales transaction rolls back.           |
| Domain layer validates stock availability BEFORE attempting the transaction. |
| Inventory balance check is within the Serializable transaction scope.        |

### R2-04: Incorrect Opening Balances

| Mitigation                                                                     |
| ------------------------------------------------------------------------------ |
| Trial balance verification before closure (FY-07).                             |
| Opening balance entry goes through standard Journal Validate() — must balance. |
| Fiscal year closure is atomic — if opening balance fails, closure rolls back.  |
| Post-closure verification: sum of opening balances = sum of closing balances.  |

### R2-05: Hierarchy Corruption

| Mitigation                                                               |
| ------------------------------------------------------------------------ |
| ParentAccountId validated at Domain layer (ACC-INV-06).                  |
| Foreign key constraint at database level.                                |
| Account deletion checks for children before allowing deactivation.       |
| Periodic integrity check: all non-root accounts must have valid parents. |

### R2-06: Period Lock Race Condition

| Mitigation                                                             |
| ---------------------------------------------------------------------- |
| Period lock operation uses Serializable isolation.                     |
| Posting checks period status within the same Serializable transaction. |
| Period status change and posting cannot interleave.                    |
| RowVersion on FiscalPeriod prevents concurrent lock/unlock operations. |

### R2-07: VAT Rounding Accumulation

| Mitigation                                                     |
| -------------------------------------------------------------- |
| Line-level rounding before totaling (RND-05).                  |
| Rounding differences posted to rounding account (6141).        |
| VAT report includes rounding differences for transparency.     |
| Cumulative rounding tracked per VAT period for reconciliation. |

### R2-08: Negative Inventory

| Mitigation                                                                 |
| -------------------------------------------------------------------------- |
| Domain layer validates: available quantity ≥ requested quantity (INV-05).  |
| Check happens within Serializable transaction (prevents concurrent drain). |
| Configurable behavior: block (default) or warn.                            |
| If blocked, the entire sale/delivery transaction rolls back.               |

### R2-09: Unbalanced Opening Balance

| Mitigation                                                                   |
| ---------------------------------------------------------------------------- |
| Opening balance entry goes through JournalEntry.Validate() (JE-INV-02).      |
| Additional check: SUM(opening debits) = SUM(closing balance sheet balances). |
| Atomic with year-end closure — failure = full rollback.                      |

### R2-10: Reversal in Locked Period

| Mitigation                                                                |
| ------------------------------------------------------------------------- |
| Reversal checks original entry's period lock status (REV-P1, REV-P2).     |
| Locked period = reversal blocked. Clear error message.                    |
| Admin must unlock period explicitly before reversal can proceed (REV-P3). |
| Closed fiscal year = use adjustment instead (REV-P4).                     |

---

## Appendix A — Entity Relationship Summary

```text
FiscalYear (1) ──────── (12) FiscalPeriod
     │
     └──── (many) JournalEntry (1) ──── (many) JournalEntryLine
                       │                              │
                       │                              └──── Account (leaf, active)
                       │
                       ├── FiscalPeriod
                       ├── ReversedEntry? ──► JournalEntry
                       ├── ReversalEntry? ──► JournalEntry
                       └── AdjustedEntry? ──► JournalEntry

Account (self-referencing hierarchy)
     │
     └── ParentAccount? ──► Account (Level - 1)

CodeSequence
     │
     └── FiscalYear + DocumentType → CurrentSequence

```

## Appendix B — Enum Reference

| Enum Name        | Values                                                                                                                     |
| ---------------- | -------------------------------------------------------------------------------------------------------------------------- |
| AccountType      | Asset=0, Liability=1, Equity=2, Revenue=3, COGS=4, Expense=5, OtherIncome=6, OtherExpense=7                                |
| NormalBalance    | Debit=0, Credit=1                                                                                                          |
| JournalStatus    | Draft=0, Posted=1, Reversed=2                                                                                              |
| SourceType       | Manual=0, SalesInvoice=1, PurchaseInvoice=2, CashReceipt=3, CashPayment=4, Inventory=5, Adjustment=6, Opening=7, Closing=8 |
| FiscalYearStatus | Setup=0, Active=1, Closed=2                                                                                                |
| PeriodStatus     | Open=0, Locked=1                                                                                                           |

## Appendix C — Governance Alignment Matrix

| Phase 2 Design Decision                    | Governance Reference                   | Status    |
| ------------------------------------------ | -------------------------------------- | --------- |
| 4-level hierarchy, Egyptian numbering      | ACCOUNTING_PRINCIPLES §3               | Aligned ✓ |
| Leaf-only posting                          | ACCOUNTING_PRINCIPLES ACC-04           | Aligned ✓ |
| System accounts locked                     | ACCOUNTING_PRINCIPLES ACC-02           | Aligned ✓ |
| Double-entry enforcement at Domain         | ACCOUNTING_PRINCIPLES §4, DEB-07       | Aligned ✓ |
| Journal status lifecycle                   | FINANCIAL_ENGINE_RULES §2              | Aligned ✓ |
| Posting workflow (15 steps)                | FINANCIAL_ENGINE_RULES §2.3            | Aligned ✓ |
| Fiscal year = calendar year                | ACCOUNTING_PRINCIPLES FY-00, FY-01     | Aligned ✓ |
| 12 monthly periods                         | ACCOUNTING_PRINCIPLES §1               | Aligned ✓ |
| Sequential period locking                  | FINANCIAL_ENGINE_RULES PER-01          | Aligned ✓ |
| Weighted Average Cost only                 | ACCOUNTING_PRINCIPLES INV-02           | Aligned ✓ |
| 4-decimal cost precision                   | ACCOUNTING_PRINCIPLES INV-06           | Aligned ✓ |
| 2-decimal financial precision              | ACCOUNTING_PRINCIPLES RND-01           | Aligned ✓ |
| Single VAT control account                 | ACCOUNTING_PRINCIPLES §6               | Aligned ✓ |
| COGS auto-generation on sales              | ACCOUNTING_PRINCIPLES §7.2             | Aligned ✓ |
| Immutable posted records                   | RECORD_PROTECTION_POLICY §1            | Aligned ✓ |
| Reversal mechanism                         | RECORD_PROTECTION_POLICY §3            | Aligned ✓ |
| Adjustment mechanism                       | RECORD_PROTECTION_POLICY §4            | Aligned ✓ |
| Auto-code generation (gaps allowed)        | RECORD_PROTECTION_POLICY §5            | Aligned ✓ |
| RowVersion for concurrency                 | DATABASE_POLICY §4.1                   | Aligned ✓ |
| Serializable isolation for posting         | DATABASE_POLICY TRX-05                 | Aligned ✓ |
| No hard deletes on financial records       | RECORD_PROTECTION_POLICY §2            | Aligned ✓ |
| Domain validates, Application orchestrates | ARCHITECTURE §2.1, §2.2                | Aligned ✓ |
| Audit trail on all mutations               | DATABASE_POLICY §8                     | Aligned ✓ |
| Clean Architecture layer boundaries        | ARCHITECTURE §1, SOLUTION_STRUCTURE §3 | Aligned ✓ |

---

## Appendix D — Confirmed Decisions

The following decisions were confirmed for Phase 2 and are now locked for implementation:

1. **Customer/Supplier Sub-Accounts:** Each customer and supplier auto-creates a dedicated leaf account under AR (1120) / AP (2110).
2. **Multi-Warehouse Inventory Accounts:** Each new warehouse auto-creates a dedicated leaf inventory account under `1130 Inventory`.
3. **Cashbox Account Auto-Creation:** Each new cashbox auto-creates a dedicated leaf account under `1110 Cash and Banks`.
4. **VAT Rate Change Mid-Year:** Support multiple active VAT rates simultaneously (new rates added; old rates deactivated only when no longer needed).
5. **Cost Center on Opening Balances:** Opening balance entries do NOT carry cost center tags; cost centers apply to current-year transactions only.
6. **Period Unlock Justification:** Unlocking a period requires a mandatory justification note, recorded in the audit log (PER-06).

---

## Version History

| Version | Date       | Change Description                     |
| ------- | ---------- | -------------------------------------- |
| 1.0     | 2026-02-08 | Initial Phase 2 accounting core design |
