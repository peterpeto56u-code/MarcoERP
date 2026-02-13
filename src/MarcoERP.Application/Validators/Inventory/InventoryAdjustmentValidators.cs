using System;
using FluentValidation;
using MarcoERP.Application.DTOs.Inventory;
using MarcoERP.Application.Interfaces;

namespace MarcoERP.Application.Validators.Inventory
{
    // ════════════════════════════════════════════════════════════
    //  Inventory Adjustment Validators
    // ════════════════════════════════════════════════════════════

    public sealed class CreateInventoryAdjustmentDtoValidator : AbstractValidator<CreateInventoryAdjustmentDto>
    {
        private readonly IDateTimeProvider _dateTimeProvider;

        public CreateInventoryAdjustmentDtoValidator(IDateTimeProvider dateTimeProvider)
        {
            _dateTimeProvider = dateTimeProvider;

            RuleFor(x => x.AdjustmentDate)
                .NotEmpty().WithMessage("تاريخ التسوية مطلوب.")
                .Must(d => d.Year >= 2020 && d <= _dateTimeProvider.UtcNow.AddDays(30))
                .WithMessage("تاريخ التسوية خارج النطاق المسموح.");

            RuleFor(x => x.WarehouseId)
                .GreaterThan(0).WithMessage("المستودع مطلوب.");

            RuleFor(x => x.Reason)
                .NotEmpty().WithMessage("سبب التسوية مطلوب.")
                .MaximumLength(500).WithMessage("سبب التسوية لا يتجاوز 500 حرف.");

            RuleFor(x => x.Notes)
                .MaximumLength(1000).WithMessage("الملاحظات لا تتجاوز 1000 حرف.");

            RuleFor(x => x.Lines)
                .NotEmpty().WithMessage("يجب إضافة بند واحد على الأقل.")
                .ForEach(line =>
                {
                    line.SetValidator(new CreateInventoryAdjustmentLineDtoValidator());
                });
        }
    }

    public sealed class UpdateInventoryAdjustmentDtoValidator : AbstractValidator<UpdateInventoryAdjustmentDto>
    {
        private readonly IDateTimeProvider _dateTimeProvider;

        public UpdateInventoryAdjustmentDtoValidator(IDateTimeProvider dateTimeProvider)
        {
            _dateTimeProvider = dateTimeProvider;

            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("معرف التسوية مطلوب.");

            RuleFor(x => x.AdjustmentDate)
                .NotEmpty().WithMessage("تاريخ التسوية مطلوب.")
                .Must(d => d.Year >= 2020 && d <= _dateTimeProvider.UtcNow.AddDays(30))
                .WithMessage("تاريخ التسوية خارج النطاق المسموح.");

            RuleFor(x => x.WarehouseId)
                .GreaterThan(0).WithMessage("المستودع مطلوب.");

            RuleFor(x => x.Reason)
                .NotEmpty().WithMessage("سبب التسوية مطلوب.")
                .MaximumLength(500).WithMessage("سبب التسوية لا يتجاوز 500 حرف.");

            RuleFor(x => x.Notes)
                .MaximumLength(1000).WithMessage("الملاحظات لا تتجاوز 1000 حرف.");

            RuleFor(x => x.Lines)
                .NotEmpty().WithMessage("يجب إضافة بند واحد على الأقل.")
                .ForEach(line =>
                {
                    line.SetValidator(new CreateInventoryAdjustmentLineDtoValidator());
                });
        }
    }

    public sealed class CreateInventoryAdjustmentLineDtoValidator : AbstractValidator<CreateInventoryAdjustmentLineDto>
    {
        public CreateInventoryAdjustmentLineDtoValidator()
        {
            RuleFor(x => x.ProductId)
                .GreaterThan(0).WithMessage("الصنف مطلوب.");

            RuleFor(x => x.UnitId)
                .GreaterThan(0).WithMessage("الوحدة مطلوبة.");

            RuleFor(x => x.ActualQuantity)
                .GreaterThanOrEqualTo(0).WithMessage("الكمية الفعلية لا يمكن أن تكون سالبة.");
        }
    }
}
