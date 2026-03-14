using FluentValidation;
using RestaurantSaas.Api.DTOs.Branches;

namespace RestaurantSaas.Api.Validators.Branches;

public class CreateBranchRequestValidator : AbstractValidator<CreateBranchRequest>
{
    public CreateBranchRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("اسم الفرع مطلوب")
            .MaximumLength(100).WithMessage("اسم الفرع لا يتجاوز 100 حرف");

        RuleFor(x => x.Address)
            .MaximumLength(300).WithMessage("العنوان لا يتجاوز 300 حرف");
    }
}

public class UpdateBranchRequestValidator : AbstractValidator<UpdateBranchRequest>
{
    public UpdateBranchRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("اسم الفرع مطلوب")
            .MaximumLength(100).WithMessage("اسم الفرع لا يتجاوز 100 حرف");

        RuleFor(x => x.Address)
            .MaximumLength(300).WithMessage("العنوان لا يتجاوز 300 حرف");
    }
}
