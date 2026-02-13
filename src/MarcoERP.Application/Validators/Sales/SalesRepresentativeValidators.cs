using FluentValidation;
using MarcoERP.Application.DTOs.Sales;

namespace MarcoERP.Application.Validators.Sales
{
    public sealed class CreateSalesRepresentativeDtoValidator : AbstractValidator<CreateSalesRepresentativeDto>
    {
        public CreateSalesRepresentativeDtoValidator()
        {
            RuleFor(x => x.Code).NotEmpty().WithMessage("كود المندوب مطلوب.").MaximumLength(20).WithMessage("كود المندوب لا يتجاوز 20 حرف.");
            RuleFor(x => x.NameAr).NotEmpty().WithMessage("اسم المندوب بالعربي مطلوب.").MaximumLength(200).WithMessage("اسم المندوب لا يتجاوز 200 حرف.");
            RuleFor(x => x.NameEn).MaximumLength(200).WithMessage("اسم المندوب بالإنجليزي لا يتجاوز 200 حرف.");
            RuleFor(x => x.Phone).MaximumLength(30).WithMessage("رقم الهاتف لا يتجاوز 30 حرف.");
            RuleFor(x => x.Mobile).MaximumLength(30).WithMessage("رقم الموبايل لا يتجاوز 30 حرف.");
            RuleFor(x => x.Email).MaximumLength(200).WithMessage("البريد الإلكتروني لا يتجاوز 200 حرف.");
            RuleFor(x => x.CommissionRate).InclusiveBetween(0, 100).WithMessage("نسبة العمولة يجب أن تكون بين 0 و 100.");
            RuleFor(x => x.Notes).MaximumLength(1000).WithMessage("الملاحظات لا تتجاوز 1000 حرف.");
        }
    }

    public sealed class UpdateSalesRepresentativeDtoValidator : AbstractValidator<UpdateSalesRepresentativeDto>
    {
        public UpdateSalesRepresentativeDtoValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0).WithMessage("معرف المندوب غير صالح.");
            RuleFor(x => x.NameAr).NotEmpty().WithMessage("اسم المندوب بالعربي مطلوب.").MaximumLength(200).WithMessage("اسم المندوب لا يتجاوز 200 حرف.");
            RuleFor(x => x.NameEn).MaximumLength(200).WithMessage("اسم المندوب بالإنجليزي لا يتجاوز 200 حرف.");
            RuleFor(x => x.Phone).MaximumLength(30).WithMessage("رقم الهاتف لا يتجاوز 30 حرف.");
            RuleFor(x => x.Mobile).MaximumLength(30).WithMessage("رقم الموبايل لا يتجاوز 30 حرف.");
            RuleFor(x => x.Email).MaximumLength(200).WithMessage("البريد الإلكتروني لا يتجاوز 200 حرف.");
            RuleFor(x => x.CommissionRate).InclusiveBetween(0, 100).WithMessage("نسبة العمولة يجب أن تكون بين 0 و 100.");
            RuleFor(x => x.Notes).MaximumLength(1000).WithMessage("الملاحظات لا تتجاوز 1000 حرف.");
        }
    }
}
