using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using MarcoERP.Application.DTOs.Inventory;
using MarcoERP.Application.DTOs.Purchases;
using MarcoERP.Application.DTOs.Sales;
using MarcoERP.Application.Interfaces;
using MarcoERP.Application.Interfaces.Inventory;
using MarcoERP.Application.Interfaces.Purchases;
using MarcoERP.Application.Interfaces.Sales;
using MarcoERP.WpfUI.Services;

namespace MarcoERP.WpfUI.ViewModels.Sales
{
    // ═══════════════════════════════════════════════════════════
    //  PriceListProductItem — row model for the Excel-like grid
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Represents a single product row inside the price-list grid.
    /// Each row carries the product master data (read-only) plus
    /// editable price-list fields (IsSelected, PriceListPrice, MinQuantity).
    /// </summary>
    public sealed class PriceListProductItem : BaseViewModel
    {
        public int ProductId { get; set; }
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string SupplierName { get; set; } = string.Empty;
        public int? SupplierId { get; set; }
        public decimal DefaultSalePrice { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        private decimal _priceListPrice;
        public decimal PriceListPrice
        {
            get => _priceListPrice;
            set => SetProperty(ref _priceListPrice, value);
        }

        private decimal _minQuantity = 1;
        public decimal MinQuantity
        {
            get => _minQuantity;
            set => SetProperty(ref _minQuantity, value);
        }
    }

    // ═══════════════════════════════════════════════════════════
    //  PriceListViewModel — main ViewModel
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// ViewModel for the Price List management screen (قوائم الأسعار).
    /// Shows ALL products in an Excel-like grid with checkboxes,
    /// inline price editing, supplier/category filtering, and PDF export.
    /// </summary>
    public sealed class PriceListViewModel : BaseViewModel
    {
        private readonly IPriceListService _priceListService;
        private readonly IProductService _productService;
        private readonly ISupplierService _supplierService;
        private readonly IInvoicePdfPreviewService _pdfService;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly SemaphoreSlim _loadGate = new(1, 1);

        public PriceListViewModel(
            IPriceListService priceListService,
            IProductService productService,
            ISupplierService supplierService,
            IInvoicePdfPreviewService pdfService,
            IDateTimeProvider dateTimeProvider)
        {
            _priceListService = priceListService ?? throw new ArgumentNullException(nameof(priceListService));
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _supplierService = supplierService ?? throw new ArgumentNullException(nameof(supplierService));
            _pdfService = pdfService ?? throw new ArgumentNullException(nameof(pdfService));
            _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));

            AllPriceLists = new ObservableCollection<PriceListListDto>();
            AllItems = new ObservableCollection<PriceListProductItem>();
            Suppliers = new ObservableCollection<SupplierDto>();
            Categories = new ObservableCollection<string>();

            _filteredItems = CollectionViewSource.GetDefaultView(AllItems);
            _filteredItems.Filter = FilterPredicate;

            LoadCommand = new AsyncRelayCommand(LoadAsync);
            NewCommand = new AsyncRelayCommand(_ => PrepareNewAsync());
            SaveCommand = new AsyncRelayCommand(SaveAsync, () => CanSave);
            DeleteCommand = new AsyncRelayCommand(DeleteAsync, () => SelectedPriceList != null && !IsNew);
            CancelCommand = new RelayCommand(CancelEditing);
            PrintCommand = new AsyncRelayCommand(PrintAsync, () => SelectedPriceList != null || IsNew);
            SelectAllCommand = new RelayCommand(_ => SetAllSelection(true));
            DeselectAllCommand = new RelayCommand(_ => SetAllSelection(false));
            CopyDefaultPricesCommand = new RelayCommand(_ => CopyDefaultPrices());
        }

        // ══════ Collections ══════════════════════════════════════

        public ObservableCollection<PriceListListDto> AllPriceLists { get; }
        public ObservableCollection<PriceListProductItem> AllItems { get; }
        public ObservableCollection<SupplierDto> Suppliers { get; }
        public ObservableCollection<string> Categories { get; }

        private readonly ICollectionView _filteredItems;
        public ICollectionView FilteredItems => _filteredItems;

        // ══════ Price List Selection ═════════════════════════════

        private PriceListListDto _selectedPriceList;
        public PriceListListDto SelectedPriceList
        {
            get => _selectedPriceList;
            set
            {
                if (SetProperty(ref _selectedPriceList, value))
                {
                    if (value != null)
                    {
                        IsEditing = true;
                        IsNew = false;
                        _ = LoadPriceListDetailAsync(value.Id);
                    }
                }
            }
        }

        // ══════ Filter Properties ════════════════════════════════

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set { if (SetProperty(ref _searchText, value)) ApplyFilter(); }
        }

        private int? _filterSupplierId;
        public int? FilterSupplierId
        {
            get => _filterSupplierId;
            set { if (SetProperty(ref _filterSupplierId, value)) ApplyFilter(); }
        }

        private string _filterCategoryName = string.Empty;
        public string FilterCategoryName
        {
            get => _filterCategoryName;
            set { if (SetProperty(ref _filterCategoryName, value)) ApplyFilter(); }
        }

        // ══════ Form Fields ═════════════════════════════════════

        private bool _isEditing;
        public bool IsEditing
        {
            get => _isEditing;
            set => SetProperty(ref _isEditing, value);
        }

        private bool _isNew;
        public bool IsNew
        {
            get => _isNew;
            set => SetProperty(ref _isNew, value);
        }

        private string _formCode = string.Empty;
        public string FormCode
        {
            get => _formCode;
            set => SetProperty(ref _formCode, value);
        }

        private string _formNameAr = string.Empty;
        public string FormNameAr
        {
            get => _formNameAr;
            set { SetProperty(ref _formNameAr, value); OnPropertyChanged(nameof(CanSave)); }
        }

        private string _formNameEn = string.Empty;
        public string FormNameEn
        {
            get => _formNameEn;
            set => SetProperty(ref _formNameEn, value);
        }

        private string _formDescription = string.Empty;
        public string FormDescription
        {
            get => _formDescription;
            set => SetProperty(ref _formDescription, value);
        }

        private DateTime? _formValidFrom;
        public DateTime? FormValidFrom
        {
            get => _formValidFrom;
            set => SetProperty(ref _formValidFrom, value);
        }

        private DateTime? _formValidTo;
        public DateTime? FormValidTo
        {
            get => _formValidTo;
            set => SetProperty(ref _formValidTo, value);
        }

        private bool _formIsActive = true;
        public bool FormIsActive
        {
            get => _formIsActive;
            set => SetProperty(ref _formIsActive, value);
        }

        // ══════ Computed ═════════════════════════════════════════

        public bool CanSave => IsEditing && !string.IsNullOrWhiteSpace(FormNameAr);

        public int SelectedCount => AllItems.Count(i => i.IsSelected);
        public int TotalVisible => _filteredItems.Cast<object>().Count();

        private void RefreshCounts()
        {
            OnPropertyChanged(nameof(SelectedCount));
            OnPropertyChanged(nameof(TotalVisible));
        }

        // ══════ Commands ═════════════════════════════════════════

        public ICommand LoadCommand { get; }
        public ICommand NewCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand PrintCommand { get; }
        public ICommand SelectAllCommand { get; }
        public ICommand DeselectAllCommand { get; }
        public ICommand CopyDefaultPricesCommand { get; }

        // ══════ Filtering ════════════════════════════════════════

        private void ApplyFilter()
        {
            _filteredItems.Refresh();
            RefreshCounts();
        }

        private bool FilterPredicate(object obj)
        {
            if (obj is not PriceListProductItem item) return false;

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var search = SearchText.Trim();
                if (!item.ProductName.Contains(search, StringComparison.OrdinalIgnoreCase) &&
                    !item.ProductCode.Contains(search, StringComparison.OrdinalIgnoreCase))
                    return false;
            }

            if (FilterSupplierId.HasValue && FilterSupplierId > 0 && item.SupplierId != FilterSupplierId)
                return false;

            if (!string.IsNullOrWhiteSpace(FilterCategoryName) && item.CategoryName != FilterCategoryName)
                return false;

            return true;
        }

        // ══════ Load ═════════════════════════════════════════════

        public async Task LoadAsync()
        {
            await _loadGate.WaitAsync();
            IsBusy = true;
            ClearError();
            try
            {
                // Load sequentially to avoid concurrent use of a shared DbContext instance
                var plResult = await _priceListService.GetAllAsync();
                var prodResult = await _productService.GetAllAsync();
                var supResult = await _supplierService.GetAllAsync();

                // Price lists
                if (plResult.IsSuccess)
                {
                    AllPriceLists.Clear();
                    foreach (var item in plResult.Data)
                        AllPriceLists.Add(item);
                }
                else
                {
                    ErrorMessage = plResult.ErrorMessage;
                    return;
                }

                // Suppliers (for filter dropdown)
                if (supResult.IsSuccess)
                {
                    Suppliers.Clear();
                    foreach (var s in supResult.Data.Where(s => s.IsActive).OrderBy(s => s.NameAr))
                        Suppliers.Add(s);
                }

                // Products → AllItems
                if (prodResult.IsSuccess)
                {
                    AllItems.Clear();
                    Categories.Clear();

                    var cats = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                    foreach (var p in prodResult.Data.OrderBy(p => p.Code))
                    {
                        AllItems.Add(new PriceListProductItem
                        {
                            ProductId = p.Id,
                            ProductCode = p.Code ?? string.Empty,
                            ProductName = p.NameAr ?? string.Empty,
                            CategoryName = p.CategoryName ?? string.Empty,
                            SupplierId = p.DefaultSupplierId,
                            SupplierName = p.DefaultSupplierName ?? string.Empty,
                            DefaultSalePrice = p.DefaultSalePrice,
                            PriceListPrice = 0,
                            MinQuantity = 1,
                            IsSelected = false
                        });

                        if (!string.IsNullOrWhiteSpace(p.CategoryName))
                            cats.Add(p.CategoryName);
                    }

                    foreach (var c in cats.OrderBy(c => c))
                        Categories.Add(c);
                }
                else
                {
                    ErrorMessage = prodResult.ErrorMessage;
                    return;
                }

                StatusMessage = $"تم تحميل {AllItems.Count} صنف و {AllPriceLists.Count} قائمة أسعار";
                RefreshCounts();
            }
            catch (Exception ex)
            {
                ErrorMessage = FriendlyErrorMessage("التحميل", ex);
            }
            finally
            {
                IsBusy = false;
                _loadGate.Release();
            }
        }

        private async Task LoadPriceListDetailAsync(int id)
        {
            await _loadGate.WaitAsync();
            IsBusy = true;
            ClearError();
            try
            {
                var result = await _priceListService.GetByIdAsync(id);
                if (result.IsSuccess)
                {
                    var dto = result.Data;
                    FormCode = dto.Code;
                    FormNameAr = dto.NameAr;
                    FormNameEn = dto.NameEn;
                    FormDescription = dto.Description;
                    FormValidFrom = dto.ValidFrom;
                    FormValidTo = dto.ValidTo;
                    FormIsActive = dto.IsActive;

                    // Merge tiers into product grid
                    var tierMap = dto.Tiers.ToDictionary(t => t.ProductId);

                    foreach (var item in AllItems)
                    {
                        if (tierMap.TryGetValue(item.ProductId, out var tier))
                        {
                            item.IsSelected = true;
                            item.PriceListPrice = tier.Price;
                            item.MinQuantity = tier.MinimumQuantity;
                        }
                        else
                        {
                            item.IsSelected = false;
                            item.PriceListPrice = 0;
                            item.MinQuantity = 1;
                        }
                    }

                    StatusMessage = $"تم تحميل قائمة الأسعار: {dto.NameAr} ({dto.Tiers.Count} شريحة)";
                    RefreshCounts();
                }
                else
                {
                    ErrorMessage = result.ErrorMessage;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = FriendlyErrorMessage("تحميل التفاصيل", ex);
            }
            finally
            {
                IsBusy = false;
                _loadGate.Release();
            }
        }

        // ══════ New ══════════════════════════════════════════════

        private async Task PrepareNewAsync()
        {
            IsEditing = true;
            IsNew = true;
            ClearError();

            _selectedPriceList = null;
            OnPropertyChanged(nameof(SelectedPriceList));

            FormNameAr = "";
            FormNameEn = "";
            FormDescription = "";
            FormValidFrom = null;
            FormValidTo = null;
            FormIsActive = true;

            // Reset all product items
            foreach (var item in AllItems)
            {
                item.IsSelected = false;
                item.PriceListPrice = 0;
                item.MinQuantity = 1;
            }

            try
            {
                var codeResult = await _priceListService.GetNextCodeAsync();
                FormCode = codeResult.IsSuccess ? codeResult.Data : "";
            }
            catch
            {
                FormCode = "";
            }

            RefreshCounts();
            StatusMessage = "إدخال قائمة أسعار جديدة...";
        }

        // ══════ Save ═════════════════════════════════════════════

        private async Task SaveAsync()
        {
            IsBusy = true;
            ClearError();
            try
            {
                var selectedTiers = AllItems
                    .Where(i => i.IsSelected)
                    .Select(i => new CreatePriceTierDto
                    {
                        ProductId = i.ProductId,
                        MinimumQuantity = i.MinQuantity,
                        Price = i.PriceListPrice
                    })
                    .ToList();

                if (IsNew)
                {
                    var dto = new CreatePriceListDto
                    {
                        NameAr = FormNameAr,
                        NameEn = FormNameEn,
                        Description = FormDescription,
                        ValidFrom = FormValidFrom,
                        ValidTo = FormValidTo,
                        Tiers = selectedTiers
                    };
                    var result = await _priceListService.CreateAsync(dto);
                    if (result.IsSuccess)
                    {
                        StatusMessage = $"تم إنشاء قائمة الأسعار: {result.Data.NameAr} ({selectedTiers.Count} صنف)";
                        IsEditing = false;
                        IsNew = false;
                        await LoadAsync();
                    }
                    else
                    {
                        ErrorMessage = result.ErrorMessage;
                    }
                }
                else
                {
                    if (SelectedPriceList == null) return;

                    var dto = new UpdatePriceListDto
                    {
                        Id = SelectedPriceList.Id,
                        NameAr = FormNameAr,
                        NameEn = FormNameEn,
                        Description = FormDescription,
                        ValidFrom = FormValidFrom,
                        ValidTo = FormValidTo,
                        Tiers = selectedTiers
                    };
                    var result = await _priceListService.UpdateAsync(dto);
                    if (result.IsSuccess)
                    {
                        StatusMessage = $"تم تحديث قائمة الأسعار: {result.Data.NameAr} ({selectedTiers.Count} صنف)";
                        IsEditing = false;
                        await LoadAsync();
                    }
                    else
                    {
                        ErrorMessage = result.ErrorMessage;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = FriendlyErrorMessage("الحفظ", ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        // ══════ Delete ═══════════════════════════════════════════

        private async Task DeleteAsync()
        {
            if (SelectedPriceList == null) return;

            var confirm = MessageBox.Show(
                $"هل أنت متأكد من حذف قائمة الأسعار «{SelectedPriceList.NameAr}»؟",
                "تأكيد الحذف",
                MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);
            if (confirm != MessageBoxResult.Yes) return;

            IsBusy = true;
            ClearError();
            try
            {
                var result = await _priceListService.DeleteAsync(SelectedPriceList.Id);
                if (result.IsSuccess)
                {
                    StatusMessage = "تم حذف قائمة الأسعار";
                    IsEditing = false;
                    await LoadAsync();
                }
                else
                {
                    ErrorMessage = result.ErrorMessage;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = FriendlyErrorMessage("الحذف", ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        // ══════ Cancel ═══════════════════════════════════════════

        private void CancelEditing(object parameter)
        {
            IsEditing = false;
            IsNew = false;
            ClearError();

            // Reset grid items
            foreach (var item in AllItems)
            {
                item.IsSelected = false;
                item.PriceListPrice = 0;
                item.MinQuantity = 1;
            }

            _selectedPriceList = null;
            OnPropertyChanged(nameof(SelectedPriceList));

            FormCode = "";
            FormNameAr = "";
            FormNameEn = "";
            FormDescription = "";
            FormValidFrom = null;
            FormValidTo = null;
            FormIsActive = true;

            RefreshCounts();
            StatusMessage = "تم الإلغاء";
        }

        // ══════ Bulk Actions ═════════════════════════════════════

        private void SetAllSelection(bool selected)
        {
            foreach (var item in _filteredItems.Cast<PriceListProductItem>())
                item.IsSelected = selected;
            RefreshCounts();
        }

        private void CopyDefaultPrices()
        {
            foreach (var item in AllItems.Where(i => i.IsSelected))
                item.PriceListPrice = item.DefaultSalePrice;

            StatusMessage = "تم نسخ أسعار البيع الافتراضية للأصناف المحددة";
        }

        // ══════ Print / PDF ══════════════════════════════════════

        private async Task PrintAsync()
        {
            IsBusy = true;
            ClearError();
            try
            {
                var selectedItems = AllItems.Where(i => i.IsSelected).ToList();
                if (selectedItems.Count == 0)
                {
                    ErrorMessage = "لا توجد أصناف محددة للطباعة. الرجاء تحديد أصناف أولاً.";
                    return;
                }

                var html = BuildPriceListHtml(selectedItems, FormNameAr, FormCode, FormValidFrom, FormValidTo, FormDescription, _dateTimeProvider);
                var request = new InvoicePdfPreviewRequest
                {
                    Title = $"قائمة أسعار — {FormNameAr}",
                    FilePrefix = $"pricelist_{FormCode}",
                    HtmlContent = html,
                    PaperSize = PdfPaperSize.A4
                };

                await _pdfService.ShowHtmlPreviewAsync(request);
            }
            catch (Exception ex)
            {
                ErrorMessage = FriendlyErrorMessage("الطباعة", ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private static string BuildPriceListHtml(
            List<PriceListProductItem> items,
            string nameAr, string code,
            DateTime? validFrom, DateTime? validTo,
            string description,
            IDateTimeProvider dateTimeProvider)
        {
            var culture = CultureInfo.GetCultureInfo("ar-EG");
            var sb = new StringBuilder(4096);

            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html lang=\"ar\" dir=\"rtl\">");
            sb.AppendLine("<head><meta charset=\"utf-8\" />");
            sb.AppendLine("<style>");
            sb.AppendLine("body{font-family:'Segoe UI',Tahoma,Arial;margin:24px;color:#263238;font-size:12px;}");
            sb.AppendLine("h1{font-size:20px;margin:0 0 4px;color:#1565C0;}");
            sb.AppendLine("h2{font-size:14px;margin:20px 0 8px;color:#37474F;border-bottom:2px solid #1565C0;padding-bottom:4px;}");
            sb.AppendLine(".meta{font-size:11px;color:#607D8B;margin-bottom:12px;}");
            sb.AppendLine("table{width:100%;border-collapse:collapse;margin-bottom:16px;}");
            sb.AppendLine("th,td{border:1px solid #CFD8DC;padding:6px 10px;}");
            sb.AppendLine("th{background:#ECEFF1;font-weight:600;text-align:right;}");
            sb.AppendLine("td{text-align:right;}");
            sb.AppendLine("tr:nth-child(even){background:#FAFAFA;}");
            sb.AppendLine(".price{font-weight:600;color:#2E7D32;}");
            sb.AppendLine(".supplier-header{background:#E3F2FD;color:#1565C0;font-size:14px;font-weight:700;" +
                          "padding:8px 12px;margin:16px 0 6px;border-radius:4px;border-right:4px solid #1565C0;}");
            sb.AppendLine(".footer{margin-top:24px;font-size:10px;color:#90A4AE;text-align:center;border-top:1px solid #ECEFF1;padding-top:8px;}");
            sb.AppendLine("@media print{body{margin:12px;} .supplier-header{break-before:auto;}}");
            sb.AppendLine("</style></head><body>");

            sb.AppendLine($"<h1>قائمة أسعار — {WebUtility.HtmlEncode(nameAr ?? "")}</h1>");

            var metaParts = new List<string>();
            if (!string.IsNullOrWhiteSpace(code))
                metaParts.Add($"الكود: {WebUtility.HtmlEncode(code)}");
            if (validFrom.HasValue)
                metaParts.Add($"من: {validFrom.Value:yyyy-MM-dd}");
            if (validTo.HasValue)
                metaParts.Add($"إلى: {validTo.Value:yyyy-MM-dd}");
            metaParts.Add($"عدد الأصناف: {items.Count}");

            sb.AppendLine($"<div class=\"meta\">{string.Join(" | ", metaParts)}</div>");

            if (!string.IsNullOrWhiteSpace(description))
                sb.AppendLine($"<p style=\"font-size:11px;color:#546E7A;\">{WebUtility.HtmlEncode(description)}</p>");

            // Group by supplier
            var groups = items
                .OrderBy(i => i.SupplierName)
                .ThenBy(i => i.ProductCode)
                .GroupBy(i => string.IsNullOrWhiteSpace(i.SupplierName) ? "بدون مورد" : i.SupplierName);

            foreach (var group in groups)
            {
                sb.AppendLine($"<div class=\"supplier-header\">{WebUtility.HtmlEncode(group.Key)}</div>");
                sb.AppendLine("<table>");
                sb.AppendLine("<thead><tr>");
                sb.AppendLine("<th style=\"width:40px\">#</th>");
                sb.AppendLine("<th style=\"width:80px\">كود الصنف</th>");
                sb.AppendLine("<th>اسم الصنف</th>");
                sb.AppendLine("<th style=\"width:90px\">الحد الأدنى</th>");
                sb.AppendLine("<th style=\"width:100px\">السعر</th>");
                sb.AppendLine("</tr></thead><tbody>");

                var idx = 1;
                foreach (var item in group)
                {
                    sb.AppendLine("<tr>");
                    sb.AppendLine($"<td>{idx++}</td>");
                    sb.AppendLine($"<td>{WebUtility.HtmlEncode(item.ProductCode)}</td>");
                    sb.AppendLine($"<td>{WebUtility.HtmlEncode(item.ProductName)}</td>");
                    sb.AppendLine($"<td>{item.MinQuantity.ToString("N0", culture)}</td>");
                    sb.AppendLine($"<td class=\"price\">{item.PriceListPrice.ToString("N2", culture)}</td>");
                    sb.AppendLine("</tr>");
                }

                sb.AppendLine("</tbody></table>");
            }

            sb.AppendLine($"<div class=\"footer\">MarcoERP — تم الطباعة: {dateTimeProvider.UtcNow:yyyy-MM-dd HH:mm} — " +
                          $"إجمالي الأصناف: {items.Count}</div>");

            sb.AppendLine("</body></html>");
            return sb.ToString();
        }
    }
}
