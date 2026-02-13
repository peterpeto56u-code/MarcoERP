using FluentValidation;
using MarcoERP.Application.DTOs.Security;

namespace MarcoERP.Application.Validators.Security
{
    // ════════════════════════════════════════════════════════════
    //  Role Validators (التحقق من بيانات الأدوار)
    // ════════════════════════════════════════════════════════════

    public sealed class CreateRoleDtoValidator : AbstractValidator<CreateRoleDto>
    {
        public CreateRoleDtoValidator()
        {
            RuleFor(x => x.NameAr)
                .NotEmpty().WithMessage("اسم الدور بالعربية مطلوب.")
                .MaximumLength(100).WithMessage("اسم الدور بالعربية لا يتجاوز 100 حرف.");

            RuleFor(x => x.NameEn)
                .MaximumLength(100).WithMessage("اسم الدور بالإنجليزية لا يتجاوز 100 حرف.");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("وصف الدور لا يتجاوز 500 حرف.");
        }
    }

    public sealed class UpdateRoleDtoValidator : AbstractValidator<UpdateRoleDto>
    {
        public UpdateRoleDtoValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("معرف الدور مطلوب.");

            RuleFor(x => x.NameAr)
                .NotEmpty().WithMessage("اسم الدور بالعربية مطلوب.")
                .MaximumLength(100).WithMessage("اسم الدور بالعربية لا يتجاوز 100 حرف.");

            RuleFor(x => x.NameEn)
                .MaximumLength(100).WithMessage("اسم الدور بالإنجليزية لا يتجاوز 100 حرف.");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("وصف الدور لا يتجاوز 500 حرف.");
        }
    }
}
