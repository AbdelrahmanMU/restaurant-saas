using FluentValidation;
using RestaurantSaas.Api.DTOs.Auth;

namespace RestaurantSaas.Api.Validators.Auth;

public class RegisterOwnerRequestValidator : AbstractValidator<RegisterOwnerRequest>
{
    public RegisterOwnerRequestValidator()
    {
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

        RuleFor(x => x.RestaurantName)
            .NotEmpty().WithMessage("اسم المطعم مطلوب")
            .MaximumLength(200).WithMessage("اسم المطعم طويل جداً");
    }
}
