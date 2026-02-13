# Phase 1 Correction Checklist

**Quick Reference for Applying Audit Corrections**

---

## PRIORITY 0 — CRITICAL (BLOCKING)

These MUST be fixed before Phase 2 begins.

### ☐ C1: Fix Reversal Date Policy

**File:** `governance/RECORD_PROTECTION_POLICY.md`

**Line 82 — CHANGE:**
```markdown
| REV-07  | Reversal entry uses the **current date** (not the original posting date).|
```

**TO:**
```markdown
| REV-07  | Reversal entry uses the **original transaction date** (same date as the original entry).|
```

**After REV-12 (Line ~94), ADD:**
```markdown
| REV-13  | If the original date falls in a locked period, the reversal is BLOCKED.  |
| REV-14  | Admin must unlock the period before the reversal can proceed.            |
| REV-15  | After successful reversal, the period may be re-locked.                  |
```

---

### ☐ C2: Add Reversal + Period Lock Interaction Rules

**File:** `governance/RECORD_PROTECTION_POLICY.md`

**After Section 3.4 (Line ~113), ADD NEW SECTION:**

```markdown
### 3.5 Reversal and Period Lock Interaction

When reversing a posted entry:

**Scenario 1: Original date is in an OPEN period**
- Reversal proceeds normally using original date
- Status: ALLOWED

**Scenario 2: Original date is in a LOCKED period**
- Reversal is BLOCKED with message: "Cannot reverse: Original period {YYYY-MM} is locked"
- Admin must unlock the period first (if allowed per PER-05)
- After reversal, period may be re-locked

**Scenario 3: Original date is in a CLOSED fiscal year**
- Reversal is BLOCKED (cannot unlock closed fiscal year)
- Alternative: Create adjustment entry in current open period
- Document reason with reference to original entry

| Rule ID | Rule                                                                     |
|---------|--------------------------------------------------------------------------|
| REV-P1  | Reversal respects period lock — no bypass mechanism exists.              |
| REV-P2  | Locked period prevents ANY posting, including reversals.                 |
| REV-P3  | To reverse an entry in a locked period: unlock → reverse → re-lock.     |
| REV-P4  | To reverse an entry in a closed fiscal year: use adjustment instead.    |
```

---

## PRIORITY 1 — IMPORTANT (STRONGLY RECOMMENDED)

These should be fixed to prevent implementation confusion.

### ☐ I1: Fix "Gap-Free" Terminology (4 Locations)

#### Location 1: `governance/PROJECT_RULES.md` — Line 77

**CHANGE:**
```markdown
| FIN-09  | Auto-generated codes follow a sequential, gap-free pattern per fiscal year.|
```

**TO:**
```markdown
| FIN-09  | Auto-generated codes are sequential and unique per fiscal year. Gaps may occur on transaction failures.|
```

#### Location 2: `governance/FINANCIAL_ENGINE_RULES.md` — Line 56

**CHANGE:**
```markdown
| 11   | **Generate final number** — sequential, gap-free journal number     | Application  |
```

**TO:**
```markdown
| 11   | **Generate final number** — sequential, unique journal number       | Application  |
```

#### Location 3: `governance/FINANCIAL_ENGINE_RULES.md` — Line 91

**CHANGE:**
```markdown
| JNL-06  | Journal numbers are sequential and gap-free for posted entries.          |
```

**TO:**
```markdown
| JNL-06  | Journal numbers are sequential and unique for posted entries. Gaps may occur if posting fails after number assignment.|
```

#### Location 4: `governance/RECORD_PROTECTION_POLICY.md` — Line 162

**CHANGE:**
```markdown
| ACG-04  | Codes are sequential and gap-free for posted documents.                  |
```

**TO:**
```markdown
| ACG-04  | Codes are sequential and unique for posted documents. Gaps acceptable.    |
```

#### Location 5: `governance/RECORD_PROTECTION_POLICY.md` — Lines 197-199

**CHANGE:**
```markdown
| SEQ-04  | If a posting transaction fails, the sequence number is NOT consumed.     |
| SEQ-05  | Gap detection: system should detect and report gaps in posted sequences.  |
| SEQ-06  | Gaps in draft sequences are acceptable.                                   |
```

**TO:**
```markdown
| SEQ-04  | Sequence number assigned BEFORE validation. If posting fails, number is consumed (gap occurs). |
| SEQ-05  | Sequence assignment uses SQL Server SEQUENCE object for multi-user uniqueness. |
| SEQ-06  | Gaps in posted sequences are normal and do not indicate data corruption.   |
| SEQ-07  | Gaps in draft sequences are acceptable and expected.                        |
```

---

### ☐ I2: Make Fiscal Year Jan-Dec Explicit

#### File: `governance/ACCOUNTING_PRINCIPLES.md` — Line 17

**CHANGE:**
```markdown
| Fiscal Year             | One active year at a time                    |
```

**TO:**
```markdown
| Fiscal Year             | Calendar year: January 1 to December 31 (always) |
| Fiscal Year Rule        | One active year at a time                         |
```

#### File: `governance/FINANCIAL_ENGINE_RULES.md` — After Line 138, ADD:

```markdown
| FY-00   | Fiscal year is ALWAYS a calendar year: January 1 to December 31.         |
| FY-01   | Start date is always January 1. End date is always December 31.          |
```

**RENUMBER existing FY-01 through FY-10 to FY-02 through FY-11**

---

### ☐ I3: Clarify Weighted Average Only

#### File: `governance/ACCOUNTING_PRINCIPLES.md` — Line 211

**CHANGE:**
```markdown
| INV-02  | Supported methods: Weighted Average Cost (default), FIFO.                |
```

**TO:**
```markdown
| INV-02  | Inventory valuation method: Weighted Average Cost (ONLY). FIFO not supported in current design. |
```

---

### ☐ I4: Update Multi-User Assumption

#### File: `PHASE1_COMPLETION.md` — Line 138

**CHANGE:**
```markdown
| T4 | The system will be **single-user initially**, then multi-user via network | ⚠️ YES (or multi-user from start?) |
```

**TO:**
```markdown
| T4 | The system is **multi-user ready from the start**. Concurrency control is mandatory from Phase 2. | ✅ CONFIRMED |
```

#### File: `PHASE1_COMPLETION.md` — Line 154

**CHANGE:**
```markdown
| Q3 | **Multi-user from start**: Is the system initially single-user (local) or multi-user (networked) from Phase 2? | Affects security and concurrency design priority |
```

**TO:**
```markdown
| Q3 | **Multi-user from start**: CONFIRMED — System is multi-user ready. All entities have RowVersion for optimistic locking. | **RESOLVED — Multi-user from Phase 2** |
```

---

## PRIORITY 2 — ENHANCEMENTS (RECOMMENDED)

These improve clarity but are not blocking.

### ☐ E1: Soften 500-Line Rule

#### File: `governance/PROJECT_RULES.md` — Line 37

**CHANGE:**
```markdown
| DEV-06  | **No God classes.** No single class should exceed 500 lines. Split when approaching limit. |
```

**TO:**
```markdown
| DEV-06  | **No God classes.** Classes should target max 500 lines (guideline). Classes exceeding 800 lines require refactoring justification. |
```

**ADD AFTER DEV-06:**
```markdown
| DEV-06a | 500-line target is a maintainability guideline, not an absolute limit.   |
| DEV-06b | Classes between 500-800 lines: acceptable with clear single responsibility. |
| DEV-06c | Classes exceeding 800 lines: must document justification or refactor.    |
```

#### Similar changes in:
- `governance/RISK_PREVENTION_FRAMEWORK.md` — Line 245
- `governance/AGENT_CONTROL_SYSTEM.md` — Line 247

---

### ☐ E2: Add Opening Balance Workflow

#### File: `governance/FINANCIAL_ENGINE_RULES.md` — After Section 7, ADD:

```markdown
## 8. Opening Balance Entry Rules

| Rule ID | Rule                                                                     |
|---------|--------------------------------------------------------------------------|
| OB-01   | Opening balances are entered via a special journal entry type: "Opening Balance". |
| OB-02   | Opening balance entry is created automatically when first fiscal year is created. |
| OB-03   | Opening balance entry date is January 1 of the first fiscal year.        |
| OB-04   | Opening balance entry must balance (Assets = Liabilities + Equity).      |
| OB-05   | Only balance sheet accounts receive opening balances (not Revenue/Expense). |
| OB-06   | Opening balance entry can be edited while Status = Draft.                |
| OB-07   | Once posted, opening balance entry follows all standard immutability rules. |
| OB-08   | Subsequent fiscal years derive opening balances from prior year closing. |
```

---

### ☐ E3: Add Inventory Precision Rules

#### File: `governance/ACCOUNTING_PRINCIPLES.md` — After INV-05, ADD:

```markdown
| INV-06  | Cost per unit precision: 4 decimal places (more precise than financial amounts). |
| INV-07  | Weighted average calculation uses 4-decimal cost, 2-decimal financial amounts.   |
| INV-08  | Unit cost is rounded to 4 decimals using Banker's Rounding after each receipt.  |
| INV-09  | Extended cost (quantity × unit cost) is rounded to 2 decimals.                  |
```

---

## COMPLETION CHECKLIST

Before proceeding to Phase 2:

- [ ] All P0 (Critical) corrections applied
- [ ] All P1 (Important) corrections applied
- [ ] P2 (Enhancement) corrections reviewed and prioritized
- [ ] PHASE1_AUDIT_REPORT.md reviewed and accepted
- [ ] All affected governance documents re-validated
- [ ] CHANGELOG.md updated with correction version
- [ ] Business stakeholders confirm all assumptions

---

**Status After Corrections:**
- [ ] Phase 1 governance is LOCKED and production-ready
- [ ] Phase 2 implementation may begin

---

**Document Version:** 1.0  
**Date:** 2026-02-08  
**Purpose:** Quick reference for applying audit corrections
