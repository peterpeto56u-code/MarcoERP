using FluentValidation;
using MarcoERP.Application.DTOs.Treasury;

namespace MarcoERP.Application.Validators.Treasury
{
    public sealed class CreateBankAccountDtoValidator : AbstractValidator<CreateBankAccountDto>
    {
        public CreateBankAccountDtoValidator()
        {
            RuleFor(x => x.NameAr)
                .NotEmpty().WithMessage("اسم الحساب البنكي بالعربي مطلوب.")
                .MaximumLength(100).WithMessage("اسم الحساب البنكي بالعربي لا يتجاوز 100 حرف.");

            RuleFor(x => x.NameEn)
                .MaximumLength(100).WithMessage("اسم الحساب البنكي بالإنجليزي لا يتجاوز 100 حرف.");

            RuleFor(x => x.BankName)
                .MaximumLength(200).WithMessage("اسم البنك لا يتجاوز 200 حرف.");

            RuleFor(x => x.AccountNumber)
                .MaximumLength(50).WithMessage("رقم الحساب لا يتجاوز 50 حرف.");

            RuleFor(x => x.IBAN)
                .MaximumLength(34).WithMessage("رقم الآيبان لا يتجاوز 34 حرف.")
                .Matches(@"^[A-Z]{2}\d{2}[A-Z0-9]{1,30}$")
                .When(x => !string.IsNullOrWhiteSpace(x.IBAN))
                .WithMessage("صيغة الآيبان غير صحيحة.");
        }
    }

    public sealed class UpdateBankAccountDtoValidator : AbstractValidator<UpdateBankAccountDto>
    {
        public UpdateBankAccountDtoValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("معرف الحساب البنكي مطلوب.");

            RuleFor(x => x.NameAr)
                .NotEmpty().WithMessage("اسم الحساب البنكي بالعربي مطلوب.")
                .MaximumLength(100).WithMessage("اسم الحساب البنكي بالعربي لا يتجاوز 100 حرف.");

            RuleFor(x => x.NameEn)
                .MaximumLength(100).WithMessage("اسم الحساب البنكي بالإنجليزي لا يتجاوز 100 حرف.");

            RuleFor(x => x.BankName)
                .MaximumLength(200).WithMessage("اسم البنك لا يتجاوز 200 حرف.");

            RuleFor(x => x.AccountNumber)
                .MaximumLength(50).WithMessage("رقم الحساب لا يتجاوز 50 حرف.");

            RuleFor(x => x.IBAN)
                .MaximumLength(34).WithMessage("رقم الآيبان لا يتجاوز 34 حرف.")
                .Matches(@"^[A-Z]{2}\d{2}[A-Z0-9]{1,30}$")
                .When(x => !string.IsNullOrWhiteSpace(x.IBAN))
                .WithMessage("صيغة الآيبان غير صحيحة.");
        }
    }
}
