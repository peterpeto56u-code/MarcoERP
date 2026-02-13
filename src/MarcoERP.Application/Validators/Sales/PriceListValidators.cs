using System;
using FluentValidation;
using MarcoERP.Application.DTOs.Sales;

namespace MarcoERP.Application.Validators.Sales
{
    // ════════════════════════════════════════════════════════════
    //  Price List Validators
    // ════════════════════════════════════════════════════════════

    public sealed class CreatePriceListDtoValidator : AbstractValidator<CreatePriceListDto>
    {
        public CreatePriceListDtoValidator()
        {
            RuleFor(x => x.NameAr)
                .NotEmpty().WithMessage("اسم قائمة الأسعار بالعربية مطلوب.")
                .MaximumLength(100).WithMessage("اسم قائمة الأسعار لا يتجاوز 100 حرف.");

            RuleFor(x => x.NameEn)
                .MaximumLength(100).WithMessage("الاسم بالإنجليزية لا يتجاوز 100 حرف.");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("الوصف لا يتجاوز 500 حرف.");

            RuleFor(x => x.ValidFrom)
                .Must(d => !d.HasValue || d.Value.Year >= 2020)
                .WithMessage("تاريخ البداية خارج النطاق المسموح.");

            RuleFor(x => x.ValidTo)
                .Must((dto, validTo) => !validTo.HasValue || !dto.ValidFrom.HasValue || validTo > dto.ValidFrom)
                .WithMessage("تاريخ الانتهاء يجب أن يكون بعد تاريخ البداية.");

            RuleFor(x => x.Tiers)
                .NotEmpty().WithMessage("يجب إضافة بند سعر واحد على الأقل.")
                .ForEach(tier =>
                {
                    tier.SetValidator(new CreatePriceTierDtoValidator());
                });
        }
    }

    public sealed class UpdatePriceListDtoValidator : AbstractValidator<UpdatePriceListDto>
    {
        public UpdatePriceListDtoValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("معرف قائمة الأسعار مطلوب.");

            RuleFor(x => x.NameAr)
                .NotEmpty().WithMessage("اسم قائمة الأسعار بالعربية مطلوب.")
                .MaximumLength(100).WithMessage("اسم قائمة الأسعار لا يتجاوز 100 حرف.");

            RuleFor(x => x.NameEn)
                .MaximumLength(100).WithMessage("الاسم بالإنجليزية لا يتجاوز 100 حرف.");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("الوصف لا يتجاوز 500 حرف.");

            RuleFor(x => x.ValidFrom)
                .Must(d => !d.HasValue || d.Value.Year >= 2020)
                .WithMessage("تاريخ البداية خارج النطاق المسموح.");

            RuleFor(x => x.ValidTo)
                .Must((dto, validTo) => !validTo.HasValue || !dto.ValidFrom.HasValue || validTo > dto.ValidFrom)
                .WithMessage("تاريخ الانتهاء يجب أن يكون بعد تاريخ البداية.");

            RuleFor(x => x.Tiers)
                .NotEmpty().WithMessage("يجب إضافة بند سعر واحد على الأقل.")
                .ForEach(tier =>
                {
                    tier.SetValidator(new CreatePriceTierDtoValidator());
                });
        }
    }

    public sealed class CreatePriceTierDtoValidator : AbstractValidator<CreatePriceTierDto>
    {
        public CreatePriceTierDtoValidator()
        {
            RuleFor(x => x.ProductId)
                .GreaterThan(0).WithMessage("الصنف مطلوب.");

            RuleFor(x => x.MinimumQuantity)
                .GreaterThanOrEqualTo(0).WithMessage("الحد الأدنى للكمية لا يمكن أن يكون سالباً.");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("السعر يجب أن يكون أكبر من صفر.")
                .LessThanOrEqualTo(99_999_999_999m).WithMessage("السعر يتجاوز الحد الأقصى المسموح.");
        }
    }
}
