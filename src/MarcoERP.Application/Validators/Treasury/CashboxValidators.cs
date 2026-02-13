using FluentValidation;
using MarcoERP.Application.DTOs.Treasury;

namespace MarcoERP.Application.Validators.Treasury
{
    public sealed class CreateCashboxDtoValidator : AbstractValidator<CreateCashboxDto>
    {
        public CreateCashboxDtoValidator()
        {
            RuleFor(x => x.NameAr)
                .NotEmpty().WithMessage("اسم الخزنة بالعربي مطلوب.")
                .MaximumLength(100).WithMessage("اسم الخزنة بالعربي لا يتجاوز 100 حرف.");

            RuleFor(x => x.NameEn)
                .MaximumLength(100).WithMessage("اسم الخزنة بالإنجليزي لا يتجاوز 100 حرف.");
        }
    }

    public sealed class UpdateCashboxDtoValidator : AbstractValidator<UpdateCashboxDto>
    {
        public UpdateCashboxDtoValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("معرف الخزنة مطلوب.");

            RuleFor(x => x.NameAr)
                .NotEmpty().WithMessage("اسم الخزنة بالعربي مطلوب.")
                .MaximumLength(100).WithMessage("اسم الخزنة بالعربي لا يتجاوز 100 حرف.");

            RuleFor(x => x.NameEn)
                .MaximumLength(100).WithMessage("اسم الخزنة بالإنجليزي لا يتجاوز 100 حرف.");
        }
    }
}
