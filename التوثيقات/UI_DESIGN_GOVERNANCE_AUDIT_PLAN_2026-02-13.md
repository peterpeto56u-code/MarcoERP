# خطة التدقيق الشامل للتصميم والحوكمة – MarcoERP

**التاريخ:** 2026-02-13  
**النطاق:** طبقة `WpfUI` كاملة + الالتزام بملفات الحوكمة في `governance/`

---

## 1) ملخص تنفيذي (الحالة الحالية)

تم تنفيذ فحص معماري/تصميمي أولي + إصلاح مباشر لمشكلة التنقل الصامت:

1. **تم إصلاح عطل فتح الشاشات بصمت**
   - في `NavigationService` عند فشل فتح شاشة (خطأ DI/تهيئة)، النظام لم يعد يتجاهل الخطأ.
   - الآن يعرض شاشة خطأ واضحة داخل المحتوى بدلاً من “زر لا يعمل”.

2. **تحسين مزامنة القائمة الجانبية**
   - العنصر النشط في الـSidebar أصبح يتزامن مع أي تنقل (ليس فقط الضغط المباشر على زر القائمة).

3. **التبويبات العلوية بنمط المتصفح موجودة بالفعل**
   - يوجد `OpenTabs` + تفعيل/تنقل + إغلاق (`Ctrl+W`) في `MainWindow`.
   - تم تحسين سلوك زر الإغلاق داخل التبويب (منع سحب التركيز + Tooltip لاسم الصفحة).

---

## 2) نتائج فحص الحوكمة والتصميم (آلي + مراجعة كود)

### 2.1 توافق مع ملفات الحوكمة

- تمت مراجعة:  
  - `governance/UI_GUIDELINES.md`  
  - `governance/UI Guidelines v2.md`  
  - `governance/ARCHITECTURE.md`

### 2.2 مخالفات/مخاطر رئيسية

1. **Hardcoded UI Colors كثيرة**
   - رُصدت حالات متعددة لاستخدام ألوان مباشرة `#...` بدل Theme Resources.
   - هذا يضعف الاتساق ويصعّب التيمات وتوحيد الهوية.

2. **عرض الأخطاء/التحذيرات عبر MessageBox بكثافة**
   - رُصدت ~109 مواضع `MessageBox.Show` في ViewModels/Views.
   - يخالف مبدأ عرض أخطاء التحقق داخل Panels/Tooltips حسب `UVL-02` في الحوكمة.

3. **تباين جزئي في Naming Convention لعناصر الواجهة**
   - رُصدت تسميات لا تتبع Prefix القياسي (خصوصاً في بعض الحوارات/النوافذ القديمة).

4. **تعقيد مرتفع في `MainWindowViewModel`**
   - توجد ملاحظات جودة (Cognitive Complexity + تكرارات نصوص) تحتاج refactor مرحلي.

---

## 3) خطة فحص شامل شاشة-بشاشة (دقيقة)

## المرحلة A: Shell & Navigation (تم بدءها)
- [x] فحص `MainWindow.xaml` + `MainWindowViewModel.cs`
- [x] فحص `NavigationService` / `ViewRegistry`
- [x] إصلاح الفشل الصامت للتنقل
- [ ] اختبار سيناريوهات Dirty-State على التنقل بين التبويبات
- [ ] تدقيق UX للتبويبات (RTL + سلوك الإغلاق + ترتيب التركيز)

## المرحلة B: Design System Compliance
- [ ] استبدال الألوان الصريحة بـ Theme Resources
- [ ] توحيد ارتفاعات/هوامش عناصر الإدخال
- [ ] توحيد حالات Hover/Active في القوائم
- [ ] مراجعة تناسق Typography عبر كل الوحدات

## المرحلة C: Validation UX
- [ ] نقل رسائل التحقق من `MessageBox` إلى Validation Summary/Inline Error
- [ ] توحيد Required Marker (`*`) + borders
- [ ] توحيد رسائل business-rule القادمة من Application

## المرحلة D: DataGrid + Fields Semantics
- [ ] مراجعة عناوين الأعمدة مقابل نوع البيانات
- [ ] مراجعة ملاءمة أسماء الحقول مع محتواها (Text vs Amount vs Date)
- [ ] مراجعة تنسيق العملات/التواريخ بشكل موحد
- [ ] مراجعة ترتيب Tab وKeyboard UX

## المرحلة E: Module-by-Module Audit
- [ ] Accounting
- [ ] Inventory
- [ ] Sales
- [ ] Purchases
- [ ] Treasury
- [ ] Reports
- [ ] Settings

لكل شاشة سيتم توثيق:
- مشاكل التصميم (UI)
- مشاكل السلوك (UX)
- التعارض مع الحوكمة
- مستوى الخطورة
- جهد الإصلاح

---

## 4) التعديلات التي تم تطبيقها فعلياً الآن

1. `src/MarcoERP.WpfUI/Navigation/NavigationService.cs`
   - إضافة عرض خطأ واضح عند فشل التنقل بدل الإخفاق الصامت.

2. `src/MarcoERP.WpfUI/ViewModels/Shell/MainWindowViewModel.cs`
   - مزامنة العنصر النشط في Sidebar حسب `ViewKey` عند أي تنقل.

3. `src/MarcoERP.WpfUI/Views/Shell/MainWindow.xaml`
   - تحسين تفاعل زر إغلاق التبويب + Tooltip عنوان الصفحة.

---

## 5) نتائج التحقق بعد التعديلات

- `dotnet build MarcoERP.sln` ✅ ناجح.
- الاختبارات: `435 Passed, 2 Failed`  
  - الفشلين في طبقة Application Tests (غير مرتبطين مباشرة بتعديلات الـUI الحالية).

---

## 6) خيارات التغيير الكبير (تحتاج موافقتك)

### الخيار 1 – محافظ (موصى به كبداية)
- إصلاحات تصميمية تدريجية دون تغيير جذري للبنية.
- نبدأ بـ Shell + Top Tabs + Sidebar + Validation UX في أكثر الشاشات استخدامًا.

### الخيار 2 – توحيد Design System كامل
- استخراج ResourceDictionary مركزي لكل الألوان/spacing/typography.
- استبدال شامل لكل hardcoded styles في جميع الشاشات.
- أعلى جودة اتساق، لكن حجم تغيير كبير.

### الخيار 3 – تحويل تدريجي إلى Components قابلة لإعادة الاستخدام
- إنشاء Controls موحدة (Header, FormSection, StandardGridToolbar, ValidationPanel).
- يقلل التكرار طويل المدى، لكنه أطول زمنيًا.

---

## 7) المطلوب منك قبل المتابعة الموسعة

يرجى اختيار المسار:
- **A)** خيار 1 (محافظ)  
- **B)** خيار 2 (توحيد شامل)  
- **C)** خيار 3 (Components تدريجية)  

وحدد الأولوية الوظيفية الأولى:
- **1)** المبيعات  
- **2)** المشتريات  
- **3)** الخزينة  
- **4)** الإعدادات  
- **5)** التقارير

بعد اختيارك سأبدأ التنفيذ المرحلي شاشة-بشاشة مع تقرير تقدم واضح لكل مرحلة.

---

## 8) نتائج التدقيق المعمّق (دفعة 1)

> **منهجية هذه الدفعة:**
> - مسح آلي للمخالفات المتكررة (hardcoded colors + MessageBox.Show) على مستوى الوحدات.
> - تدقيق يدوي لملفات Detail/ViewModel الحرجة في: Sales / Purchases / Treasury / Settings / Inventory.
> - مطابقة مباشرة مع بنود الحوكمة: `UTR-01..UTR-08`, `UNV-*`, `Message & Dialog Standards`.

### 8.1 مؤشرات كمية (قابلة للقياس)

#### A) عدد ألوان Hex الصريحة داخل XAML (مؤشر عدم الالتزام بـTheme Resources)

| الوحدة | عدد المطابقات |
|---|---:|
| Sales | 40 |
| Purchases | 22 |
| Treasury | 13 |
| Settings | 40 |
| Inventory | 19 |
| Accounting | 28 |

#### B) عدد `MessageBox.Show` داخل ViewModels

| الوحدة | عدد المطابقات |
|---|---:|
| Sales | 26 |
| Purchases | 23 |
| Treasury | 19 |
| Settings | 9 |
| Inventory | 9 |
| Accounting | 8 |

**قراءة سريعة للأرقام:**
- أعلى ضغط توحيد بصري: **Sales + Settings + Accounting**.
- أعلى ضغط UX رسائل/حوارات: **Sales + Purchases + Treasury**.

### 8.2 مخالفات حرجة مؤكدة (High Severity)

1. **مخالفة مباشرة لمعيار UTR-08 في شاشات عروض الأسعار**
   - المعيار ينص على تحرير البنود عبر Popup (`InvoiceAddLineWindow`) وليس Inline DataGrid editing.
   - الحالة الفعلية:
     - `src/MarcoERP.WpfUI/Views/Sales/SalesQuotationDetailView.xaml`
       - تحرير مباشر داخل خلايا DataGrid (`ComboBox`/`TextBox`) للبنود.
     - `src/MarcoERP.WpfUI/Views/Purchases/PurchaseQuotationDetailView.xaml`
       - نفس النمط (Inline editing) بدلاً من Popup موحد.
   - **الأثر:** تباين سلوكي بين فواتير/مرتجعات (Popup) وعروض الأسعار (Inline)، وزيادة أخطاء الإدخال.

2. **انتشار واسع للألوان الصريحة (Hardcoded Hex) خارج ResourceDictionary**
   - أمثلة متكررة:
     - `src/MarcoERP.WpfUI/Views/Sales/SalesInvoiceDetailView.xaml` (`#E8EAF0`, `#E0E0E0`, `#ECEFF1`)
     - `src/MarcoERP.WpfUI/Views/Purchases/PurchaseInvoiceDetailView.xaml` (`#E0E0E0`, `#ECEFF1`)
     - `src/MarcoERP.WpfUI/Views/Treasury/CashReceiptView.xaml` و `CashPaymentView.xaml` (`#ECEFF1`)
     - `src/MarcoERP.WpfUI/Views/Settings/RoleManagementView.xaml` (`#E0E0E0`, `#ECEFF1`)
   - **الأثر:** صعوبة الصيانة، وتعارض محتمل مع أي Theme تبديل أو Design tokenization.

3. **ازدحام طبقة ViewModel بنوافذ MessageBox على نطاق واسع**
   - رغم أن التأكيدات مسموحة، يوجد استخدام كثيف أيضًا في سيناريوهات إرشادية/تنقل.
   - ملفات ذات كثافة عالية:
     - `src/MarcoERP.WpfUI/ViewModels/Sales/SalesInvoiceDetailViewModel.cs`
     - `src/MarcoERP.WpfUI/ViewModels/Treasury/CashReceiptViewModel.cs`
     - `src/MarcoERP.WpfUI/ViewModels/Treasury/CashPaymentViewModel.cs`
   - **الأثر:** تجربة متقطعة، وإرهاق المستخدم بنوافذ modal متكررة.

### 8.3 ملاحظات متوسطة (Medium Severity)

1. **اتساق بصري غير مكتمل في أشرطة الحالة السفلية**
   - نفس النمط يتكرر بألوان صريحة (`#ECEFF1`) في عدة شاشات بدل مورد موحد.

2. **تفاوت في أسلوب إجماليات المستندات (Totals Bar)**
   - بعض الشاشات تستخدم `Foreground="Gray"` وبعضها `SubtitleBrush`، مع اختلافات طفيفة بالخط/المسافات.

3. **التعقيد السلوكي للأوامر (ظاهر/غير فعال)**
   - في بعض شاشات Treasury تظهر أزرار `Post/Cancel/Delete` دائمًا بينما الفعالية منضبطة بالأوامر.
   - يفضّل توحيد **Visibility** حسب حالة المستند لتقليل الالتباس.

### 8.4 نقاط قوة مؤكدة (ملتزم/جيد)

1. **UTR-01 (Status badge) مطبق بشكل جيد** في أغلب شاشات المعاملات الحرجة (Sales/Purchases/Treasury).
2. **UTR-02 (Read-only بعد الترحيل)** متحقق وظيفيًا غالبًا عبر `IsEditing` + شروط الأوامر.
3. **F1SearchBehavior** منتشر جيدًا على ComboBoxs المهمة في الإدخال.
4. **Popup line editing** مطبق بنجاح في:
   - `SalesInvoiceDetailView`
   - `PurchaseInvoiceDetailView`
   - `SalesReturnDetailView`
   - `PurchaseReturnDetailView`

---

## 9) مصفوفة الفجوات (Severity × Effort × Priority)

| الفجوة | الشدة | الجهد | الأولوية | ملاحظة تنفيذ |
|---|---|---|---|---|
| تحويل Quotation lines إلى Popup موحد بدل Inline | High | M | P0 | أعلى أثر على الاتساق الوظيفي |
| توحيد الألوان الصريحة إلى Theme Resources (الدفعة الأولى) | High | M | P0 | نبدأ بـ Sales + Settings |
| تقليل MessageBox غير الضروري وإحلال Status/Inline feedback | High | M | P1 | لا يمس Confirmات التدمير |
| توحيد Status Bar/Totals visual tokens | Medium | S | P1 | سريع ومفيد بصريًا |
| توحيد Visibility للأزرار حسب حالة المستند | Medium | S | P2 | تحسين إرشادي وتقليل الالتباس |

---

## 10) خطة تنفيذ عملية مقترحة (3 موجات)

### الموجة 1 (P0) — اتساق سلوك المعاملات
1. توحيد شاشات **SalesQuotationDetail + PurchaseQuotationDetail** على Popup line editor.
2. الحفاظ على نفس VM state المستخدمة في الفواتير/المرتجعات (Add & Next + Edit line popup).
3. اختبار سيناريوهات Draft/Send/Convert بعد التحويل.

### الموجة 2 (P0/P1) — توحيد بصري سريع
1. استخراج Brush tokens بديلة للقيم المتكررة (`#ECEFF1`, `#E0E0E0`, `Gray`, ...).
2. استبدال تدريجي في أكثر الشاشات استخدامًا: Sales/Purchases/Treasury.
3. مراجعة Contrast + consistency بعد كل دفعة.

### الموجة 3 (P1/P2) — UX الرسائل والتغذية الراجعة
1. إبقاء MessageBox للتأكيدات الحرجة فقط (Delete/Post/Cancel).
2. نقل الرسائل الإرشادية والتنقلية إلى `StatusMessage` + Error Bar.
3. توحيد إظهار/إخفاء أزرار الإجراءات حسب حالة المستند بدلاً من التعطيل فقط.

---

## 11) القرار المطلوب قبل بدء التنفيذ البرمجي

إذا وافقت، أبدأ مباشرة بالموجة 1 (الأكثر تأثيرًا):

1. **توحيد Quotation line editing إلى Popup**
2. **توحيد أول دفعة ألوان صريحة** في ملفات Sales/Purchases/Treasury الأساسية

ثم أقدّم Patch صغير لكل خطوة مع Build/Test بعد كل موجة.

---

## 12) التدقيق المحاسبي العميق (Code-Level)

### 12.1 نطاق القراءة الفعلية

تمت قراءة/مراجعة ملفات المحاسبة عبر الطبقات التالية:

- **Domain**: `JournalEntry`, `JournalEntryLine`, `FiscalYear`, `FiscalPeriod`, `Account`.
- **Application**: `JournalEntryService`, `FiscalYearService`, `YearEndClosingService`, `AccountService`.
- **Persistence**: Configurations + Repositories الخاصة بالقيود/الفترات.
- **WpfUI**: كل Views/ViewModels المحاسبية (قيود يومية، سنة/فترة مالية، شجرة الحسابات، الأرصدة الافتتاحية).
- **Tests**: Domain + Application tests الخاصة بالمحاسبة.

### 12.2 نقاط قوة محاسبية مؤكدة

1. **قواعد القيد المزدوج قوية** في `JournalEntry`:
   - حد أدنى سطرين، منع المدين/الدائن معًا، منع السطر الصفري، توازن الإجماليات.
2. **تجميد القيد بعد الترحيل** مطبق بوضوح (`EnsureDraft` + منع SoftDelete لغير المسودة).
3. **حوكمة السنوات والفترات** جيدة في `FiscalYearService`:
   - سنة واحدة نشطة، قفل تسلسلي للفترات، منع الإقفال مع مسودات معلقة.
4. **قيود قاعدة البيانات** داعمة:
   - `CK_JournalEntries_Balance` + فهارس مهمة + `DeleteBehavior.Restrict`.

### 12.3 مخاطر عالية (High-Risk)

#### HR-ACC-01 — مسار إقفال السنة يتجاوز بوابة تحقق الترحيل القياسية

- `YearEndClosingService.GenerateClosingEntryAsync` ينشئ ويرحل قيد الإقفال مباشرةً عبر الـDomain + Repository.
- هذا **يتجاوز** التحققات التطبيقية المركزية الموجودة في `JournalEntryService.PostAsync` (خاصة قيود انفتاح الفترة/فعالية السنة وتحقق الحسابات وقت الترحيل).
- الخطر: أي تحديث لاحق لقواعد الترحيل في `JournalEntryService` لن يُطبق تلقائيًا على قيد الإقفال.

**التوصية:**
- توحيد مسار الترحيل بحيث يستخدم قيد الإقفال نفس بوابة التحقق المركزية (إما عبر `JournalEntryService` أو خدمة DomainPolicy مشتركة إلزامية قبل `Post`).

#### HR-ACC-02 — تناقض حوكمي محتمل: شرط قفل كل الفترات قبل الإقفال مقابل توليد قيد إقفال داخل آخر فترة

- `FiscalYearService.CloseAsync` يشترط أن كل الفترات مقفلة أولًا.
- ثم يستدعي `YearEndClosingService` الذي ينشئ قيدًا في الفترة الأخيرة.
- هذا يتطلب **استثناء حوكمي صريح**: "قيد الإقفال يسمح به حتى لو كانت الفترة مقفلة".

**التوصية:**
- تثبيت هذه القاعدة كتابيًا في `FINANCIAL_ENGINE_RULES.md` و `UI_GUIDELINES.md` لتجنب تضارب التدقيقات المستقبلية.

### 12.4 مخاطر متوسطة (Medium)

#### M-ACC-01 — تكرار وظيفي لشاشة إدارة الفترات

- يوجد مساران وظيفيان متداخلان:
  - `FiscalYearView` (إدارة الفترات ضمن شاشة السنة)
  - `FiscalPeriodView` (إدارة الفترات كشاشة مستقلة)
- هذا يرفع احتمالية انحراف UX والقواعد مستقبلاً.

**التوصية:**
- اعتماد شاشة واحدة كمسار رئيسي، أو توحيد كامل للسلوك/النصوص/التحقق بين الشاشتين.

#### M-ACC-02 — كثافة ألوان صريحة في واجهات المحاسبة

- رصد **28** استخدامًا مباشرًا للألوان في Views المحاسبية، أعلى تركيز في `OpeningBalanceWizardView` و`ChartOfAccountsView`.
- يضعف قابلية التيمات والاتساق البصري.

### 12.5 ملاحظة اختبارات مهمة

تم تشغيل اختبارات المحاسبة المتخصصة:

- **النتيجة:** `164 Passed / 1 Failed`
- الاختبار الفاشل: `FiscalYearServiceTests.CloseAsync_WithNoDrafts_ReturnsSuccess`
- سبب الفشل الظاهر: بيانات الاختبار لا تحقق شرط توازن ميزان المراجعة الذي أصبح مفروضًا قبل الإقفال.

**الخلاصة:** الفشل أقرب إلى **Test Drift** بعد تشديد القواعد، وليس دليلًا مباشرًا على عطب إنتاجي.

### 12.6 توصيات تنفيذ مرتبة (Accounting)

1. **P0:** توحيد مسار ترحيل قيد الإقفال مع بوابة الترحيل القياسية.
2. **P0:** توثيق الاستثناء الحوكمي لقيد الإقفال داخل فترة مقفلة (إن كان مقصودًا).
3. **P1:** تحديث الاختبار الفاشل ليعكس قواعد التوازن الحالية.
4. **P1:** دمج/توحيد تجربة `FiscalYearView` و`FiscalPeriodView`.
5. **P2:** إزالة hardcoded colors من شاشات المحاسبة.

---

## 13) إقفال المرحلة: تدقيق ملف-بملف (محاسبة)

تم تنفيذ طلب التدقيق الصارم "ملف-بملف بدون تخطي" على نطاق المحاسبة مع مخرجات إثبات مستقلة:

1. **تغطية قراءة شاملة (Workspace كامل):**
   - `التوثيقات/FULL_READ_COVERAGE_2026-02-13.md`
   - النتيجة: `8085/8085` ملفات مقروءة بنجاح، فشل `0`.

2. **ملخص تغطية لكل فولدر:**
   - `التوثيقات/FULL_READ_FOLDER_SUMMARY_2026-02-13.md`

3. **مصفوفة تدقيق محاسبي ملف-بملف:**
   - `التوثيقات/ACCOUNTING_FILE_BY_FILE_AUDIT_2026-02-13.md`
   - تشمل: عدد الأسطر، مؤشرات الحوار، hardcoded colors، إشارات المعاملات والتحقق.

4. **قاعدة تشغيل إلزامية للتدقيق:**
   - `التوثيقات/STRICT_FULL_SCAN_RULE.md`

### نتيجة هذه المرحلة

- لا يوجد تخطي ملفات ضمن نطاق المحاسبة.
- تم ربط الفجوات الحرجة (خصوصًا مسار إقفال السنة) بأدلة كود واختبارات.
- المرحلة التالية جاهزة: تنفيذ إصلاحات P0 في مسار الإقفال + تحديث اختبارات الانجراف.

---

## 14) تنفيذ الإصلاحات (تم)

تم تنفيذ إصلاحات مباشرة على الكود وفق الفجوات الحرجة/العالية في نطاق المحاسبة:

### 14.1 إصلاحات منطق محاسبي (P0)

1. **تقوية مسار قيد الإقفال السنوي** في:
   - `src/MarcoERP.Application/Services/Accounting/YearEndClosingService.cs`
   - الإضافات:
     - تحقق سياق الترحيل (`closingDate` داخل السنة/الفترة الأخيرة).
     - تحقق قابلية الترحيل لحساب الأرباح المحتجزة وكل حساب يدخل في قيد الإقفال.
     - تحقق Domain validation قبل `Post()` لمنع تمرير قيد غير صالح.

2. **تحديث اختبار الانجراف** في:
   - `tests/MarcoERP.Application.Tests/Accounting/FiscalYearServiceTests.cs`
   - تم جعل بيانات الاختبار متوازنة قبل الإقفال (مدين/دائن متساويان) ليتطابق مع قاعدة الميزان.

### 14.2 إصلاحات تصميمية (Accounting UI)

تم إزالة hardcoded colors في شاشات المحاسبة التالية والاعتماد على Theme Resources:

- `src/MarcoERP.WpfUI/Views/Accounting/OpeningBalanceWizardView.xaml`
- `src/MarcoERP.WpfUI/Views/Accounting/ChartOfAccountsView.xaml`
- `src/MarcoERP.WpfUI/Views/Accounting/FiscalPeriodView.xaml`
- `src/MarcoERP.WpfUI/Views/Accounting/JournalEntryView.xaml`

### 14.3 تحقق ما بعد الإصلاح

- `dotnet build MarcoERP.sln` ✅ **ناجح**.
- Accounting targeted tests: ✅ `165 Passed / 0 Failed`.
- فحص الألوان الصريحة في `Views/Accounting/*.xaml`: ✅ **لا توجد مطابقة** `#[0-9A-Fa-f]{3,8}`.
