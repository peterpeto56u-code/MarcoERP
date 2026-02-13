using FluentValidation;
using MarcoERP.Application.DTOs.Inventory;

namespace MarcoERP.Application.Validators.Inventory
{
    public sealed class CreateUnitDtoValidator : AbstractValidator<CreateUnitDto>
    {
        public CreateUnitDtoValidator()
        {
            RuleFor(x => x.NameAr)
                .NotEmpty().WithMessage("اسم الوحدة بالعربي مطلوب.")
                .MaximumLength(50).WithMessage("اسم الوحدة لا يتجاوز 50 حرف.");

            RuleFor(x => x.NameEn)
                .MaximumLength(50);

            RuleFor(x => x.AbbreviationAr)
                .NotEmpty().WithMessage("اختصار الوحدة بالعربي مطلوب.")
                .MaximumLength(10);

            RuleFor(x => x.AbbreviationEn)
                .MaximumLength(10);
        }
    }

    public sealed class UpdateUnitDtoValidator : AbstractValidator<UpdateUnitDto>
    {
        public UpdateUnitDtoValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0).WithMessage("معرف الوحدة مطلوب.");

            RuleFor(x => x.NameAr)
                .NotEmpty().WithMessage("اسم الوحدة بالعربي مطلوب.")
                .MaximumLength(50);

            RuleFor(x => x.AbbreviationAr)
                .NotEmpty().WithMessage("اختصار الوحدة بالعربي مطلوب.")
                .MaximumLength(10);

            RuleFor(x => x.NameEn).MaximumLength(50);
            RuleFor(x => x.AbbreviationEn).MaximumLength(10);
        }
    }
}
