using FluentValidation;
using MarcoERP.Application.DTOs.Security;

namespace MarcoERP.Application.Validators.Security
{
    // ════════════════════════════════════════════════════════════
    //  User Validators
    // ════════════════════════════════════════════════════════════

    public sealed class CreateUserDtoValidator : AbstractValidator<CreateUserDto>
    {
        public CreateUserDtoValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("اسم المستخدم مطلوب.")
                .MinimumLength(3).WithMessage("اسم المستخدم يجب أن يكون 3 أحرف على الأقل.")
                .MaximumLength(50).WithMessage("اسم المستخدم لا يتجاوز 50 حرف.")
                .Matches(@"^[a-zA-Z0-9._-]+$").WithMessage("اسم المستخدم يحتوي فقط على أحرف إنجليزية وأرقام ونقاط وشرطات.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("كلمة المرور مطلوبة.")
                .MinimumLength(8).WithMessage("كلمة المرور يجب أن تكون 8 أحرف على الأقل.")
                .MaximumLength(100).WithMessage("كلمة المرور لا تتجاوز 100 حرف.")
                .Matches(@"[A-Z]").WithMessage("كلمة المرور يجب أن تحتوي على حرف كبير واحد على الأقل.")
                .Matches(@"[a-z]").WithMessage("كلمة المرور يجب أن تحتوي على حرف صغير واحد على الأقل.")
                .Matches(@"\d").WithMessage("كلمة المرور يجب أن تحتوي على رقم واحد على الأقل.");

            RuleFor(x => x.ConfirmPassword)
                .Equal(x => x.Password).WithMessage("كلمة المرور وتأكيدها غير متطابقتين.");

            RuleFor(x => x.FullNameAr)
                .NotEmpty().WithMessage("الاسم الكامل بالعربية مطلوب.")
                .MaximumLength(100).WithMessage("الاسم الكامل لا يتجاوز 100 حرف.");

            RuleFor(x => x.FullNameEn)
                .MaximumLength(100).WithMessage("الاسم بالإنجليزية لا يتجاوز 100 حرف.");

            RuleFor(x => x.Email)
                .MaximumLength(200).WithMessage("البريد الإلكتروني لا يتجاوز 200 حرف.")
                .EmailAddress().When(x => !string.IsNullOrEmpty(x.Email))
                .WithMessage("البريد الإلكتروني غير صالح.");

            RuleFor(x => x.Phone)
                .MaximumLength(20).WithMessage("رقم الهاتف لا يتجاوز 20 حرف.");

            RuleFor(x => x.RoleId)
                .GreaterThan(0).WithMessage("الدور مطلوب.");
        }
    }

    public sealed class UpdateUserDtoValidator : AbstractValidator<UpdateUserDto>
    {
        public UpdateUserDtoValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("معرف المستخدم مطلوب.");

            RuleFor(x => x.FullNameAr)
                .NotEmpty().WithMessage("الاسم الكامل بالعربية مطلوب.")
                .MaximumLength(100).WithMessage("الاسم الكامل لا يتجاوز 100 حرف.");

            RuleFor(x => x.FullNameEn)
                .MaximumLength(100).WithMessage("الاسم بالإنجليزية لا يتجاوز 100 حرف.");

            RuleFor(x => x.Email)
                .MaximumLength(200).WithMessage("البريد الإلكتروني لا يتجاوز 200 حرف.")
                .EmailAddress().When(x => !string.IsNullOrEmpty(x.Email))
                .WithMessage("البريد الإلكتروني غير صالح.");

            RuleFor(x => x.Phone)
                .MaximumLength(20).WithMessage("رقم الهاتف لا يتجاوز 20 حرف.");

            RuleFor(x => x.RoleId)
                .GreaterThan(0).WithMessage("الدور مطلوب.");
        }
    }

    public sealed class ChangePasswordDtoValidator : AbstractValidator<ChangePasswordDto>
    {
        public ChangePasswordDtoValidator()
        {
            RuleFor(x => x.CurrentPassword)
                .NotEmpty().WithMessage("كلمة المرور الحالية مطلوبة.");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("كلمة المرور الجديدة مطلوبة.")
                .MinimumLength(8).WithMessage("كلمة المرور الجديدة يجب أن تكون 8 أحرف على الأقل.")
                .MaximumLength(100).WithMessage("كلمة المرور الجديدة لا تتجاوز 100 حرف.")
                .Matches(@"[A-Z]").WithMessage("كلمة المرور يجب أن تحتوي على حرف كبير واحد على الأقل.")
                .Matches(@"[a-z]").WithMessage("كلمة المرور يجب أن تحتوي على حرف صغير واحد على الأقل.")
                .Matches(@"\d").WithMessage("كلمة المرور يجب أن تحتوي على رقم واحد على الأقل.");

            RuleFor(x => x.ConfirmNewPassword)
                .Equal(x => x.NewPassword).WithMessage("كلمة المرور الجديدة وتأكيدها غير متطابقتين.");
        }
    }

    public sealed class ResetPasswordDtoValidator : AbstractValidator<ResetPasswordDto>
    {
        public ResetPasswordDtoValidator()
        {
            RuleFor(x => x.UserId)
                .GreaterThan(0).WithMessage("معرف المستخدم مطلوب.");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("كلمة المرور الجديدة مطلوبة.")
                .MinimumLength(8).WithMessage("كلمة المرور الجديدة يجب أن تكون 8 أحرف على الأقل.")
                .MaximumLength(100).WithMessage("كلمة المرور الجديدة لا تتجاوز 100 حرف.")
                .Matches(@"[A-Z]").WithMessage("كلمة المرور يجب أن تحتوي على حرف كبير واحد على الأقل.")
                .Matches(@"[a-z]").WithMessage("كلمة المرور يجب أن تحتوي على حرف صغير واحد على الأقل.")
                .Matches(@"\d").WithMessage("كلمة المرور يجب أن تحتوي على رقم واحد على الأقل.");

            RuleFor(x => x.ConfirmNewPassword)
                .Equal(x => x.NewPassword).WithMessage("كلمة المرور الجديدة وتأكيدها غير متطابقتين.");
        }
    }

    public sealed class LoginDtoValidator : AbstractValidator<LoginDto>
    {
        public LoginDtoValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("اسم المستخدم مطلوب.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("كلمة المرور مطلوبة.");
        }
    }
}
