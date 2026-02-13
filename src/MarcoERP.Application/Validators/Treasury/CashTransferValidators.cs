using System;
using FluentValidation;
using MarcoERP.Application.DTOs.Treasury;
using MarcoERP.Application.Interfaces;

namespace MarcoERP.Application.Validators.Treasury
{
    public sealed class CreateCashTransferDtoValidator : AbstractValidator<CreateCashTransferDto>
    {
        private readonly IDateTimeProvider _dateTimeProvider;

        public CreateCashTransferDtoValidator(IDateTimeProvider dateTimeProvider)
        {
            _dateTimeProvider = dateTimeProvider;

            RuleFor(x => x.TransferDate)
                .NotEmpty().WithMessage("تاريخ التحويل مطلوب.");

            RuleFor(x => x.SourceCashboxId)
                .GreaterThan(0).WithMessage("خزنة المصدر مطلوبة.");

            RuleFor(x => x.TargetCashboxId)
                .GreaterThan(0).WithMessage("خزنة الاستلام مطلوبة.");

            RuleFor(x => x.TargetCashboxId)
                .NotEqual(x => x.SourceCashboxId)
                .WithMessage("لا يمكن التحويل من وإلى نفس الخزنة.");

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("مبلغ التحويل يجب أن يكون أكبر من صفر.")
                .LessThanOrEqualTo(99_999_999_999m).WithMessage("المبلغ يتجاوز الحد الأقصى المسموح.");

            RuleFor(x => x.TransferDate)
                .Must(d => d.Year >= 2020 && d <= _dateTimeProvider.UtcNow.AddDays(30))
                .WithMessage("التاريخ خارج النطاق المسموح.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("وصف التحويل مطلوب.")
                .MaximumLength(500).WithMessage("الوصف لا يتجاوز 500 حرف.");

            RuleFor(x => x.Notes)
                .MaximumLength(500).WithMessage("الملاحظات لا تتجاوز 500 حرف.");
        }
    }

    public sealed class UpdateCashTransferDtoValidator : AbstractValidator<UpdateCashTransferDto>
    {
        private readonly IDateTimeProvider _dateTimeProvider;

        public UpdateCashTransferDtoValidator(IDateTimeProvider dateTimeProvider)
        {
            _dateTimeProvider = dateTimeProvider;

            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("معرف التحويل مطلوب.");

            RuleFor(x => x.TransferDate)
                .NotEmpty().WithMessage("تاريخ التحويل مطلوب.")
                .Must(d => d.Year >= 2020 && d <= _dateTimeProvider.UtcNow.AddDays(30))
                .WithMessage("التاريخ خارج النطاق المسموح.");

            RuleFor(x => x.SourceCashboxId)
                .GreaterThan(0).WithMessage("خزنة المصدر مطلوبة.");

            RuleFor(x => x.TargetCashboxId)
                .GreaterThan(0).WithMessage("خزنة الاستلام مطلوبة.");

            RuleFor(x => x.TargetCashboxId)
                .NotEqual(x => x.SourceCashboxId)
                .WithMessage("لا يمكن التحويل من وإلى نفس الخزنة.");

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("مبلغ التحويل يجب أن يكون أكبر من صفر.")
                .LessThanOrEqualTo(99_999_999_999m).WithMessage("المبلغ يتجاوز الحد الأقصى المسموح.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("وصف التحويل مطلوب.")
                .MaximumLength(500).WithMessage("الوصف لا يتجاوز 500 حرف.");

            RuleFor(x => x.Notes)
                .MaximumLength(500).WithMessage("الملاحظات لا تتجاوز 500 حرف.");
        }
    }
}
