using FluentValidation;
using RestaurantSaas.Api.DTOs.Auth;

namespace RestaurantSaas.Api.Validators.Auth;

public class ActivateInviteRequestValidator : AbstractValidator<ActivateInviteRequest>
{
    public ActivateInviteRequestValidator()
    {
        RuleFor(x => x.InviteToken)
            .NotEmpty().WithMessage("رمز الدعوة مطلوب")
            .Must(t => Guid.TryParse(t, out _)).WithMessage("رمز الدعوة غير صالح");

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("الاسم الكامل مطلوب")
            .MaximumLength(100).WithMessage("الاسم طويل جداً");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("رقم الهاتف مطلوب")
            .MinimumLength(7).WithMessage("رقم الهاتف قصير جداً")
            .MaximumLength(20).WithMessage("رقم الهاتف طويل جداً")
            .Matches(@"^\+?[0-9]+$").WithMessage("رقم الهاتف غير صالح");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("كلمة المرور مطلوبة")
            .MinimumLength(8).WithMessage("كلمة المرور يجب أن تكون 8 أحرف على الأقل");
    }
}
