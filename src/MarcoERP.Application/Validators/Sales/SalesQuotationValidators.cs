using FluentValidation;
using MarcoERP.Application.DTOs.Sales;

namespace MarcoERP.Application.Validators.Sales
{
    public sealed class CreateSalesQuotationDtoValidator : AbstractValidator<CreateSalesQuotationDto>
    {
        public CreateSalesQuotationDtoValidator()
        {
            RuleFor(x => x.QuotationDate)
                .NotEmpty().WithMessage("تاريخ عرض السعر مطلوب.");
            RuleFor(x => x.ValidUntil)
                .NotEmpty().WithMessage("تاريخ الصلاحية مطلوب.")
                .GreaterThan(x => x.QuotationDate).WithMessage("تاريخ الصلاحية يجب أن يكون بعد تاريخ العرض.");
            RuleFor(x => x.CustomerId)
                .GreaterThan(0).WithMessage("العميل مطلوب.");
            RuleFor(x => x.WarehouseId)
                .GreaterThan(0).WithMessage("المستودع مطلوب.");
            RuleFor(x => x.Lines)
                .NotEmpty().WithMessage("يجب إضافة بند واحد على الأقل.")
                .ForEach(line => { line.SetValidator(new CreateSalesQuotationLineDtoValidator()); });
            RuleFor(x => x.Notes)
                .MaximumLength(1000).WithMessage("الملاحظات لا تتجاوز 1000 حرف.");
        }
    }

    public sealed class UpdateSalesQuotationDtoValidator : AbstractValidator<UpdateSalesQuotationDto>
    {
        public UpdateSalesQuotationDtoValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("معرف عرض السعر غير صالح.");
            RuleFor(x => x.QuotationDate)
                .NotEmpty().WithMessage("تاريخ عرض السعر مطلوب.");
            RuleFor(x => x.ValidUntil)
                .NotEmpty().WithMessage("تاريخ الصلاحية مطلوب.")
                .GreaterThan(x => x.QuotationDate).WithMessage("تاريخ الصلاحية يجب أن يكون بعد تاريخ العرض.");
            RuleFor(x => x.CustomerId)
                .GreaterThan(0).WithMessage("العميل مطلوب.");
            RuleFor(x => x.WarehouseId)
                .GreaterThan(0).WithMessage("المستودع مطلوب.");
            RuleFor(x => x.Lines)
                .NotEmpty().WithMessage("يجب إضافة بند واحد على الأقل.")
                .ForEach(line => { line.SetValidator(new CreateSalesQuotationLineDtoValidator()); });
            RuleFor(x => x.Notes)
                .MaximumLength(1000).WithMessage("الملاحظات لا تتجاوز 1000 حرف.");
        }
    }

    public sealed class CreateSalesQuotationLineDtoValidator : AbstractValidator<CreateSalesQuotationLineDto>
    {
        public CreateSalesQuotationLineDtoValidator()
        {
            RuleFor(x => x.ProductId)
                .GreaterThan(0).WithMessage("الصنف مطلوب.");
            RuleFor(x => x.UnitId)
                .GreaterThan(0).WithMessage("الوحدة مطلوبة.");
            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("الكمية يجب أن تكون أكبر من صفر.");
            RuleFor(x => x.UnitPrice)
                .GreaterThanOrEqualTo(0).WithMessage("سعر الوحدة لا يمكن أن يكون سالباً.");
            RuleFor(x => x.DiscountPercent)
                .InclusiveBetween(0, 100).WithMessage("نسبة الخصم يجب أن تكون بين 0 و 100.");
        }
    }
}
