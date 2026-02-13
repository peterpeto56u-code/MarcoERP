using FluentValidation;
using MarcoERP.Application.DTOs.Accounting;

namespace MarcoERP.Application.Validators.Accounting
{
    /// <summary>
    /// FluentValidation rules for <see cref="CreateFiscalYearDto"/>.
    /// </summary>
    public sealed class CreateFiscalYearDtoValidator : AbstractValidator<CreateFiscalYearDto>
    {
        public CreateFiscalYearDtoValidator()
        {
            RuleFor(x => x.Year)
                .InclusiveBetween(2000, 2100)
                .WithMessage("السنة المالية يجب أن تكون بين 2000 و 2100.");
        }
    }
}
