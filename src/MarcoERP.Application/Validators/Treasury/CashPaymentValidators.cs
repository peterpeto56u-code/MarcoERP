using System;
using FluentValidation;
using MarcoERP.Application.DTOs.Treasury;
using MarcoERP.Application.Interfaces;

namespace MarcoERP.Application.Validators.Treasury
{
    public sealed class CreateCashPaymentDtoValidator : AbstractValidator<CreateCashPaymentDto>
    {
        private readonly IDateTimeProvider _dateTimeProvider;

        public CreateCashPaymentDtoValidator(IDateTimeProvider dateTimeProvider)
        {
            _dateTimeProvider = dateTimeProvider;

            RuleFor(x => x.PaymentDate)
                .NotEmpty().WithMessage("تاريخ سند الصرف مطلوب.");

            RuleFor(x => x.CashboxId)
                .GreaterThan(0).WithMessage("الخزنة مطلوبة.");

            RuleFor(x => x.AccountId)
                .GreaterThan(0).WithMessage("الحساب المقابل مطلوب.");

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("مبلغ سند الصرف يجب أن يكون أكبر من صفر.")
                .LessThanOrEqualTo(99_999_999_999m).WithMessage("المبلغ يتجاوز الحد الأقصى المسموح.");

            RuleFor(x => x.PaymentDate)
                .Must(d => d.Year >= 2020 && d <= _dateTimeProvider.UtcNow.AddDays(30))
                .WithMessage("التاريخ خارج النطاق المسموح.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("وصف سند الصرف مطلوب.")
                .MaximumLength(500).WithMessage("الوصف لا يتجاوز 500 حرف.");

            RuleFor(x => x.Notes)
                .MaximumLength(500).WithMessage("الملاحظات لا تتجاوز 500 حرف.");
        }
    }

    public sealed class UpdateCashPaymentDtoValidator : AbstractValidator<UpdateCashPaymentDto>
    {
        private readonly IDateTimeProvider _dateTimeProvider;

        public UpdateCashPaymentDtoValidator(IDateTimeProvider dateTimeProvider)
        {
            _dateTimeProvider = dateTimeProvider;

            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("معرف سند الصرف مطلوب.");

            RuleFor(x => x.PaymentDate)
                .NotEmpty().WithMessage("تاريخ سند الصرف مطلوب.")
                .Must(d => d.Year >= 2020 && d <= _dateTimeProvider.UtcNow.AddDays(30))
                .WithMessage("التاريخ خارج النطاق المسموح.");

            RuleFor(x => x.CashboxId)
                .GreaterThan(0).WithMessage("الخزنة مطلوبة.");

            RuleFor(x => x.AccountId)
                .GreaterThan(0).WithMessage("الحساب المقابل مطلوب.");

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("مبلغ سند الصرف يجب أن يكون أكبر من صفر.")
                .LessThanOrEqualTo(99_999_999_999m).WithMessage("المبلغ يتجاوز الحد الأقصى المسموح.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("وصف سند الصرف مطلوب.")
                .MaximumLength(500).WithMessage("الوصف لا يتجاوز 500 حرف.");

            RuleFor(x => x.Notes)
                .MaximumLength(500).WithMessage("الملاحظات لا تتجاوز 500 حرف.");
        }
    }
}
