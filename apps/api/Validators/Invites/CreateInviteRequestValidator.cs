using FluentValidation;
using RestaurantSaas.Api.Domain.Enums;
using RestaurantSaas.Api.DTOs.Invites;

namespace RestaurantSaas.Api.Validators.Invites;

public class CreateInviteRequestValidator : AbstractValidator<CreateInviteRequest>
{
    private static readonly string[] ValidRoles =
        Enum.GetNames<Role>().Where(r => r != nameof(Role.Owner)).ToArray();

    public CreateInviteRequestValidator()
    {
        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("رقم الهاتف مطلوب")
            .MinimumLength(7).WithMessage("رقم الهاتف قصير جداً")
            .MaximumLength(20).WithMessage("رقم الهاتف طويل جداً")
            .Matches(@"^\+?[0-9]+$").WithMessage("رقم الهاتف غير صالح");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("الدور مطلوب")
            .Must(r => ValidRoles.Contains(r))
            .WithMessage($"الدور غير صالح. الأدوار المتاحة: {string.Join(", ", ValidRoles)}");
    }
}
