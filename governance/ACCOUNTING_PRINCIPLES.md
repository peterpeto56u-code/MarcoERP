# MarcoERP – Accounting Principles

**Double Entry Enforcement, Accounting Logic, and Financial Foundation**

---

## 1. Accounting Model

| Property                | Value                                        |
|-------------------------|----------------------------------------------|
| Accounting Method       | Full Double-Entry Bookkeeping                |
| Basis                   | Accrual Basis                                |
| Account Structure       | Hierarchical Chart of Accounts               |
| Currency                | Single currency (system-wide)                |
| VAT Model               | Inclusive or Exclusive (configurable)         |
| Decimal Precision       | 2 decimal places (configurable)              |
| Fiscal Year             | Calendar year: January 1 to December 31 (always) |
| Fiscal Year Rule        | One active year at a time                         |
| Periods                 | 12 monthly periods per fiscal year           |
| Cost Centers            | Optional, tag-level association               |

---

## 2. The Fundamental Equation

> **Assets = Liabilities + Equity**

Every transaction in MarcoERP must preserve this equation. This is enforced by the double-entry system — every financial entry must produce balanced debits and credits.

> **Total Debits = Total Credits** (for every transaction, always)

---

## 3. Chart of Accounts Structure

### 3.1 Account Classification

| Level | Type                      | Code Range    | Natural Balance |
|-------|---------------------------|---------------|-----------------|
| 1     | Assets                    | 1xxx          | Debit           |
| 2     | Liabilities               | 2xxx          | Credit          |
| 3     | Equity                    | 3xxx          | Credit          |
| 4     | Revenue                   | 4xxx          | Credit          |
| 5     | Cost of Goods Sold (COGS) | 5xxx          | Debit           |
| 6     | Expenses                  | 6xxx          | Debit           |
| 7     | Other Income              | 7xxx          | Credit          |
| 8     | Other Expenses            | 8xxx          | Debit           |

### 3.2 Account Properties

| Property         | Description                                              |
|------------------|----------------------------------------------------------|
| AccountCode      | Unique, alphanumeric (e.g., `1100`, `1100-001`)          |
| AccountName      | Human-readable name                                       |
| AccountType      | Enum: Asset, Liability, Equity, Revenue, Expense          |
| ParentAccountId  | Reference to parent account (for sub-accounts)           |
| NormalBalance    | Debit or Credit (determined by AccountType)               |
| IsActive         | Whether the account can receive new postings             |
| IsSystemAccount  | Protected accounts (cannot be deleted or modified)        |
| Level            | Hierarchy depth (1 = top-level)                          |
| AllowPosting     | Whether direct posting is allowed (false for parent/group)|
| CurrencyCode     | ISO 4217 currency code                                    |

### 3.3 Account Rules

| Rule ID | Rule                                                                     |
|---------|--------------------------------------------------------------------------|
| ACC-01  | Account codes must be unique across the entire chart.                    |
| ACC-02  | System accounts (Cash, AR, AP, VAT, etc.) cannot be deleted.            |
| ACC-03  | Accounts with posted transactions cannot be deleted — only deactivated.  |
| ACC-04  | Parent accounts cannot receive direct postings.                          |
| ACC-05  | Account type determines the natural balance (Debit or Credit).           |
| ACC-06  | Account hierarchy depth is limited to 4 levels.                          |
| ACC-07  | Changing account type after posting is **forbidden**.                     |

---

## 4. Double-Entry Enforcement Rules

### 4.1 The Core Rule

| Rule ID | Rule                                                                     |
|---------|--------------------------------------------------------------------------|
| DEB-01  | **Every journal entry MUST have Total Debits = Total Credits.**          |
| DEB-02  | A journal entry with a single line is **forbidden**.                     |
| DEB-03  | Zero-amount lines are **forbidden** in journal entries.                  |
| DEB-04  | Negative amounts are **forbidden**. Use the opposite side (Debit/Credit).|
| DEB-05  | Each journal line must reference exactly one account.                    |
| DEB-06  | Each journal line is either Debit OR Credit, never both.                 |
| DEB-07  | Balance verification happens at the **Domain layer** before persistence. |

### 4.2 Balance Verification

Before any journal entry can be saved (even as draft):

```
SUM(DebitAmounts) must equal SUM(CreditAmounts)
```

This check is:
- Enforced in the Domain entity (JournalEntry.Validate())
- Re-verified in the Application service before persistence
- Stored as an assertion at the database level (check constraint)

### 4.3 Debit/Credit Rules by Account Type

| Account Type | Increase By | Decrease By |
|--------------|-------------|-------------|
| Asset        | Debit       | Credit      |
| Liability    | Credit      | Debit       |
| Equity       | Credit      | Debit       |
| Revenue      | Credit      | Debit       |
| Expense      | Debit       | Credit      |

---

## 5. Journal Entry Structure

### 5.1 Journal Entry (Header)

| Field              | Description                                         |
|--------------------|-----------------------------------------------------|
| JournalNumber      | Auto-generated sequential number per fiscal year     |
| JournalDate        | Transaction date (must fall within open period)      |
| PostingDate        | Date when posted (set on posting)                    |
| Description        | Narrative describing the transaction                 |
| ReferenceNumber    | External reference (invoice number, etc.)            |
| Status             | Draft / Posted / Reversed                            |
| SourceType         | Manual / SalesInvoice / PurchaseInvoice / Inventory  |
| SourceId           | Reference to originating document                    |
| FiscalYearId       | Link to the active fiscal year                       |
| PeriodId           | Link to the fiscal period                            |
| CostCenterId       | Optional cost center tag                             |

### 5.2 Journal Entry Line (Detail)

| Field              | Description                                         |
|--------------------|-----------------------------------------------------|
| LineNumber         | Sequential line number within the entry              |
| AccountId          | Reference to the Chart of Accounts                   |
| DebitAmount        | Debit amount (0 if this is a credit line)            |
| CreditAmount       | Credit amount (0 if this is a debit line)            |
| Description        | Line-level narrative                                 |
| CostCenterId       | Optional cost center tag (overrides header)          |
| WarehouseId        | Warehouse reference (for inventory-related entries)  |

---

## 6. VAT Handling

### 6.1 VAT Principles

| Rule ID | Rule                                                                     |
|---------|--------------------------------------------------------------------------|
| VAT-01  | VAT calculation is centralized in `ILineCalculationService` (Application layer). No arithmetic in ViewModels or UI. |
| VAT-02  | VAT rates are stored as master data, not hardcoded.                      |
| VAT-03  | Each product/service has an assigned VAT category.                       |
| VAT-04  | VAT is calculated per line item, then totaled.                           |
| VAT-05  | VAT generates automatic journal entries:                                  |
|         | - Sales: Credit to VAT Output (Liability)                                |
|         | - Purchases: Debit to VAT Input (Asset)                                  |
| VAT-06  | VAT-exempt transactions must explicitly set VAT rate to 0%.              |
| VAT-07  | VAT amounts use the same decimal precision as all financial amounts.     |

### 6.2 VAT Calculation

```
For VAT-Exclusive pricing:
  Line VAT = Line Amount × VAT Rate
  Line Total = Line Amount + Line VAT

For VAT-Inclusive pricing:
  Line VAT = Line Total × (VAT Rate / (1 + VAT Rate))
  Line Amount = Line Total - Line VAT
```

### 6.3 VAT Account Mapping

| Transaction Type  | VAT Account              | Side    |
|-------------------|--------------------------|---------|
| Sales Invoice     | VAT Output Payable       | Credit  |
| Purchase Invoice  | VAT Input Receivable     | Debit   |
| VAT Settlement    | VAT Output → VAT Input   | Both    |

---

## 7. Inventory-Accounting Synchronization

### 7.1 The Core Principle

> **Every inventory movement generates a corresponding accounting entry.**

Inventory is not just a quantity ledger — it is financially tracked through the accounting system.

### 7.2 Inventory Movement → Journal Entry Mapping

| Inventory Event        | Debit Account         | Credit Account        |
|------------------------|-----------------------|-----------------------|
| Purchase Receipt       | Inventory (Asset)     | Accounts Payable      |
| Sales Delivery         | COGS (Expense)        | Inventory (Asset)     |
| Warehouse Transfer     | Dest Warehouse Inv    | Source Warehouse Inv  |
| Inventory Adjustment+  | Inventory (Asset)     | Inventory Adjustment  |
| Inventory Adjustment-  | Inventory Adjustment  | Inventory (Asset)     |
| Stock Count Gain       | Inventory (Asset)     | Inventory Adjustment  |
| Stock Count Loss       | Inventory Adjustment  | Inventory (Asset)     |

### 7.3 Inventory Valuation

| Rule ID | Rule                                                                     |
|---------|--------------------------------------------------------------------------|
| INV-01  | Inventory valuation method must be declared and consistent.              |
| INV-02  | Inventory valuation method: Weighted Average Cost (ONLY). FIFO not supported in current design. |
| INV-03  | Valuation method change requires fiscal year closure and approval.       |
| INV-04  | Cost per unit is recalculated on each receipt (Weighted Average).        |
| INV-05  | Negative stock is **forbidden** (configurable: warn or block).           |
| INV-06  | Cost per unit precision: 4 decimal places (more precise than financial amounts). |
| INV-07  | Weighted average calculation uses 4-decimal cost, 2-decimal financial amounts.   |
| INV-08  | Unit cost is rounded to 4 decimals using Banker's Rounding after each receipt.  |
| INV-09  | Extended cost (quantity × unit cost) is rounded to 2 decimals.                  |

### 7.4 Per-Warehouse Ledger

| Rule ID | Rule                                                                     |
|---------|--------------------------------------------------------------------------|
| WHS-01  | Each warehouse maintains its own inventory quantities.                   |
| WHS-02  | Each warehouse has its own inventory GL account (sub-account).           |
| WHS-03  | Transfers between warehouses generate accounting entries.                |
| WHS-04  | Warehouse totals must equal the main inventory control account.          |

---

## 8. Multi-Cashbox Rules

| Rule ID | Rule                                                                     |
|---------|--------------------------------------------------------------------------|
| CSH-01  | Each cashbox has a dedicated GL cash account.                            |
| CSH-02  | Cash receipts and payments are posted to the specific cashbox account.   |
| CSH-03  | Cashbox balance must never go negative.                                  |
| CSH-04  | Cash transfers between cashboxes generate journal entries.               |
| CSH-05  | Daily cashbox balance reconciliation should be supported.                |

---

## 9. Cost Center Rules

| Rule ID | Rule                                                                     |
|---------|--------------------------------------------------------------------------|
| CC-01   | Cost centers are **optional** — the system works without them.           |
| CC-02   | When enabled, cost centers are tags on journal entry lines.              |
| CC-03   | Cost centers do not affect the Chart of Accounts structure.              |
| CC-04   | Reports can filter/group by cost center.                                 |
| CC-05   | Cost center assignment is not mandatory on any transaction.              |
| CC-06   | Revenue and expense accounts are the primary cost center targets.        |

---

## 10. Rounding Rules

| Rule ID | Rule                                                                     |
|---------|--------------------------------------------------------------------------|
| RND-01  | All financial amounts are rounded to 2 decimal places (configurable).    |
| RND-02  | Rounding method: Banker's rounding (MidpointRounding.ToEven).            |
| RND-03  | Rounding differences are posted to a designated rounding account.        |
| RND-04  | VAT rounding follows the same rules.                                     |
| RND-05  | Line-level rounding before totaling (not total-level rounding).          |

---

## 11. Reconciliation Principles

| Reconciliation Type     | Description                                          |
|-------------------------|------------------------------------------------------|
| Trial Balance           | Sum of all debits must equal sum of all credits      |
| Bank Reconciliation     | Bank statement vs. GL bank account balance           |
| Cashbox Reconciliation  | Physical cash vs. GL cashbox balance                 |
| Inventory Reconciliation| Physical stock vs. GL inventory balance              |
| AR/AP Aging             | Outstanding balances vs. GL control accounts         |

---

## Version History

| Version | Date       | Change Description                    |
|---------|------------|---------------------------------------|
| 1.0     | 2026-02-08 | Initial Phase 1 governance release    |
