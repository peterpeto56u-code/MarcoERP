using FluentValidation;
using MarcoERP.Application.DTOs.Sales;

namespace MarcoERP.Application.Validators.Sales
{
    // ═══════════════════════════════════════════════════════════
    //  POS Validators
    // ═══════════════════════════════════════════════════════════

    /// <summary>Validates OpenPosSessionDto.</summary>
    public sealed class OpenPosSessionDtoValidator : AbstractValidator<OpenPosSessionDto>
    {
        public OpenPosSessionDtoValidator()
        {
            RuleFor(x => x.CashboxId)
                .GreaterThan(0).WithMessage("الخزنة مطلوبة.");

            RuleFor(x => x.WarehouseId)
                .GreaterThan(0).WithMessage("المستودع مطلوب.");

            RuleFor(x => x.OpeningBalance)
                .GreaterThanOrEqualTo(0).WithMessage("الرصيد الافتتاحي لا يمكن أن يكون سالباً.");
        }
    }

    /// <summary>Validates ClosePosSessionDto.</summary>
    public sealed class ClosePosSessionDtoValidator : AbstractValidator<ClosePosSessionDto>
    {
        public ClosePosSessionDtoValidator()
        {
            RuleFor(x => x.SessionId)
                .GreaterThan(0).WithMessage("معرف الجلسة غير صالح.");

            RuleFor(x => x.ActualClosingBalance)
                .GreaterThanOrEqualTo(0).WithMessage("رصيد الإغلاق لا يمكن أن يكون سالباً.");

            RuleFor(x => x.Notes)
                .MaximumLength(1000).WithMessage("الملاحظات لا تتجاوز 1000 حرف.");
        }
    }

    /// <summary>Validates CompletePoseSaleDto.</summary>
    public sealed class CompletePosSaleDtoValidator : AbstractValidator<CompletePoseSaleDto>
    {
        public CompletePosSaleDtoValidator()
        {
            RuleFor(x => x.SessionId)
                .GreaterThan(0).WithMessage("جلسة نقطة البيع مطلوبة.");

            RuleFor(x => x.Lines)
                .NotEmpty().WithMessage("يجب إضافة بند واحد على الأقل.")
                .ForEach(line =>
                {
                    line.SetValidator(new PosSaleLineDtoValidator());
                });

            RuleFor(x => x.Payments)
                .NotEmpty().WithMessage("يجب تحديد طريقة دفع واحدة على الأقل.")
                .ForEach(payment =>
                {
                    payment.SetValidator(new PosPaymentDtoValidator());
                });

            RuleFor(x => x.Notes)
                .MaximumLength(1000).WithMessage("الملاحظات لا تتجاوز 1000 حرف.");
        }
    }

    /// <summary>Validates a POS sale line.</summary>
    public sealed class PosSaleLineDtoValidator : AbstractValidator<PosSaleLineDto>
    {
        public PosSaleLineDtoValidator()
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

    /// <summary>Validates a POS payment entry.</summary>
    public sealed class PosPaymentDtoValidator : AbstractValidator<PosPaymentDto>
    {
        public PosPaymentDtoValidator()
        {
            RuleFor(x => x.PaymentMethod)
                .NotEmpty().WithMessage("طريقة الدفع مطلوبة.")
                .Must(m => m == "Cash" || m == "Card" || m == "OnAccount")
                .WithMessage("طريقة الدفع غير صالحة. المسموح: Cash, Card, OnAccount.");

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("مبلغ الدفع يجب أن يكون أكبر من صفر.");

            RuleFor(x => x.ReferenceNumber)
                .MaximumLength(100).WithMessage("رقم المرجع لا يتجاوز 100 حرف.");
        }
    }
}
