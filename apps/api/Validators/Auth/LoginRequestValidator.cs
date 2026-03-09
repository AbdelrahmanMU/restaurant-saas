using FluentValidation;
using RestaurantSaas.Api.DTOs.Auth;

namespace RestaurantSaas.Api.Validators.Auth;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("رقم الهاتف مطلوب")
            .MinimumLength(7).WithMessage("رقم الهاتف قصير جداً")
            .MaximumLength(20).WithMessage("رقم الهاتف طويل جداً")
            .Matches(@"^\+?[0-9]+$").WithMessage("رقم الهاتف غير صالح");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("كلمة المرور مطلوبة")
            .MinimumLength(6).WithMessage("كلمة المرور قصيرة جداً");
    }
}
