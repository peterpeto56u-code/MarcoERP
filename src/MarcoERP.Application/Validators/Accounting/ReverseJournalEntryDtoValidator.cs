using FluentValidation;
using MarcoERP.Application.DTOs.Accounting;

namespace MarcoERP.Application.Validators.Accounting
{
    /// <summary>
    /// FluentValidation rules for <see cref="ReverseJournalEntryDto"/>.
    /// </summary>
    public sealed class ReverseJournalEntryDtoValidator : AbstractValidator<ReverseJournalEntryDto>
    {
        public ReverseJournalEntryDtoValidator()
        {
            RuleFor(x => x.JournalEntryId)
                .GreaterThan(0).WithMessage("معرّف القيد غير صالح.");

            RuleFor(x => x.ReversalReason)
                .NotEmpty().WithMessage("سبب العكس مطلوب.")
                .MaximumLength(500).WithMessage("سبب العكس لا يتجاوز 500 حرف.");

            RuleFor(x => x.ReversalDate)
                .NotEmpty().WithMessage("تاريخ قيد العكس مطلوب.");
        }
    }
}
