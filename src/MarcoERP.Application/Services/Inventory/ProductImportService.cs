using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClosedXML.Excel;
using MarcoERP.Application.Common;
using MarcoERP.Application.DTOs.Inventory;
using MarcoERP.Application.Interfaces;
using MarcoERP.Application.Interfaces.Inventory;
using MarcoERP.Application.Interfaces.Purchases;

namespace MarcoERP.Application.Services.Inventory
{
    /// <summary>
    /// Imports products from Excel files with full validation and lookup resolution.
    /// </summary>
    public sealed class ProductImportService : IProductImportService
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IUnitService _unitService;
        private readonly ISupplierService _supplierService;
        private readonly ICurrentUserService _currentUser;

        // ── Expected column order (Arabic headers) ──
        private static readonly string[] ExpectedHeaders = new[]
        {
            "كود الصنف",        // 0 - Code (required)
            "اسم الصنف (عربي)", // 1 - NameAr (required)
            "اسم الصنف (إنجليزي)", // 2 - NameEn
            "التصنيف",          // 3 - CategoryName (required)
            "الوحدة الأساسية",  // 4 - BaseUnitName (required)
            "سعر التكلفة",      // 5 - CostPrice
            "سعر البيع",       // 6 - DefaultSalePrice
            "الحد الأدنى",      // 7 - MinimumStock
            "حد إعادة الطلب",   // 8 - ReorderLevel
            "نسبة الضريبة %",   // 9 - VatRate
            "الباركود",         // 10 - Barcode
            "الوصف",           // 11 - Description
            "المورد الافتراضي", // 12 - SupplierName
        };

        public ProductImportService(
            IProductService productService,
            ICategoryService categoryService,
            IUnitService unitService,
            ISupplierService supplierService,
            ICurrentUserService currentUser)
        {
            _productService = productService;
            _categoryService = categoryService;
            _unitService = unitService;
            _supplierService = supplierService;
            _currentUser = currentUser;
        }

        /// <inheritdoc />
        public async Task<ServiceResult<IReadOnlyList<ProductImportRowDto>>> ParseExcelAsync(
            string filePath, CancellationToken ct = default)
        {
            var authCheck = AuthorizationGuard.Check<IReadOnlyList<ProductImportRowDto>>(
                _currentUser, PermissionKeys.InventoryManage);
            if (!authCheck.IsSuccess) return authCheck;

            try
            {
                using var workbook = new XLWorkbook(filePath);
                var ws = workbook.Worksheets.FirstOrDefault();
                if (ws == null)
                    return ServiceResult<IReadOnlyList<ProductImportRowDto>>.Failure(
                        "الملف لا يحتوي على أي ورقة عمل.");

                // Validate headers
                var headerErrors = ValidateHeaders(ws);
                if (headerErrors != null)
                    return ServiceResult<IReadOnlyList<ProductImportRowDto>>.Failure(headerErrors);

                // Load lookup data
                var categories = await LoadCategoriesAsync(ct);
                var units = await LoadUnitsAsync(ct);
                var suppliers = await LoadSuppliersAsync(ct);
                var existingCodes = await LoadExistingCodesAsync(ct);

                // Parse rows
                var rows = new List<ProductImportRowDto>();
                var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;

                for (var row = 2; row <= lastRow; row++)
                {
                    ct.ThrowIfCancellationRequested();

                    var code = ws.Cell(row, 1).GetString()?.Trim();
                    if (string.IsNullOrWhiteSpace(code)) continue; // Skip empty rows

                    var importRow = new ProductImportRowDto
                    {
                        RowNumber = row - 1,
                        Code = code,
                        NameAr = ws.Cell(row, 2).GetString()?.Trim(),
                        NameEn = ws.Cell(row, 3).GetString()?.Trim(),
                        CategoryName = ws.Cell(row, 4).GetString()?.Trim(),
                        BaseUnitName = ws.Cell(row, 5).GetString()?.Trim(),
                        CostPrice = GetDecimal(ws.Cell(row, 6)),
                        DefaultSalePrice = GetDecimal(ws.Cell(row, 7)),
                        MinimumStock = GetDecimal(ws.Cell(row, 8)),
                        ReorderLevel = GetDecimal(ws.Cell(row, 9)),
                        VatRate = GetDecimal(ws.Cell(row, 10)),
                        Barcode = ws.Cell(row, 11).GetString()?.Trim(),
                        Description = ws.Cell(row, 12).GetString()?.Trim(),
                        SupplierName = ws.Cell(row, 13).GetString()?.Trim(),
                    };

                    ValidateRow(importRow, categories, units, suppliers, existingCodes);
                    rows.Add(importRow);
                }

                if (rows.Count == 0)
                    return ServiceResult<IReadOnlyList<ProductImportRowDto>>.Failure(
                        "الملف لا يحتوي على أي بيانات أصناف.");

                return ServiceResult<IReadOnlyList<ProductImportRowDto>>.Success(rows);
            }
            catch (Exception ex)
            {
                return ServiceResult<IReadOnlyList<ProductImportRowDto>>.Failure(
                    $"خطأ في قراءة الملف: {ex.Message}");
            }
        }

        /// <inheritdoc />
        public async Task<ServiceResult<ProductImportResultDto>> ImportAsync(
            IReadOnlyList<ProductImportRowDto> rows, CancellationToken ct = default)
        {
            var authCheck = AuthorizationGuard.Check<ProductImportResultDto>(
                _currentUser, PermissionKeys.InventoryManage);
            if (!authCheck.IsSuccess) return authCheck;

            var result = new ProductImportResultDto { TotalRows = rows.Count };

            foreach (var row in rows)
            {
                ct.ThrowIfCancellationRequested();

                if (!row.IsValid)
                {
                    result.SkippedCount++;
                    result.FailedRows.Add(row);
                    continue;
                }

                try
                {
                    var dto = new CreateProductDto
                    {
                        Code = row.Code,
                        NameAr = row.NameAr,
                        NameEn = row.NameEn,
                        CategoryId = row.ResolvedCategoryId!.Value,
                        BaseUnitId = row.ResolvedBaseUnitId!.Value,
                        CostPrice = row.CostPrice,
                        DefaultSalePrice = row.DefaultSalePrice,
                        MinimumStock = row.MinimumStock,
                        ReorderLevel = row.ReorderLevel,
                        VatRate = row.VatRate,
                        Barcode = row.Barcode,
                        Description = row.Description,
                        DefaultSupplierId = row.ResolvedSupplierId,
                        Units = new List<CreateProductUnitDto>()
                    };

                    var createResult = await _productService.CreateAsync(dto, ct);

                    if (createResult.IsSuccess)
                    {
                        result.SuccessCount++;
                    }
                    else
                    {
                        row.IsValid = false;
                        row.Errors.Add(createResult.ErrorMessage);
                        result.FailedCount++;
                        result.FailedRows.Add(row);
                    }
                }
                catch (Exception ex)
                {
                    row.IsValid = false;
                    row.Errors.Add($"خطأ غير متوقع: {ex.Message}");
                    result.FailedCount++;
                    result.FailedRows.Add(row);
                }
            }

            return ServiceResult<ProductImportResultDto>.Success(result);
        }

        /// <inheritdoc />
        public Task<ServiceResult<string>> GenerateTemplateAsync(
            string outputPath, CancellationToken ct = default)
        {
            try
            {
                using var workbook = new XLWorkbook();
                var ws = workbook.AddWorksheet("أصناف");
                ws.RightToLeft = true;

                // Headers
                for (var i = 0; i < ExpectedHeaders.Length; i++)
                {
                    var cell = ws.Cell(1, i + 1);
                    cell.Value = ExpectedHeaders[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = XLColor.FromArgb(33, 150, 243);
                    cell.Style.Font.FontColor = XLColor.White;
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }

                // Sample row
                ws.Cell(2, 1).Value = "P001";
                ws.Cell(2, 2).Value = "صنف تجريبي";
                ws.Cell(2, 3).Value = "Sample Product";
                ws.Cell(2, 4).Value = "تصنيف عام";
                ws.Cell(2, 5).Value = "قطعة";
                ws.Cell(2, 6).Value = 10.00;
                ws.Cell(2, 7).Value = 15.00;
                ws.Cell(2, 8).Value = 5;
                ws.Cell(2, 9).Value = 10;
                ws.Cell(2, 10).Value = 15;
                ws.Cell(2, 11).Value = "123456789";
                ws.Cell(2, 12).Value = "صنف تجريبي للتوضيح";
                ws.Cell(2, 13).Value = "";

                // Column widths
                ws.Column(1).Width = 15;
                ws.Column(2).Width = 25;
                ws.Column(3).Width = 25;
                ws.Column(4).Width = 18;
                ws.Column(5).Width = 15;
                ws.Column(6).Width = 14;
                ws.Column(7).Width = 14;
                ws.Column(8).Width = 12;
                ws.Column(9).Width = 14;
                ws.Column(10).Width = 14;
                ws.Column(11).Width = 18;
                ws.Column(12).Width = 30;
                ws.Column(13).Width = 20;

                // Instruction row
                var noteRow = ws.Cell(4, 1);
                noteRow.Value = "ملاحظة: الأعمدة المطلوبة هي: كود الصنف، اسم الصنف (عربي)، التصنيف، الوحدة الأساسية. باقي الأعمدة اختيارية.";
                noteRow.Style.Font.FontColor = XLColor.Red;
                noteRow.Style.Font.Italic = true;

                workbook.SaveAs(outputPath);
                return Task.FromResult(ServiceResult<string>.Success(outputPath));
            }
            catch (Exception ex)
            {
                return Task.FromResult(ServiceResult<string>.Failure(
                    $"خطأ في إنشاء القالب: {ex.Message}"));
            }
        }

        // ══════════════════════════════════════════════════════════
        // PRIVATE HELPERS
        // ══════════════════════════════════════════════════════════

        private static string ValidateHeaders(IXLWorksheet ws)
        {
            // Only check the first 4 required headers
            var requiredIndices = new[] { 0, 1, 3, 4 }; // Code, NameAr, Category, Unit
            foreach (var idx in requiredIndices)
            {
                var actual = ws.Cell(1, idx + 1).GetString()?.Trim();
                if (string.IsNullOrEmpty(actual))
                    return $"العمود {idx + 1} يجب أن يكون '{ExpectedHeaders[idx]}'.";
            }
            return null;
        }

        private void ValidateRow(
            ProductImportRowDto row,
            Dictionary<string, int> categories,
            Dictionary<string, int> units,
            Dictionary<string, int> suppliers,
            HashSet<string> existingCodes)
        {
            // Required fields
            if (string.IsNullOrWhiteSpace(row.Code))
            {
                row.IsValid = false;
                row.Errors.Add("كود الصنف مطلوب.");
            }
            else if (row.Code.Length > 20)
            {
                row.IsValid = false;
                row.Errors.Add("كود الصنف يجب أن لا يزيد عن 20 حرف.");
            }
            else if (existingCodes.Contains(row.Code))
            {
                row.IsValid = false;
                row.Errors.Add($"الكود '{row.Code}' موجود مسبقاً.");
            }

            if (string.IsNullOrWhiteSpace(row.NameAr))
            {
                row.IsValid = false;
                row.Errors.Add("اسم الصنف (عربي) مطلوب.");
            }
            else if (row.NameAr.Length > 200)
            {
                row.IsValid = false;
                row.Errors.Add("اسم الصنف يجب أن لا يزيد عن 200 حرف.");
            }

            // Category lookup
            if (string.IsNullOrWhiteSpace(row.CategoryName))
            {
                row.IsValid = false;
                row.Errors.Add("التصنيف مطلوب.");
            }
            else if (categories.TryGetValue(row.CategoryName, out var catId))
            {
                row.ResolvedCategoryId = catId;
            }
            else
            {
                row.IsValid = false;
                row.Errors.Add($"التصنيف '{row.CategoryName}' غير موجود في النظام.");
            }

            // Unit lookup
            if (string.IsNullOrWhiteSpace(row.BaseUnitName))
            {
                row.IsValid = false;
                row.Errors.Add("الوحدة الأساسية مطلوبة.");
            }
            else if (units.TryGetValue(row.BaseUnitName, out var unitId))
            {
                row.ResolvedBaseUnitId = unitId;
            }
            else
            {
                row.IsValid = false;
                row.Errors.Add($"الوحدة '{row.BaseUnitName}' غير موجودة في النظام.");
            }

            // Numeric validations
            if (row.CostPrice < 0)
            {
                row.IsValid = false;
                row.Errors.Add("سعر التكلفة لا يمكن أن يكون سالباً.");
            }

            if (row.DefaultSalePrice < 0)
            {
                row.IsValid = false;
                row.Errors.Add("سعر البيع لا يمكن أن يكون سالباً.");
            }

            if (row.MinimumStock < 0)
            {
                row.IsValid = false;
                row.Errors.Add("الحد الأدنى لا يمكن أن يكون سالباً.");
            }

            if (row.VatRate < 0 || row.VatRate > 100)
            {
                row.IsValid = false;
                row.Errors.Add("نسبة الضريبة يجب أن تكون بين 0 و 100.");
            }

            if (!string.IsNullOrEmpty(row.Barcode) && row.Barcode.Length > 50)
            {
                row.IsValid = false;
                row.Errors.Add("الباركود يجب أن لا يزيد عن 50 حرف.");
            }

            if (!string.IsNullOrEmpty(row.Description) && row.Description.Length > 500)
            {
                row.IsValid = false;
                row.Errors.Add("الوصف يجب أن لا يزيد عن 500 حرف.");
            }

            // Supplier lookup (optional)
            if (!string.IsNullOrWhiteSpace(row.SupplierName))
            {
                if (suppliers.TryGetValue(row.SupplierName, out var supId))
                    row.ResolvedSupplierId = supId;
                else
                    row.Errors.Add($"تحذير: المورد '{row.SupplierName}' غير موجود — سيتم تجاهله.");
            }

            // Track code to detect duplicates within the same file
            if (row.IsValid)
                existingCodes.Add(row.Code);
        }

        private async Task<Dictionary<string, int>> LoadCategoriesAsync(CancellationToken ct)
        {
            var result = await _categoryService.GetAllAsync(ct);
            if (!result.IsSuccess || result.Data == null) return new Dictionary<string, int>();
            return result.Data.ToDictionary(c => c.NameAr, c => c.Id, StringComparer.OrdinalIgnoreCase);
        }

        private async Task<Dictionary<string, int>> LoadUnitsAsync(CancellationToken ct)
        {
            var result = await _unitService.GetAllAsync(ct);
            if (!result.IsSuccess || result.Data == null) return new Dictionary<string, int>();
            return result.Data.ToDictionary(u => u.NameAr, u => u.Id, StringComparer.OrdinalIgnoreCase);
        }

        private async Task<Dictionary<string, int>> LoadSuppliersAsync(CancellationToken ct)
        {
            var result = await _supplierService.GetAllAsync(ct);
            if (!result.IsSuccess || result.Data == null) return new Dictionary<string, int>();
            return result.Data.ToDictionary(s => s.NameAr, s => s.Id, StringComparer.OrdinalIgnoreCase);
        }

        private async Task<HashSet<string>> LoadExistingCodesAsync(CancellationToken ct)
        {
            var result = await _productService.GetAllAsync(ct);
            if (!result.IsSuccess || result.Data == null) return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            return new HashSet<string>(result.Data.Select(p => p.Code), StringComparer.OrdinalIgnoreCase);
        }

        private static decimal GetDecimal(IXLCell cell)
        {
            if (cell.IsEmpty()) return 0;
            if (cell.DataType == XLDataType.Number)
                return (decimal)cell.GetDouble();

            var text = cell.GetString()?.Trim();
            return decimal.TryParse(text, out var val) ? val : 0;
        }
    }
}
