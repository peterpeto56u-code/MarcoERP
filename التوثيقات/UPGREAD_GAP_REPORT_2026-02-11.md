# MarcoERP — تقرير مقارنة (الموجود vs المطلوب) وخطة التنفيذ

**التاريخ:** 2026-02-11  
**المصدر الأساسي للمتطلبات:** [upgread.md](upgread.md)

## 1) ملخص تنفيذي (Executive Summary)
- أغلب متطلبات **المراحل 1–3** المذكورة في [upgread.md](upgread.md) موجودة بالفعل في المشروع (خصوصًا داخل WpfUI) مع تطبيق فعلي لـ: List/Detail split، Smart Entry، Keyboard navigation، PDF preview، تنقل فواتير، وDirty-state guard.
- هناك متطلبات **متقدمة** (Sales Representative / Price Lists / Credit Control / POS Sessions) يظهر من الكود والمهاجرشن (migrations) أنها **موجودة جزئيًا أو بالكامل** بالفعل.
- أهم الفجوات الحالية ليست في “وجود الشاشة”، بل في **الربط/الضوابط** (مثال: Credit Control كمنع بيع/موافقة مدير) وميزات غير موجودة مثل **شاشة تعديل أسعار مجمع (Bulk Update)**.

> ملاحظة جودة: آخر سياق تشغيل كان `dotnet test` ناجح (356 tests passed) بحسب سجل العمل الحالي.

---

## 2) ما هو “الموجود” اليوم؟ (Baseline)
### 2.1 هيكلة الشاشات (List/Detail) للفواتير والمرتجعات
- Routes مسجلة في DI + ViewRegistry للفواتير والمرتجعات:
  - [Sales/Purchases routes](src/MarcoERP.WpfUI/App.xaml.cs#L484-L497)
- ViewRegistry يقوم بإنشاء الـView والـViewModel من DI بدون إنشاء VM في code-behind:
  - [ViewRegistry](src/MarcoERP.WpfUI/Navigation/ViewRegistry.cs#L9-L46)

### 2.2 Navigation Guard (Dirty State)
- NavigationService يمنع التنقل عند وجود تغييرات غير محفوظة (IDirtyStateAware):
  - [Dirty-state guard](src/MarcoERP.WpfUI/Navigation/NavigationService.cs#L31-L47)
- تعريف الواجهة:
  - [IDirtyStateAware](src/MarcoERP.WpfUI/Navigation/IDirtyStateAware.cs#L7-L14)

### 2.3 Smart Entry + Keyboard Navigation
- سلوك DataGrid Smart Entry: Enter/F2/Delete/Esc + إضافة بند آخر الصف:
  - [DataGridSmartEntryBehavior](src/MarcoERP.WpfUI/Common/UiBehaviors.cs#L247-L418)
- SelectAllOnFocus لحقل السعر/الكمية…:
  - [SelectAllOnFocusBehavior](src/MarcoERP.WpfUI/Common/UiBehaviors.cs#L202-L245)

### 2.4 PDF Viewer داخلي + تنقل الفواتير
- زر PDF موجود داخل شاشة الفاتورة + خدمة عرض PDF:
  - [Sales Print button](src/MarcoERP.WpfUI/Views/Sales/SalesInvoiceDetailView.xaml#L113-L116)
  - [InvoicePdfPreviewService](src/MarcoERP.WpfUI/Services/InvoicePdfPreviewService.cs#L14-L70)
- PDF dialog داخلي يعتمد WebView2 ويطبع HTML إلى PDF:
  - [InvoicePdfPreviewDialog PrintToPdfAsync](src/MarcoERP.WpfUI/Views/Common/InvoicePdfPreviewDialog.xaml.cs#L58-L86)
- تنقل (التالي/السابق) + Jump-to-invoice-number:
  - [Sales nav + jump](src/MarcoERP.WpfUI/Views/Sales/SalesInvoiceDetailView.xaml#L122-L146)

### 2.5 تكامل الخزينة (Treasury Integration)
- خدمة تكامل تنشئ Receipt/Payment بعد حفظ فاتورة (Dialog اختياري):
  - [PromptAndCreateSalesReceiptAsync](src/MarcoERP.WpfUI/Services/InvoiceTreasuryIntegrationService.cs#L31-L62)
  - [PromptAndCreatePurchasePaymentAsync](src/MarcoERP.WpfUI/Services/InvoiceTreasuryIntegrationService.cs#L64-L95)
- ربط هذا التدفق في ViewModels:
  - [Sales PromptCreateReceiptFromInvoiceAsync](src/MarcoERP.WpfUI/ViewModels/Sales/SalesInvoiceDetailViewModel.cs#L604-L660)
  - [Purchase PromptCreatePaymentFromInvoiceAsync](src/MarcoERP.WpfUI/ViewModels/Purchases/PurchaseInvoiceDetailViewModel.cs#L420-L460)

---

## 3) مصفوفة المقارنة (المطلوب vs الموجود)

التقييم:
- ✅ **منفّذ** (Verified)
- ⚠️ **منفّذ جزئيًا** (Partial)
- ❌ **غير موجود/غير واضح** (Not found)

| البند المطلوب (من upgread) | الحالة | الأدلة داخل الكود | الملاحظات / المطلوب لاستكماله |
|---|---:|---|---|
| Phase1: فصل SalesInvoice إلى List + Full Screen Editor | ✅ | [Routes](src/MarcoERP.WpfUI/App.xaml.cs#L484-L485), [Views](src/MarcoERP.WpfUI/Views/Sales/SalesInvoiceListView.xaml), [Detail](src/MarcoERP.WpfUI/Views/Sales/SalesInvoiceDetailView.xaml) | موجود كـ ListView وDetailView بالفعل.
| Phase1: نفس الفصل لـ PurchaseInvoice | ✅ | [Routes](src/MarcoERP.WpfUI/App.xaml.cs#L494-L495), [Views](src/MarcoERP.WpfUI/Views/Purchases/PurchaseInvoiceListView.xaml), [Detail](src/MarcoERP.WpfUI/Views/Purchases/PurchaseInvoiceDetailView.xaml) | موجود.
| Phase1: نفس الفصل لـ SalesReturn / PurchaseReturn | ✅ | [SalesReturn routes](src/MarcoERP.WpfUI/App.xaml.cs#L486-L487), [PurchaseReturn routes](src/MarcoERP.WpfUI/App.xaml.cs#L496-L497) | موجود List/Detail للمرتجعات.
| Phase1: التنقل (فتح فاتورة من القائمة + إنشاء جديد) | ✅ | [SalesInvoiceListViewModel](src/MarcoERP.WpfUI/ViewModels/Sales/SalesInvoiceListViewModel.cs#L86-L92), [PurchaseInvoiceListViewModel](src/MarcoERP.WpfUI/ViewModels/Purchases/PurchaseInvoiceListViewModel.cs#L43-L44) | موجود: NavigateTo مع parameter id.
| Phase1: MVVM بدون code-behind لإنشاء ViewModels | ✅ | [ViewRegistry DI](src/MarcoERP.WpfUI/Navigation/ViewRegistry.cs#L13-L29) | الـViewModel يتم حقنه من DI.
| Phase1: Layout Header/Body/Footer ومساحة البنود | ✅ | [SalesInvoiceDetailView](src/MarcoERP.WpfUI/Views/Sales/SalesInvoiceDetailView.xaml#L1-L210) | واضح تقسيم Header/Error/Body/Totals.
| Phase2: عرض المخزون/آخر بيع/آخر شراء/متوسط تكلفة عند اختيار الصنف | ✅ | [Smart columns](src/MarcoERP.WpfUI/Views/Sales/SalesInvoiceDetailView.xaml#L302-L305), [SmartEntry queries](src/MarcoERP.WpfUI/ViewModels/Sales/SalesInvoiceDetailViewModel.cs#L449-L470) | يتم الجلب عبر `ISmartEntryQueryService`.
| Phase2: Highlight فرق السعر | ✅ | [IsPriceDifferentFromLastSale trigger](src/MarcoERP.WpfUI/Views/Sales/SalesInvoiceDetailView.xaml#L323-L343) | موجود في Sales.
| Phase2: SelectAll + Focus + Enter next | ✅ | [SelectAllOnFocus usage](src/MarcoERP.WpfUI/Views/Sales/SalesInvoiceDetailView.xaml#L313-L349), [SelectAll behavior](src/MarcoERP.WpfUI/Common/UiBehaviors.cs#L202-L245) | SelectAll موجود؛ التنقل يتم عبر SmartEntry DataGrid.
| Phase2: Enter آخر حقل يضيف بند | ✅ | [SmartEntry behavior](src/MarcoERP.WpfUI/Common/UiBehaviors.cs#L334-L389) | AddLineCommand يتم استدعاؤه عند نهاية الصف.
| Phase2: Keyboard shortcuts (Enter/F2/Delete/Esc) | ✅ | [Shortcuts](src/MarcoERP.WpfUI/Common/UiBehaviors.cs#L298-L332) | مطابق للمتطلبات.
| Phase2: عرض Previous Balance + Outstanding + تنبيه متأخرات | ✅ | [UI panel](src/MarcoERP.WpfUI/Views/Sales/SalesInvoiceDetailView.xaml#L222-L259), [Queries](src/MarcoERP.WpfUI/ViewModels/Sales/SalesInvoiceDetailViewModel.cs#L472-L509) | موجود (عرض + حساب متأخرات عبر DaysAllowed).
| Phase2: تحسين الأداء (Compiled Queries… إلخ) | ⚠️ | لا يوجد دليل مباشر في WpfUI (التحسين عادة في Persistence) | يلزم تدقيق Persistence/SmartEntry services للتأكد من compiled queries فعليًا.
| Phase3: بعد حفظ SalesInvoice Dialog تحصيل اختياري + Receipt | ✅ | [Treasury integration](src/MarcoERP.WpfUI/Services/InvoiceTreasuryIntegrationService.cs#L31-L62), [Sales prompt](src/MarcoERP.WpfUI/ViewModels/Sales/SalesInvoiceDetailViewModel.cs#L604-L660) | موجود.
| Phase3: بعد حفظ PurchaseInvoice خيار دفع فوري + Payment | ✅ | [Treasury integration](src/MarcoERP.WpfUI/Services/InvoiceTreasuryIntegrationService.cs#L64-L95), [Purchase prompt](src/MarcoERP.WpfUI/ViewModels/Purchases/PurchaseInvoiceDetailViewModel.cs#L420-L460) | موجود.
| Part4: PDF Viewer داخلي + تنقل الفواتير | ✅ | [PDF button](src/MarcoERP.WpfUI/Views/Sales/SalesInvoiceDetailView.xaml#L113-L116), [Nav buttons](src/MarcoERP.WpfUI/Views/Sales/SalesInvoiceDetailView.xaml#L122-L146), [WebView2 print](src/MarcoERP.WpfUI/Views/Common/InvoicePdfPreviewDialog.xaml.cs#L58-L86) | موجود.
| Part4: Dirty State Tracking + منع الخروج | ✅ | [Dirty guard](src/MarcoERP.WpfUI/Navigation/NavigationService.cs#L31-L47) | موجود على مستوى التنقل.
| Part1 (Advanced): Sales Representative | ✅ | [Route](src/MarcoERP.WpfUI/App.xaml.cs#L489), [ViewModel uses service](src/MarcoERP.WpfUI/ViewModels/Sales/SalesRepresentativeViewModel.cs#L18-L44) | موجود شاشة وخدمات/Repo مسجلة في App.
| Part2 (Advanced): Bulk Price Update Screen | ✅ | [Route](src/MarcoERP.WpfUI/App.xaml.cs#L480), [View](src/MarcoERP.WpfUI/Views/Inventory/BulkPriceUpdateView.xaml), [ViewModel](src/MarcoERP.WpfUI/ViewModels/Inventory/BulkPriceUpdateViewModel.cs), [Service](src/MarcoERP.Application/Services/Inventory/BulkPriceUpdateService.cs) | موجود (Preview + Apply + Audit Log). تأكد فقط من إدراجه في القائمة/السايدبار حسب UX المطلوب.
| Advanced: نظام تسعير متعدد المستويات + Tier Pricing + Audit | ✅ | UI إدارة القوائم: [PriceLists route](src/MarcoERP.WpfUI/App.xaml.cs#L490), [PriceListViewModel](src/MarcoERP.WpfUI/ViewModels/Sales/PriceListViewModel.cs#L17-L60), [PriceListView](src/MarcoERP.WpfUI/Views/Sales/PriceListView.xaml#L1-L200). تطبيق داخل الفاتورة: [ISmartEntryQueryService](src/MarcoERP.Application/Interfaces/SmartEntry/ISmartEntryQueryService.cs), [SmartEntryQueryService](src/MarcoERP.Persistence/Services/SmartEntry/SmartEntryQueryService.cs), [SalesInvoiceDetailViewModel](src/MarcoERP.WpfUI/ViewModels/Sales/SalesInvoiceDetailViewModel.cs) | تم توصيل Tier Pricing داخل شاشة فاتورة البيع ضمن Smart Entry وبأولوية: Tier &gt; آخر بيع &gt; سعر الصنف (master default) (بدون كسر تعديل المستخدم).
| Advanced: Credit Control (CreditLimit/DaysAllowed/BlockedOnOverdue) | ⚠️ | UI: [CustomerView](src/MarcoERP.WpfUI/Views/Sales/CustomerView.xaml#L78-L190) + Overdue warning: [SalesInvoiceDetailView](src/MarcoERP.WpfUI/Views/Sales/SalesInvoiceDetailView.xaml#L222-L259). Enforcement: [SalesInvoiceService](src/MarcoERP.Application/Services/Sales/SalesInvoiceService.cs) + outstanding calc: [SmartEntryQueryService](src/MarcoERP.Persistence/Services/SmartEntry/SmartEntryQueryService.cs) | **المنع عند التجاوز موجود** (Create/Post). المتبقي: Workflow “موافقة مدير” (صلاحيات/توثيق/تجاوز مؤقت).
| Advanced: Profitability analysis + warning بيع بخسارة | ✅ | منطق التحذير: [SalesInvoiceLineFormItem](src/MarcoERP.WpfUI/ViewModels/Sales/SalesInvoiceViewModel.cs). UI highlight: [SalesInvoiceDetailView](src/MarcoERP.WpfUI/Views/Sales/SalesInvoiceDetailView.xaml) | تم إضافة تحذير داخل شاشة الفاتورة عند البيع بخسارة اعتمادًا على `WeightedAverageCost` مع مراعاة خصم السطر.
| Advanced: POS Sessions منع بيع بدون جلسة | ✅ (داخل POS) | [HasSession + error](src/MarcoERP.WpfUI/ViewModels/Sales/PosViewModel.cs#L42-L46) و[يجب فتح جلسة أولاً](src/MarcoERP.WpfUI/ViewModels/Sales/PosViewModel.cs#L402-L406) | مطبق على POS تدفق.

### ملاحظة تقنية صغيرة
- PurchaseInvoiceDetailView يستخدم `WarningColor` في الـHighlight:
  - [WarningColor usage](src/MarcoERP.WpfUI/Views/Purchases/PurchaseInvoiceDetailView.xaml#L291-L295)
  - الـResource موجود فعليًا في الثيم: [WarningColor in AppStyles](src/MarcoERP.WpfUI/Themes/AppStyles.xaml#L17-L29)

---

## 4) خطة التنفيذ المقترحة (Roadmap)

### 4.1 أولويات سريعة (1–2 يوم)
1) **مراجعة Highlight في PurchaseInvoiceDetailView**
  - الحالة: `WarningColor` معرف بالفعل في الثيم، فلا توجد مخاطرة Missing Resource.
  - المكان: [PurchaseInvoiceDetailView highlight](src/MarcoERP.WpfUI/Views/Purchases/PurchaseInvoiceDetailView.xaml#L291-L295) + [AppStyles](src/MarcoERP.WpfUI/Themes/AppStyles.xaml#L17-L29)

2) **توثيق/تأكيد أداء SmartEntry (Compiled Queries)**
   - الهدف: التأكد أن خدمات [SmartEntry](src/MarcoERP.Persistence/Services/SmartEntry) تستخدم compiled queries فعليًا.
   - الناتج: سطر/قسم في تقرير الأداء يوضح أين تم تطبيق compiled queries.

### 4.2 فجوات وظيفية متوسطة (3–7 أيام)
3) **Credit Control Enforcement (منع أو موافقة مدير)**
  - الحالة: المنع عند التجاوز تم تطبيقه في Application (Create/Post) بناءً على Outstanding + Overdue.
  - المتبقي: Workflow موافقة مدير (صلاحيات/تسجيل/تجاوز مؤقت) إن كان مطلوبًا بالسياسة.

4) **تطبيق أولوية التسعير داخل الفاتورة (special/tier/base) + Audit**
  - الحالة: تم تنفيذ Tier Pricing داخل Smart Entry في فاتورة البيع.
  - التنفيذ: عبر `ISmartEntryQueryService` + Persistence query + wiring في SalesInvoiceDetailViewModel.

### 4.3 تطوير ميزات كبيرة (1–2 أسبوع)
5) **Price Bulk Update Screen**
  - الحالة: موجودة بالفعل (UI + Preview + Apply + Audit).
  - المكان: [BulkPriceUpdate route](src/MarcoERP.WpfUI/App.xaml.cs#L480), [ViewModel](src/MarcoERP.WpfUI/ViewModels/Inventory/BulkPriceUpdateViewModel.cs), [Service](src/MarcoERP.Application/Services/Inventory/BulkPriceUpdateService.cs)

6) **Profitability داخل شاشة الفاتورة**
  - الحالة: تم إضافة تحذير بيع بخسارة (Line-level) داخل شاشة فاتورة البيع.
  - ملاحظة: يمكن لاحقًا توسيعها لعرض ربح/هامش إجمالي للفاتورة إن لزم.

---

## 5) نطاق التنفيذ (Scope) وقيود Clean Architecture
- متطلبات Phase1/Phase2 في [upgread.md](upgread.md) تذكر “WpfUI فقط” — وهذا متحقق إلى حد كبير، لكن عناصر مثل Credit Control / Pricing priority / Audit عادةً لا يمكن تنفيذها بشكل صحيح في WpfUI وحده.
- القاعدة المقترحة:
  - UI يعرض ويحسّن UX.
  - Application يفرض قواعد العمل (Credit Control, Pricing Engine, Profitability rules).
  - Persistence ينفذ الاستعلامات بكفاءة (Compiled Queries).

---

## 6) ماذا أحتاج منك لتثبيت الخطة؟
اختار 1 فقط كبداية تنفيذ فعلية:
1) تركيز على **الترقية حسب Phase1–3 فقط** (تحسينات WpfUI + توحيد highlight + توثيق الأداء).
2) فتح النطاق ليشمل **Application/Persistence** لتطبيق Credit Control + Pricing priority + Bulk Update.
