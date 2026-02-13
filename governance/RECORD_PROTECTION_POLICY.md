# MarcoERP – Record Protection Policy

**Deliverable 5: Immutability, Reversals, Adjustments, and Auto-Code Generation**

---

## 1. Core Protection Principle

> **Financial records, once posted, are permanent and unalterable.**
>
> Corrections are made by adding new records (reversals or adjustments), never by modifying or deleting existing ones.

This is not a technical preference — it is an **accounting legal requirement** and the foundation of audit integrity.

---

## 2. No Hard Delete Policy

### 2.1 What Cannot Be Hard Deleted (EVER)

| Data Category                  | Hard Delete | Soft Delete | Deactivate |
|--------------------------------|-------------|-------------|------------|
| Posted Journal Entries         | FORBIDDEN   | FORBIDDEN   | N/A        |
| Posted Journal Lines           | FORBIDDEN   | FORBIDDEN   | N/A        |
| Draft Journal Entries          | FORBIDDEN   | ALLOWED     | N/A        |
| Posted Invoices                | FORBIDDEN   | FORBIDDEN   | N/A        |
| Draft Invoices                 | FORBIDDEN   | ALLOWED     | N/A        |
| Chart of Accounts (with posts) | FORBIDDEN  | FORBIDDEN   | ALLOWED    |
| Chart of Accounts (no posts)  | FORBIDDEN   | ALLOWED     | ALLOWED    |
| Fiscal Years                   | FORBIDDEN   | FORBIDDEN   | N/A        |
| Fiscal Periods                 | FORBIDDEN   | FORBIDDEN   | N/A        |
| Audit Log Records              | FORBIDDEN   | FORBIDDEN   | N/A        |
| User Accounts                  | FORBIDDEN   | ALLOWED     | ALLOWED    |
| Inventory Transactions         | FORBIDDEN   | FORBIDDEN   | N/A        |
| Customers (with transactions)  | FORBIDDEN   | FORBIDDEN   | ALLOWED    |
| Customers (no transactions)    | FORBIDDEN   | ALLOWED     | ALLOWED    |
| Suppliers (with transactions)  | FORBIDDEN   | FORBIDDEN   | ALLOWED    |
| Suppliers (no transactions)    | FORBIDDEN   | ALLOWED     | ALLOWED    |
| Products (with transactions)   | FORBIDDEN   | FORBIDDEN   | ALLOWED    |
| Products (no transactions)     | FORBIDDEN   | ALLOWED     | ALLOWED    |
| Warehouses (with stock)        | FORBIDDEN   | FORBIDDEN   | ALLOWED    |
| Cashboxes (with transactions)  | FORBIDDEN   | FORBIDDEN   | ALLOWED    |

### 2.2 Soft Delete Implementation

| Rule ID | Rule                                                                     |
|---------|--------------------------------------------------------------------------|
| SD-01   | Soft delete sets `IsDeleted = true`, `DeletedAt = UTC now`, `DeletedBy = current user`. |
| SD-02   | Soft-deleted records are **excluded** from all normal queries.           |
| SD-03   | Soft-deleted records remain visible in audit trail and historical reports.|
| SD-04   | Soft-deleted records can be restored by Admin (un-delete).               |
| SD-05   | EF Core global query filter: `.HasQueryFilter(e => !e.IsDeleted)`.       |
| SD-06   | Queries that need soft-deleted records must explicitly ignore the filter. |

### 2.3 Deactivation vs. Soft Delete

| Action       | Effect                                                          |
|--------------|-----------------------------------------------------------------|
| Deactivate   | Record exists, visible, but cannot be used in new transactions  |
| Soft Delete  | Record hidden from normal queries, treated as removed           |

Deactivation is the preferred method for master data that has been referenced in posted transactions.

---

## 3. Reversal Mechanism

### 3.1 What is a Reversal?

A reversal is a new journal entry that is the **exact mirror** of the original posted entry. Every debit becomes a credit, and every credit becomes a debit, for the same amounts.

### 3.2 Reversal Rules

| Rule ID | Rule                                                                     |
|---------|--------------------------------------------------------------------------|
| REV-01  | Only **posted** entries can be reversed.                                 |
| REV-02  | Reversal creates a **new** journal entry — the original is not modified. |
| REV-03  | The original entry's status changes to `Reversed`.                       |
| REV-04  | The reversal entry links to the original via `ReversedEntryId`.          |
| REV-05  | The original entry links to the reversal via `ReversalEntryId`.          |
| REV-06  | Reversal entry is **auto-posted** — it does not go through draft state.  |
| REV-07  | Reversal entry uses the **original transaction date** (same date as the original entry).|
| REV-08  | Reversal date must fall within an open fiscal period.                    |
| REV-09  | An already-reversed entry **cannot** be reversed again.                  |
| REV-10  | Reversal requires authorization (Accountant or Admin role).              |
| REV-11  | Reversal must include a mandatory reason/justification text.             |
| REV-12  | Reversal is logged in the audit trail with reason and user.              |
| REV-13  | If the original date falls in a locked period, the reversal is BLOCKED.  |
| REV-14  | Admin must unlock the period before the reversal can proceed.            |
| REV-15  | After successful reversal, the period may be re-locked.                  |

### 3.3 Reversal Process

```
Step 1: User selects a posted journal entry
Step 2: User clicks "Reverse" and enters justification
Step 3: System creates new journal entry with mirrored lines
Step 4: System auto-posts the reversal entry
Step 5: System marks original entry as "Reversed"
Step 6: System links original ↔ reversal entries
Step 7: Audit log records the reversal with justification
Step 8: Success confirmation shown to user
```

### 3.4 Reversal Entry Structure

```
Original Entry:
  Line 1: Debit  Account A    $500.00
  Line 2: Credit Account B    $500.00

Reversal Entry:
  Line 1: Credit Account A    $500.00
  Line 2: Debit  Account B    $500.00
  Description: "Reversal of Journal #JE-2026-0042 — Reason: [user text]"
```

### 3.5 Reversal and Period Lock Interaction

**CRITICAL RULE: Reversals respect period locks. No bypass mechanism exists.**

When reversing a posted entry, the system evaluates the original transaction date against the current period lock status:

#### Scenario 1: Original date is in an OPEN period
- Reversal proceeds normally using original date
- Reversal entry is auto-posted
- Status: **ALLOWED**

#### Scenario 2: Original date is in a LOCKED period
- Reversal is **BLOCKED** with message: "Cannot reverse: Original period {YYYY-MM} is locked"
- Admin must unlock the period first (if allowed per period lock policy)
- After reversal, period may be re-locked
- Status: **BLOCKED until period unlocked**

#### Scenario 3: Original date is in a CLOSED fiscal year
- Reversal is **BLOCKED** (closed fiscal years cannot be unlocked)
- Alternative: Create adjustment entry in current open period
- Document reason with reference to original entry
- Status: **BLOCKED permanently (use adjustment instead)**

| Rule ID | Rule                                                                     |
|---------|--------------------------------------------------------------------------|
| REV-P1  | Reversal respects period lock — no bypass mechanism exists.              |
| REV-P2  | Locked period prevents ANY posting, including reversals.                 |
| REV-P3  | To reverse an entry in a locked period: unlock → reverse → re-lock.     |
| REV-P4  | To reverse an entry in a closed fiscal year: use adjustment instead.    |
| REV-P5  | Closed fiscal years are immutable. No reversals or adjustments allowed in closed years. |

---

## 4. Adjustment Mechanism

### 4.1 What is an Adjustment?

An adjustment is a **new journal entry** that corrects or modifies the financial effect of a previous entry without fully reversing it. It is used when the original entry was partially correct.

### 4.2 Adjustment Rules

| Rule ID | Rule                                                                     |
|---------|--------------------------------------------------------------------------|
| ADJ-01  | Adjustments are standard journal entries with SourceType = "Adjustment".|
| ADJ-02  | Adjustments reference the original entry via `AdjustedEntryId`.          |
| ADJ-03  | Adjustments follow all normal posting rules (balance, period, accounts). |
| ADJ-04  | The original entry is **not modified** — it retains its Posted status.   |
| ADJ-05  | Adjustments are typically used for:                                       |
|         | - Correcting amounts (post the difference)                               |
|         | - Reclassifying to a different account                                   |
|         | - Adding missing lines                                                   |
| ADJ-06  | Adjustments require a mandatory description explaining the correction.   |
| ADJ-07  | Adjustment authorization follows the same rules as standard posting.     |
| ADJ-08  | **Adjustments are the ONLY way to correct entries in CLOSED periods.**  |
| ADJ-09  | Adjustment entry is posted in the current OPEN period (not original period). |
| ADJ-10  | System must validate that AdjustedEntryId references a real posted entry. |
| ADJ-11  | Adjustment must include clear documentation linking to original transaction. |

### 4.3 Reversal vs. Adjustment Decision Guide

| Scenario                            | Use Reversal | Use Adjustment | Notes                           |
|-------------------------------------|:------------:|:--------------:|---------------------------------|
| Entire entry is wrong               | Yes          | No             | If period is open               |
| Amount on one line is wrong         | No           | Yes            | Post the difference             |
| Wrong account used                  | Either       | Preferred      | Adjustment is simpler           |
| Missing a line entry                | No           | Yes            | Add the missing line            |
| Duplicate entry posted              | Yes          | No             | If period is open               |
| Need to change the posting date     | Yes*         | No             | Reverse and re-post             |
| Original is in LOCKED period        | No           | Yes            | Cannot reverse locked period    |
| Original is in CLOSED fiscal year   | No           | Yes            | Cannot reverse closed year      |

*Reverse and re-post with correct date (only if period is open).

---

## 5. Auto-Code Generation Rules

### 5.1 Code Generation Principles

| Rule ID | Rule                                                                     |
|---------|--------------------------------------------------------------------------|
| ACG-01  | Auto-codes are generated by the **Infrastructure layer** via `ICodeGenerator`. |
| ACG-02  | Code generation is called by the **Application layer** during posting.   |
| ACG-03  | Each document type has its own code sequence.                            |
| ACG-04  | Codes are sequential and unique for posted documents. Gaps acceptable.    |
| ACG-05  | Codes reset at the start of each fiscal year.                            |
| ACG-06  | Draft documents receive temporary codes until posting.                   |

### 5.2 Code Format Patterns

| Document Type     | Pattern                    | Example                |
|-------------------|----------------------------|------------------------|
| Journal Entry     | `JE-{YYYY}-{SEQ:5}`       | `JE-2026-00001`        |
| Sales Invoice     | `SI-{YYYYMM}-{SEQ:4}`     | `SI-202602-0001`       |
| Purchase Invoice  | `PI-{YYYYMM}-{SEQ:4}`     | `PI-202602-0001`       |
| Sales Return      | `SR-{YYYYMM}-{SEQ:4}`     | `SR-202602-0001`       |
| Purchase Return   | `PR-{YYYYMM}-{SEQ:4}`     | `PR-202602-0001`       |
| Cash Receipt      | `CR-{YYYYMM}-{SEQ:4}`     | `CR-202602-0001`       |
| Cash Payment      | `CP-{YYYYMM}-{SEQ:4}`     | `CP-202602-0001`       |
| Cash Transfer     | `CT-{YYYYMM}-{SEQ:4}`     | `CT-202602-0001`       |
| Inv. Adjustment   | `ADJ-{YYYYMM}-{SEQ:4}`    | `ADJ-202602-0001`      |
| Product Code      | `PRD-{SEQ:5}`              | `PRD-00123`            |
| Customer Code     | `CUS-{SEQ:5}`              | `CUS-00045`            |
| Supplier Code     | `SUP-{SEQ:5}`              | `SUP-00012`            |

### 5.3 Draft Code Rules

| Rule ID | Rule                                                                     |
|---------|--------------------------------------------------------------------------|
| DRC-01  | Drafts receive a temporary code: `DRAFT-{GUID:8}` (e.g., `DRAFT-a1b2c3d4`). |
| DRC-02  | Draft codes are replaced with final sequential codes on posting.         |
| DRC-03  | Re-numbering (changing draft order) is allowed **only** while in Draft.  |
| DRC-04  | Once posted, the code is permanent and cannot change.                    |
| DRC-05  | The temporary-to-final code change is logged in the audit trail.         |

### 5.4 Sequence Management

| Rule ID | Rule                                                                     |
|---------|--------------------------------------------------------------------------|
| SEQ-01  | Each document type maintains its own sequence counter per fiscal year.   |
| SEQ-02  | Sequence counters are stored in a dedicated `CodeSequences` table.       |
| SEQ-03  | Sequence increment uses SQL Server SEQUENCE object for multi-user uniqueness. |
| SEQ-04  | Sequence number assigned BEFORE validation. If posting fails, number is consumed (gap occurs). |
| SEQ-05  | Sequence assignment guarantees uniqueness but NOT gap-free numbering.     |
| SEQ-06  | Gaps in posted sequences are normal and do not indicate data corruption.   |
| SEQ-07  | Gaps in draft sequences are acceptable and expected.                        |

### 5.5 Code Sequence Table Structure

| Column          | Type              | Description                          |
|-----------------|-------------------|--------------------------------------|
| Id              | int (PK)          | Primary key                          |
| DocumentType    | nvarchar(50)      | Type identifier (JE, SI, PI, etc.)   |
| FiscalYearId    | int (FK)          | Fiscal year reference                |
| CurrentSequence | bigint            | Current counter value                |
| Prefix          | nvarchar(20)      | Code prefix template                 |
| PaddingLength   | int               | Zero-padding length (default: 5)     |

---

## 6. Record Protection Summary Matrix

| Operation               | Draft | Posted | Reversed | Soft-Deleted |
|-------------------------|-------|--------|----------|--------------|
| View                    | Yes   | Yes    | Yes      | Admin only   |
| Edit fields             | Yes   | NO     | NO       | NO           |
| Add/remove lines        | Yes   | NO     | NO       | NO           |
| Change date             | Yes   | NO     | NO       | NO           |
| Change account          | Yes   | NO     | NO       | NO           |
| Post                    | Yes   | N/A    | NO       | NO           |
| Soft delete             | Yes   | NO     | NO       | N/A          |
| Reverse                 | NO    | Yes    | NO       | NO           |
| Adjust                  | NO    | Yes    | NO       | NO           |
| Re-number code          | Yes   | NO     | NO       | NO           |
| Print                   | Yes   | Yes    | Yes      | Admin only   |
| Include in reports      | NO    | Yes    | Yes      | NO           |
| Hard delete             | NO    | NO     | NO       | NO           |

---

## Version History

| Version | Date       | Change Description                    |
|---------|------------|---------------------------------------|
| 1.0     | 2026-02-08 | Initial Phase 1 governance release    |
