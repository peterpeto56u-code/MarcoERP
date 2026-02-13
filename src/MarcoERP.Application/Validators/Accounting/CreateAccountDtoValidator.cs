using FluentValidation;
using MarcoERP.Application.DTOs.Accounting;
using MarcoERP.Domain.Enums;

namespace MarcoERP.Application.Validators.Accounting
{
    /// <summary>
    /// FluentValidation rules for <see cref="CreateAccountDto"/>.
    /// These are structural/format validations only.
    /// Business-rule validations (code uniqueness, parent existence) are in AccountService.
    /// </summary>
    public sealed class CreateAccountDtoValidator : AbstractValidator<CreateAccountDto>
    {
        public CreateAccountDtoValidator()
        {
            RuleFor(x => x.AccountCode)
                .NotEmpty().WithMessage("كود الحساب مطلوب.")
                .Length(4).WithMessage("كود الحساب يجب أن يكون 4 أرقام بالضبط.")
                .Matches(@"^\d{4}$").WithMessage("كود الحساب يجب أن يحتوي على أرقام فقط.");

            RuleFor(x => x.AccountNameAr)
                .NotEmpty().WithMessage("اسم الحساب بالعربي مطلوب.")
                .MaximumLength(200).WithMessage("اسم الحساب بالعربي لا يتجاوز 200 حرف.");

            RuleFor(x => x.AccountNameEn)
                .MaximumLength(200).WithMessage("اسم الحساب بالإنجليزي لا يتجاوز 200 حرف.")
                .When(x => !string.IsNullOrEmpty(x.AccountNameEn));

            RuleFor(x => x.AccountType)
                .IsInEnum().WithMessage("نوع الحساب غير صالح.");

            RuleFor(x => x.Level)
                .InclusiveBetween(1, 4).WithMessage("مستوى الحساب يجب أن يكون بين 1 و 4.");

            RuleFor(x => x.ParentAccountId)
                .Null().When(x => x.Level == 1)
                .WithMessage("حسابات المستوى الأول لا يمكن أن يكون لها حساب أب.");

            RuleFor(x => x.ParentAccountId)
                .NotNull().When(x => x.Level > 1)
                .WithMessage("الحسابات أسفل المستوى الأول يجب أن يكون لها حساب أب.");

            RuleFor(x => x.CurrencyCode)
                .NotEmpty().WithMessage("رمز العملة مطلوب.")
                .Length(3).WithMessage("رمز العملة يجب أن يكون 3 أحرف (ISO 4217).");
        }
    }
}
