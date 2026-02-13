📘 MarcoERP – Accounting Principles v1.1 (Commercial-Ready Edition)

مش حذف…
ولا كسر…
ولا تغيير جذري…

ده توسعة محاسبية ذكية عشان تدعم:

Credit Control

Sales Representative

Profit Analysis

Quotation System

Customer Price Lists

Commercial Controls

Session Control

Advanced Reconciliation

Version History
Version	Date	Change Description
1.0	2026-02-08	Initial governance release
1.1	2026-02-10	Commercial upgrade – Credit Control, Quotation, Profit Tracking, Commercial Enhancements
1. Accounting Model (Updated)
Property	Value
Accounting Method	Full Double Entry
Basis	Accrual
Currency	Single Currency
VAT	Inclusive/Exclusive
Inventory Valuation	Weighted Average Only
Credit Control	Supported (NEW)
Sales Commission	Supported (NEW)
Quotation Documents	Non-posting documents (NEW)
2. Fundamental Equation (Unchanged)

Assets = Liabilities + Equity
Total Debits = Total Credits

⚠ لا تغيير.

3. NEW – Commercial Control Layer
3.1 Credit Control Rules
Rule ID	Rule
CRD-01	Each customer may have a CreditLimit.
CRD-02	CreditLimit validation occurs BEFORE invoice posting.
CRD-03	Credit warning may block posting (configurable).
CRD-04	Overdue invoices may block new sales (configurable).
CRD-05	Credit control does NOT modify journal logic.

⚠ Important:
Credit logic lives in Application layer, not Domain accounting core.

3.2 Customer Balance Definition

Customer Balance =
Total Sales – Total Receipts – Total Credit Notes

Displayed live before invoice confirmation.

4. NEW – Quotation Accounting Rule

Quotations:

DO NOT create journal entries

DO NOT affect inventory

DO NOT affect AR/AP

Rule ID	Rule
QTN-01	SalesQuotation is a non-posting document.
QTN-02	Conversion to Invoice creates accounting effect.
QTN-03	Expired quotations cannot be converted.
5. NEW – Profit & Margin Logic

Profit is NOT a stored value.
It is calculated dynamically.

5.1 Profit Formula

For each Sales Line:

Profit = (Selling Price – Weighted Average Cost) × Quantity

Rule	Description
PFT-01	Profit is informational, not journal-based.
PFT-02	Profit cannot modify posting values.
PFT-03	Negative profit must trigger warning (optional).
6. NEW – Sales Representative (Commission)

Commission is calculated:

After posting

Based on Sales or Profit (configurable)

Rule	Description
COM-01	Commission does not affect revenue recognition.
COM-02	Commission may generate Expense entry when approved.
COM-03	Commission payable account must be configured.
7. Inventory Rules (Clarified)
7.1 Negative Stock
Mode	Behavior
Strict	Block transaction
Warning	Allow with warning
Disabled	Allow (not recommended)

Default: Strict.

8. NEW – Customer Price List Logic
Rule	Description
CPL-01	Customer-specific price overrides master price.
CPL-02	Tier pricing applies before customer override unless configured otherwise.
CPL-03	Price lists do NOT create accounting entries.
CPL-04	Expired price lists ignored automatically.
9. VAT (Clarified)

VAT rounding remains:

Line level first

Then total

Banker's rounding

VAT settlement remains separate journal entry.

10. Cashbox & POS Enhancements
10.1 POS Session Rules
Rule	Description
POS-01	Session must be open to allow cash sale.
POS-02	Closing session requires reconciliation.
POS-03	Cash difference posted to Over/Short account.
11. Advanced Reconciliation (NEW)

System must support:

AR Aging Report

AP Aging Report

VAT Reconciliation

Inventory GL vs Warehouse Check

Commission Payable Reconciliation

12. Rounding (Unchanged)

Financial: 2 decimals

Inventory cost: 4 decimals

Banker’s rounding

13. NEW – Accounting Integrity Enforcement
Rule	Description
INT-01	Any document conversion must preserve financial neutrality until posting.
INT-02	No commercial feature may bypass JournalEntry validation.
INT-03	Application layer cannot override Domain balance rules.
INT-04	AuditLog must capture commercial overrides (credit approval, etc.).
14. Forbidden Accounting Practices
#	Forbidden
1	Direct journal manipulation from UI
2	Deleting posted entries
3	Editing posted entries
4	Changing valuation method mid-year
5	Storing calculated profit permanently
🔥 أهم إضافة فك