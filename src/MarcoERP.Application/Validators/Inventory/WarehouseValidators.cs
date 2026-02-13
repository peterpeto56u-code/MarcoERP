using FluentValidation;
using MarcoERP.Application.DTOs.Inventory;

namespace MarcoERP.Application.Validators.Inventory
{
    public sealed class CreateWarehouseDtoValidator : AbstractValidator<CreateWarehouseDto>
    {
        public CreateWarehouseDtoValidator()
        {
            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("كود المخزن مطلوب.")
                .MaximumLength(10).WithMessage("كود المخزن لا يتجاوز 10 أحرف.");

            RuleFor(x => x.NameAr)
                .NotEmpty().WithMessage("اسم المخزن بالعربي مطلوب.")
                .MaximumLength(100);

            RuleFor(x => x.NameEn).MaximumLength(100);
            RuleFor(x => x.Address).MaximumLength(300);
            RuleFor(x => x.Phone).MaximumLength(20);
        }
    }

    public sealed class UpdateWarehouseDtoValidator : AbstractValidator<UpdateWarehouseDto>
    {
        public UpdateWarehouseDtoValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0).WithMessage("معرف المخزن مطلوب.");

            RuleFor(x => x.NameAr)
                .NotEmpty().WithMessage("اسم المخزن بالعربي مطلوب.")
                .MaximumLength(100);

            RuleFor(x => x.NameEn).MaximumLength(100);
            RuleFor(x => x.Address).MaximumLength(300);
            RuleFor(x => x.Phone).MaximumLength(20);
        }
    }
}
