using FluentValidation;
using MarcoERP.Application.DTOs.Settings;

namespace MarcoERP.Application.Validators.Settings
{
    public sealed class UpdateSystemSettingDtoValidator : AbstractValidator<UpdateSystemSettingDto>
    {
        public UpdateSystemSettingDtoValidator()
        {
            RuleFor(x => x.SettingKey)
                .NotEmpty().WithMessage("مفتاح الإعداد مطلوب.");

            // SettingValue can be null/empty for optional settings
        }
    }
}
