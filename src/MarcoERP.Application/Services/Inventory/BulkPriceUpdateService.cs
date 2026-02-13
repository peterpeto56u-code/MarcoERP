using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MarcoERP.Application.Common;
using MarcoERP.Application.DTOs.Inventory;
using MarcoERP.Application.Interfaces;
using MarcoERP.Application.Interfaces.Inventory;
using MarcoERP.Domain.Enums;
using MarcoERP.Domain.Interfaces;
using MarcoERP.Domain.Interfaces.Inventory;

namespace MarcoERP.Application.Services.Inventory
{
    [Module(SystemModule.Inventory)]
    public sealed class BulkPriceUpdateService : IBulkPriceUpdateService
    {
        private readonly IProductRepository _productRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;
        private readonly IAuditLogger _auditLogger;

        public BulkPriceUpdateService(
            IProductRepository productRepo,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUser,
            IAuditLogger auditLogger)
        {
            _productRepo = productRepo ?? throw new ArgumentNullException(nameof(productRepo));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
            _auditLogger = auditLogger ?? throw new ArgumentNullException(nameof(auditLogger));
        }

        public async Task<ServiceResult<IReadOnlyList<BulkPricePreviewItemDto>>> PreviewAsync(
            BulkPriceUpdateRequestDto request, CancellationToken ct = default)
        {
            var authCheck = AuthorizationGuard.Check<IReadOnlyList<BulkPricePreviewItemDto>>(_currentUser, PermissionKeys.InventoryManage);
            if (authCheck != null) return authCheck;

            var validationError = ValidateRequest(request);
            if (validationError != null)
                return ServiceResult<IReadOnlyList<BulkPricePreviewItemDto>>.Failure(validationError);

            var items = new List<BulkPricePreviewItemDto>();

            foreach (var productId in request.ProductIds)
            {
                var product = await _productRepo.GetByIdAsync(productId, ct);
                if (product == null) continue;

                var currentPrice = request.PriceTarget == "CostPrice" ? product.CostPrice : product.DefaultSalePrice;
                var newPrice = CalculateNewPrice(currentPrice, request);

                items.Add(new BulkPricePreviewItemDto
                {
                    ProductId = product.Id,
                    Code = product.Code,
                    NameAr = product.NameAr,
                    CurrentPrice = currentPrice,
                    NewPrice = newPrice,
                    Difference = newPrice - currentPrice,
                    PercentageChange = currentPrice != 0 ? Math.Round((newPrice - currentPrice) / currentPrice * 100, 2) : 0
                });
            }

            return ServiceResult<IReadOnlyList<BulkPricePreviewItemDto>>.Success(items);
        }

        public async Task<ServiceResult<BulkPriceUpdateResultDto>> ApplyAsync(
            BulkPriceUpdateRequestDto request, CancellationToken ct = default)
        {
            var authCheck = AuthorizationGuard.Check<BulkPriceUpdateResultDto>(_currentUser, PermissionKeys.InventoryManage);
            if (authCheck != null) return authCheck;

            var validationError = ValidateRequest(request);
            if (validationError != null)
                return ServiceResult<BulkPriceUpdateResultDto>.Failure(validationError);

            var result = new BulkPriceUpdateResultDto();

            foreach (var productId in request.ProductIds)
            {
                try
                {
                    var product = await _productRepo.GetByIdAsync(productId, ct);
                    if (product == null)
                    {
                        result.FailedCount++;
                        result.Errors.Add($"الصنف {productId} غير موجود.");
                        continue;
                    }

                    var currentPrice = request.PriceTarget == "CostPrice" ? product.CostPrice : product.DefaultSalePrice;
                    var newPrice = CalculateNewPrice(currentPrice, request);

                    if (newPrice < 0)
                    {
                        result.FailedCount++;
                        result.Errors.Add($"السعر الجديد سالب للصنف {product.Code}: {newPrice:N2}");
                        continue;
                    }

                    // Update the product price via domain method
                    if (request.PriceTarget == "CostPrice")
                    {
                        product.UpdateCostPrice(newPrice);
                    }
                    else
                    {
                        product.Update(
                            product.NameAr,
                            product.NameEn,
                            product.CategoryId,
                            newPrice,
                            product.MinimumStock,
                            product.ReorderLevel,
                            product.VatRate,
                            product.Barcode,
                            product.Description);
                    }

                    _productRepo.Update(product);

                    await _auditLogger.LogAsync(
                        "Product",
                        product.Id,
                        "BulkPriceUpdate",
                        _currentUser.Username ?? "System",
                        $"تحديث سعر {request.PriceTarget} للصنف {product.Code} من {currentPrice:N2} إلى {newPrice:N2}",
                        ct);

                    result.UpdatedCount++;
                }
                catch (Exception ex)
                {
                    result.FailedCount++;
                    result.Errors.Add($"خطأ في الصنف {productId}: {ex.Message}");
                }
            }

            if (result.UpdatedCount > 0)
                await _unitOfWork.SaveChangesAsync(ct);

            return ServiceResult<BulkPriceUpdateResultDto>.Success(result);
        }

        private static decimal CalculateNewPrice(decimal currentPrice, BulkPriceUpdateRequestDto request)
        {
            if (request.Mode == "Direct")
                return Math.Round(request.DirectPrice, 4);

            // Percentage mode
            var change = currentPrice * request.PercentageChange / 100m;
            return Math.Round(currentPrice + change, 4);
        }

        private static string ValidateRequest(BulkPriceUpdateRequestDto request)
        {
            if (request == null) return "بيانات التحديث مطلوبة.";
            if (request.ProductIds == null || request.ProductIds.Count == 0) return "يجب اختيار صنف واحد على الأقل.";
            if (request.Mode != "Percentage" && request.Mode != "Direct") return "وضع التحديث غير صالح (Percentage أو Direct).";
            if (request.PriceTarget != "SalePrice" && request.PriceTarget != "CostPrice") return "هدف السعر غير صالح.";
            if (request.Mode == "Direct" && request.DirectPrice < 0) return "السعر المباشر لا يمكن أن يكون سالباً.";
            if (request.Mode == "Percentage" && (request.PercentageChange < -100 || request.PercentageChange > 1000))
                return "نسبة التغيير يجب أن تكون بين -100% و 1000%.";
            return null;
        }
    }
}
