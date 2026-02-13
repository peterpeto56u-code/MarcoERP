using FluentValidation;
using MarcoERP.Application.DTOs.Inventory;

namespace MarcoERP.Application.Validators.Inventory
{
    public sealed class CreateCategoryDtoValidator : AbstractValidator<CreateCategoryDto>
    {
        public CreateCategoryDtoValidator()
        {
            RuleFor(x => x.NameAr)
                .NotEmpty().WithMessage("اسم التصنيف بالعربي مطلوب.")
                .MaximumLength(100).WithMessage("اسم التصنيف لا يتجاوز 100 حرف.");

            RuleFor(x => x.NameEn)
                .MaximumLength(100).WithMessage("اسم التصنيف بالإنجليزي لا يتجاوز 100 حرف.");

            RuleFor(x => x.Level)
                .InclusiveBetween(1, 3).WithMessage("مستوى التصنيف يجب أن يكون بين 1 و 3.");

            RuleFor(x => x.ParentCategoryId)
                .Null().When(x => x.Level == 1).WithMessage("تصنيفات المستوى الأول لا تقبل تصنيف أب.")
                .NotNull().When(x => x.Level > 1).WithMessage("التصنيفات الفرعية يجب أن يكون لها تصنيف أب.");

            RuleFor(x => x.Description)
                .MaximumLength(500);
        }
    }

    public sealed class UpdateCategoryDtoValidator : AbstractValidator<UpdateCategoryDto>
    {
        public UpdateCategoryDtoValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0).WithMessage("معرف التصنيف مطلوب.");

            RuleFor(x => x.NameAr)
                .NotEmpty().WithMessage("اسم التصنيف بالعربي مطلوب.")
                .MaximumLength(100);

            RuleFor(x => x.NameEn).MaximumLength(100);
            RuleFor(x => x.Description).MaximumLength(500);
        }
    }
}
