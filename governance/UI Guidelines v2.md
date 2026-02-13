MarcoERP – Unified UI Governance Document

Version 2.0 – Commercial Enterprise Standard
Date: 2026-02-11

1️⃣ UI Platform
Property	Value
Framework	WPF (.NET 8)
Target OS	Windows 10/11
Resolution	1366×768 min – 1920×1080 optimal
DPI	Per-Monitor DPI Aware
Theme	Default (future theming supported)
RTL	Fully supported
2️⃣ Core UI Philosophy
ID	Principle
UI-P1	Strict separation of concerns (No business logic in UI).
UI-P2	Module-based consistency across entire system.
UI-P3	Immediate visual feedback for every action.
UI-P4	Data safety first (Unsaved change protection mandatory).
UI-P5	Keyboard-first usability.
UI-P6	No destructive action without confirmation.
UI-P7	Commercial-grade performance required.
UI-P8	Scalable navigation architecture.
3️⃣ Module-Based Navigation Architecture
3.1 Mandatory Modules

Every screen MUST belong to one of:

Sales

Purchases

Inventory

Treasury

Accounting

Reports

Settings

No screen may exist outside a module.

3.2 Sidebar Rules
Rule ID	Description
MN-01	Reports live under their parent module only.
MN-02	No duplicated report entries.
MN-03	POS belongs to Sales only.
MN-04	NavigationService registers views by module.
MN-05	Sidebar supports collapse animation.
MN-06	Tabbed document hosting inside MainWindow only.
MN-07	Sidebar items use clear active-state highlight and minimum hit height 44px.
MN-08	Sidebar expanded width target 300px; collapsed width target 72px.
4️⃣ Window Architecture
4.1 Main Window

MainWindow = Navigation shell

Hosts tabbed views

No direct OS-level window spawning unless modal dialog

Windows may be implemented as tab-hosted views inside MainWindow.

4.2 Window Types
Type	Purpose
ListWindow	Data grid with filtering
DetailWindow	Single entity edit
TransactionWindow	Multi-line financial entry
QuotationWindow	Sales/Purchase offers
PriceListWindow	Customer pricing
BulkUpdateWindow	Batch update
SessionWindow	POS session
ReportWindow	Reporting/export
DialogWindow	Confirmation/input
MainWindow	Navigation shell
4.3 Naming Convention
{Entity}{Type}Window


Examples:

SalesInvoiceWindow

AccountListWindow

SalesQuotationWindow

4.4 One Window – One Responsibility
Rule ID	Description
UIF-01	One functional area per window
UIF-02	Complex windows must use UserControls
UIF-03	No business calculations in code-behind
UIF-04	No direct DbContext access
UIF-05	No static/global state passing
UIF-06	Max logical complexity controlled (group large forms)
5️⃣ Data Binding Rules
Rule ID	Description
UDB-01	Bind only to Application DTOs
UDB-02	Use ObservableCollection
UDB-03	Use CollectionViewSource
UDB-04	No manual control population loops
UDB-05	ComboBox uses DisplayMemberPath & SelectedValuePath
6️⃣ Layout Standards
Standard Layout

Title
Toolbar (40px height)
Content
Status bar

Spacing:

Window padding: 10px

Row spacing: 8px

Section spacing: 15px

Button spacing: 5px

Top Bar:

Includes a quick dropdown menu that lists all accessible modules/screens.

Displays global search and system status icons.

7️⃣ Full-Screen Transaction Standard

Applies to Sales & Purchase transactions.

Must support:

Full-screen mode

Draft / Posted badge

Sticky totals section

Real-time debounced totals

Inline validation

Quick payment dialog (if applicable)

Balance mismatch highlight (real-time)

Posted → Read-only
Delete allowed only in Draft

8️⃣ Dirty State Protection (MANDATORY)
Rule ID	Description
DSP-01	All transaction windows implement UnsavedChangesGuard
DSP-02	Close requires confirmation if dirty
DSP-03	Navigation blocked if unsaved
DSP-04	Warning dialog used
9️⃣ Smart Entry UX Rules
ID	Description
UX-01	Enter moves to next field
UX-02	Enter on last column adds line
UX-03	Esc cancels editing
UX-04	F2 edits selected line
UX-05	Numeric fields select-all on focus
UX-06	Barcode auto-detection
UX-07	Last customer price shown
UX-08	Live profit margin highlight

12️⃣ Keyboard Shortcuts (Global)

Ctrl+K: Command palette / quick search

Ctrl+N: New document

Ctrl+S: Save

Ctrl+E: Edit

Ctrl+R: Refresh

Ctrl+P: Print

F9: Post/Submit

Esc: Cancel edit

Alt+Right/Alt+Left: Next/Previous record (when supported)

Ctrl+Tab / Ctrl+Shift+Tab: Next/Previous tab

Ctrl+W: Close active tab
🔟 Pricing UI Standards

Customer Price List must support:

Filter by Supplier

Filter by Category

All products / In-stock only

Bulk percentage update

Manual override

Preview before save

Export PDF

Visual pricing priority clarity

11️⃣ Quotation UI Standards

SalesQuotationWindow & PurchaseQuotationWindow:

Expiry date required

Status badge (Draft / Approved / Expired / Converted)

Convert to Invoice button

Conversion confirmation

Expired highlight

Quotations do NOT affect:

Stock

Journal

Accounting balances

12️⃣ POS Session UI Standard

Session must open before sale

Close with cash count

Mismatch highlighted red

Printable session summary

13️⃣ Validation Rules
Rule ID	Description
UVL-01	Required fields marked *
UVL-02	Errors shown in summary panel
UVL-03	Field validation on exit
UVL-04	Business errors shown in error panel
UVL-05	No raw exception messages
14️⃣ Performance Standards
Rule ID	Description
PERF-01	Async/await mandatory
PERF-02	No blocking UI thread
PERF-03	Paging required if > 500 rows
PERF-04	Lazy loading tabs
PERF-05	Debounced calculations
PERF-06	CollectionViewSource standard
15️⃣ Global Search & Command Palette

Ctrl+K opens command palette.

Supports search in:

Customers

Products

Invoices

Journal entries

Grouped by module.

16️⃣ Message & Dialog Standards
Type	Usage
Info	Success operations
Warning	Unsaved changes
Confirm	Post/Delete
Error	Business rule failures
Fatal	Unrecoverable exception with log reference
17️⃣ Forbidden UI Practices

Business logic in UI

Direct SQL in windows

Thread.Sleep

Static global state

Hard-coded text

2 modal nesting

Large ungrouped forms

Blocking UI thread

Raw exception exposure

Ignoring IDisposable

18️⃣ Governance Enforcement

Any new screen MUST:

Declare module

Follow naming convention

Implement UnsavedChangesGuard if transactional

Bind to DTOs only

Respect Clean Architecture

Pass governance review before merge

19️⃣ Commercial UX Enhancements

Sidebar collapse animation

Sticky totals bar

Theme toggle ready

Notification indicator

Highlight negative profit

Highlight overdue credit

Final Statement

This document defines the commercial UI standard for MarcoERP.

No feature may bypass these standards.
No UI element may violate separation of concerns.
All enhancements must align with module structure and accounting integrity.