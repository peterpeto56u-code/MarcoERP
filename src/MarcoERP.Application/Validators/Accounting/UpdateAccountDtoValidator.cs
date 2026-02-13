using FluentValidation;
using MarcoERP.Application.DTOs.Accounting;

namespace MarcoERP.Application.Validators.Accounting
{
    /// <summary>
    /// FluentValidation rules for <see cref="UpdateAccountDto"/>.
    /// </summary>
    public sealed class UpdateAccountDtoValidator : AbstractValidator<UpdateAccountDto>
    {
        public UpdateAccountDtoValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("معرّف الحساب غير صالح.");

            RuleFor(x => x.AccountNameAr)
                .NotEmpty().WithMessage("اسم الحساب بالعربي مطلوب.")
                .MaximumLength(200).WithMessage("اسم الحساب بالعربي لا يتجاوز 200 حرف.");

            RuleFor(x => x.AccountNameEn)
                .MaximumLength(200).WithMessage("اسم الحساب بالإنجليزي لا يتجاوز 200 حرف.")
                .When(x => !string.IsNullOrEmpty(x.AccountNameEn));

            RuleFor(x => x.RowVersion)
                .NotNull().WithMessage("بيانات التحكم في التزامن مطلوبة (RowVersion).");
        }
    }
}
