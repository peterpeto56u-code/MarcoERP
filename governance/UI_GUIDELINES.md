# MarcoERP – UI Guidelines

**WPF UI Standards, Patterns, and Constraints**

---

## 1. UI Platform

| Property          | Value                               |
|-------------------|-------------------------------------|
| Framework         | WPF (.NET)                          |
| Target OS         | Windows 10/11                       |
| Resolution Target | 1366×768 minimum, 1920×1080 optimal|
| DPI Awareness     | Per-Monitor DPI Aware               |
| Theme             | System default (future skinning)    |

---

## 2. General UI Principles

| Principle ID | Principle                                                              |
|--------------|------------------------------------------------------------------------|
| UI-P1        | **Separation of concerns.** Windows display data and capture input — nothing more. |
| UI-P2        | **Consistency.** All windows follow the same layout, naming, and interaction patterns. |
| UI-P3        | **User feedback.** Every action gives immediate visual feedback (loading, success, error). |
| UI-P4        | **Data safety.** Unsaved changes trigger confirmation before navigation or close. |
| UI-P5        | **Accessibility.** Tab order is logical, keyboard shortcuts are consistent. |
| UI-P6        | **No surprises.** Destructive actions always require confirmation dialogs. |

---

## 3. Window Architecture

### 3.1 Window Types

| Type              | Purpose                                    | Example                      |
|-------------------|--------------------------------------------|------------------------------|
| List Window       | Displays data grid, filter, search         | `AccountListWindow`          |
| Detail Window     | View/edit single record                    | `AccountDetailWindow`        |
| Transaction Window| Multi-line financial entry                 | `JournalEntryWindow`         |
| Dialog Window     | Quick input or confirmation                | `ConfirmPostDialog`          |
| Report Window     | Displays reports with export options       | `TrialBalanceReportWindow`   |
| Settings Window   | Application configuration                  | `FiscalYearSettingsWindow`   |
| Main Window       | Navigation shell with menu                 | `MainWindow`                 |

### 3.2 Window Naming Convention

```
{Entity}{Type}Window
```

Examples:
- `AccountListWindow`
- `AccountDetailWindow`
- `JournalEntryWindow`
- `ConfirmPostDialog`

### 3.3 Shell View Naming (Tabbed/Content Area)

When the UI is hosted inside a single shell window (MainWindow) using tabbed or content-area navigation,
UserControls must follow this naming convention:

```
{Entity}{Type}View
```

Examples:
- `AccountListView`
- `AccountDetailView`
- `JournalEntryView`

**Windows are reserved for modal dialogs only** (confirmations, print preview, etc.).

### 3.3 One Window, One Responsibility

| Rule ID | Rule                                                                     |
|---------|--------------------------------------------------------------------------|
| UIF-01  | Each window handles **one** functional area.                             |
| UIF-02  | No window exceeds 800 lines including XAML + code-behind.                |
| UIF-03  | Complex screens use UserControls for sub-sections.                       |
| UIF-04  | Shared UI logic goes into helper classes, not base windows.              |
| UIF-05  | No window-to-window direct data passing via static fields or globals.    |

---

## 4. Data Binding Rules

| Rule ID | Rule                                                                     |
|---------|--------------------------------------------------------------------------|
| UDB-01  | Windows bind to **DTOs** (from Application layer), never to domain entities.|
| UDB-02  | DataGrid binds to `ObservableCollection<T>` or `CollectionViewSource`.     |
| UDB-03  | CollectionViewSource is the standard mechanism for list filtering/sorting.|
| UDB-04  | No manual population of controls in loops — use data binding.              |
| UDB-05  | ComboBox/ListBox uses `DisplayMemberPath` and `SelectedValuePath`.         |

---

## 5. Layout Standards

### 5.1 Standard Window Layout

```
┌──────────────────────────────────────────────┐
│  Title Bar (Window Title)                     │
├──────────────────────────────────────────────┤
│  Toolbar Area (Action buttons)                │
├──────────────────────────────────────────────┤
│                                               │
│  Content Area                                 │
│  (DataGrid / Window Fields)                   │
│                                               │
├──────────────────────────────────────────────┤
│  Status Bar (Record count, status info)       │
└──────────────────────────────────────────────┘
```

### 5.2 Detail Window Layout

```
┌──────────────────────────────────────────────┐
│  Title Bar                                    │
├──────────────────────────────────────────────┤
│  Toolbar: [Save] [Cancel] [Print] [Delete]   │
├──────────────────────────────────────────────┤
│  ┌─ Header Fields ──────────────────────┐    │
│  │  Code:     [________]  Auto-generated │    │
│  │  Date:     [________]                 │    │
│  │  Status:   [Draft/Posted]             │    │
│  └──────────────────────────────────────┘    │
│  ┌─ Detail Section ─────────────────────┐    │
│  │  DataGrid for line items              │    │
│  └──────────────────────────────────────┘    │
│  ┌─ Totals Section ─────────────────────┐    │
│  │  Subtotal:  [_____]                   │    │
│  │  VAT:       [_____]                   │    │
│  │  Total:     [_____]                   │    │
│  └──────────────────────────────────────┘    │
├──────────────────────────────────────────────┤
│  Status Bar                                   │
└──────────────────────────────────────────────┘
```

### 5.3 Spacing and Margins

| Element            | Value     |
|--------------------|-----------|
| Window padding     | 10px      |
| Label-to-control   | 5px       |
| Between rows       | 8px       |
| Section spacing    | 15px      |
| Button spacing     | 5px       |
| Toolbar height     | 40px      |

### 5.4 Sidebar and Top Bar

| Element                | Value |
|------------------------|-------|
| Sidebar expanded width | 210px |
| Sidebar collapsed width| 72px  |
| Sidebar item min height| 44px  |
| Active item indicator  | Right-edge highlight |

Top bar includes a quick dropdown menu listing accessible screens and global search.

### 5.5 Global Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| Ctrl+K | Command palette / quick search |
| Ctrl+N | New document |
| Ctrl+S | Save |
| Ctrl+E | Edit |
| Ctrl+R | Refresh |
| Ctrl+P | Print |
| F1 | Open search lookup popup on focused ComboBox (SearchLookupWindow) |
| F9 | Post/Submit |
| Esc | Cancel edit |
| Alt+Right / Alt+Left | Next/Previous record (when supported) |
| Ctrl+Tab / Ctrl+Shift+Tab | Next/Previous tab |
| Ctrl+W | Close active tab |

---

## 6. Control Standards

### 6.1 Standard Controls

| Purpose              | Control                  | Notes                          |
|----------------------|--------------------------|--------------------------------|
| Data display         | `DataGrid`               | Read-only by default           |
| Text input           | `TextBox`                | With MaxLength set             |
| Numeric input        | `TextBox` + validation   | Numeric validation + format    |
| Date input           | `DatePicker`             | Format: yyyy-MM-dd             |
| Selection            | `ComboBox`               | IsEditable = false             |
| Boolean              | `CheckBox`               | Clear label text               |
| Action               | `Button`                 | Consistent sizing              |
| Progress             | `ProgressBar`            | For long operations            |
| Grouping             | `GroupBox` / `Panel`     | With clear border/title        |

### 6.2 Control Naming Convention

```
{abbreviation}{Purpose}
```

| Control Type     | Prefix  | Example              |
|------------------|---------|----------------------|
| TextBox          | `txt`   | `txtAccountCode`     |
| ComboBox         | `cmb`   | `cmbAccountType`     |
| Button           | `btn`   | `btnSave`            |
| Label            | `lbl`   | `lblTotal`           |
| DataGrid         | `dg`    | `dgJournalLines`     |
| DatePicker       | `dp`    | `dpPostingDate`      |
| Numeric input    | `num`   | `numAmount`          |
| CheckBox         | `chk`   | `chkIsActive`        |
| GroupBox         | `grp`   | `grpTotals`          |
| Panel            | `pnl`   | `pnlHeader`          |
| TabControl       | `tab`   | `tabDetails`         |
| StatusBar        | `stb`   | `stbMain`            |
| ToolBar          | `tlb`   | `tlbActions`         |
| CollectionViewSource | `cvs` | `cvsAccounts`     |

---

## 7. Transaction Window Rules (Critical)

| Rule ID | Rule                                                                     |
|---------|--------------------------------------------------------------------------|
| UTR-01  | Transaction windows show a clear **Draft** or **Posted** status indicator. |
| UTR-02  | Posted transactions: all fields become **read-only**.                     |
| UTR-03  | Draft transactions: Save, Post, and Delete buttons visible.              |
| UTR-04  | Post button requires confirmation dialog with summary.                   |
| UTR-05  | Posting failure shows detailed error message (which line, what rule).    |
| UTR-06  | Balance mismatch (Debit ≠ Credit) shown with red highlight in real-time.|
| UTR-07  | Auto-calculated totals update as user types (debounced).                 |
| UTR-08  | Line editing uses popup-based **InvoiceAddLineWindow** (not inline DataGrid editing). Add & Next workflow keeps popup open for batch entry. |

---

## 8. Validation Display Rules

| Rule ID | Rule                                                                     |
|---------|--------------------------------------------------------------------------|
| UVL-01  | Required fields marked with `*` and red border on validation failure.    |
| UVL-02  | Validation errors shown in a summary panel or tooltip, not alert boxes.  |
| UVL-03  | Validation runs on field exit (per-field) and on Save (full-window).     |
| UVL-04  | Invalid fields keep focus until corrected or explicitly skipped.         |
| UVL-05  | Business validation errors (from Application layer) shown in error panel.|

---

## 9. Navigation Rules

| Rule ID | Rule                                                                     |
|---------|--------------------------------------------------------------------------|
| UNV-01  | Main menu provides access to all modules (Accounting, Inventory, etc.). |
| UNV-02  | List windows open as tabbed views inside the main window.                |
| UNV-03  | Detail windows open as modal or docked depending on context.             |
| UNV-04  | Double-click on grid row opens the detail window.                        |
| UNV-05  | Back/Close button always available. Escape key closes dialogs.          |
| UNV-06  | Keyboard shortcuts: Ctrl+S (Save), Ctrl+N (New), F5 (Refresh), Del (Delete). |

---

## 10. Long Operation Handling

| Rule ID | Rule                                                                     |
|---------|--------------------------------------------------------------------------|
| ULO-01  | Operations > 1 second show a loading indicator.                          |
| ULO-02  | UI remains responsive during data loading (async/await).                  |
| ULO-03  | Cancel button available for operations > 3 seconds.                      |
| ULO-04  | Progress bar for batch operations (posting, reporting).                  |
| ULO-05  | Status bar shows current operation description.                          |

---

## 11. Message & Dialog Standards

| Type        | Control               | When                                          |
|-------------|-----------------------|-----------------------------------------------|
| Info        | `MessageBox` (Info)   | Operation completed successfully               |
| Warning     | `MessageBox` (Warning)| Unsaved changes, about to close               |
| Error       | Error panel on window | Validation or business rule failure            |
| Confirm     | `MessageBox` (Question)| Before destructive action (Post, Delete)      |
| Fatal Error | Error window          | Unrecoverable exception (with log reference)   |

---

## 12. Forbidden UI Practices

| #  | Forbidden Practice                                                      |
|----|-------------------------------------------------------------------------|
| 1  | Performing business calculations in code-behind                         |
| 2  | Direct SQL or DbContext access from windows                             |
| 3  | Showing raw exception messages to users                                 |
| 4  | Using `Thread.Sleep` for timing or delays                               |
| 5  | Storing application state in static variables                           |
| 6  | Creating windows with more than 30 fields without grouping              |
| 7  | Nesting more than 2 modal dialogs deep                                 |
| 8  | Using `DispatcherFrame`/manual DoEvents for responsiveness              |
| 9  | Hard-coding display text — use resource files for all labels            |
| 10 | Ignoring window disposal — all windows implement `IDisposable` properly |

---

## 13. Shared UI Components

| Component | Location | Purpose |
|-----------|----------|---------|
| `InvoiceLinePopupState` | ViewModels/Common/ | Shared popup state for add/edit line across all invoice types (Sale, Purchase, Return). Supports dual-unit entry, profit calc, smart-entry data. |
| `InvoiceAddLineWindow` | Views/Common/ | Shared popup window bound to `LinePopup.*`. Supports Add & Next workflow. |
| `SearchLookupWindow` | Views/Common/ | Reusable F1 search popup with real-time substring filter, auto-columns, DataGrid results. |
| `F1SearchBehavior` | Common/ | Attached behavior — press F1 on any ComboBox to open `SearchLookupWindow`. |
| `QuickTreasuryDialog` | Views/Common/ | Quick cash receipt/payment from invoice detail. |
| `InvoicePdfPreviewDialog` | Views/Common/ | Invoice PDF preview. |
| `IInvoiceLineFormHost` | ViewModels/ | Interface providing Products collection + line calculation contract. |
| `IDirtyStateAware` | Navigation/ | Interface for dirty-state navigation protection. Detail VMs implement this to guard unsaved changes. |

---

## 14. Popup-Based Line Editing Standard

All invoice-type detail views (Sales Invoice, Purchase Invoice, Sales Return, Purchase Return) follow this pattern:
1. **Add Line**: Button opens `InvoiceAddLineWindow` via `OpenAddLinePopupCommand`. Popup loops ("Add & Next") until user cancels.
2. **Edit Line**: Row edit button opens same popup via `EditLineCommand` with pre-loaded data.
3. **Delete Line**: Row delete button with confirmation.
4. **DataGrid**: Read-only display only (no inline cell editing).
5. **InvoicePopupMode**: `Sale` shows profit columns + sale price hint; `Purchase` hides profit + shows purchase price hint.

---

## Version History

| Version | Date       | Change Description                    |
|---------|------------|---------------------------------------|
| 1.0     | 2026-02-08 | Initial Phase 1 governance release    |
| 1.1     | 2026-02-11 | Allow shell-based View naming alongside Window naming |
| 1.2     | 2026-02-11 | Add F1 search, popup editing, shared components, sidebar 210px |
