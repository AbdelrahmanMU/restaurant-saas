using FluentValidation;
using RestaurantSaas.Api.DTOs.Menu;

namespace RestaurantSaas.Api.Validators.Menu;

public class CreateMenuSectionRequestValidator : AbstractValidator<CreateMenuSectionRequest>
{
    public CreateMenuSectionRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("اسم القسم مطلوب")
            .MaximumLength(200).WithMessage("اسم القسم لا يتجاوز 200 حرف");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("الوصف لا يتجاوز 500 حرف");

        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0).WithMessage("ترتيب العرض يجب أن يكون صفراً أو أكثر");
    }
}

public class UpdateMenuSectionRequestValidator : AbstractValidator<UpdateMenuSectionRequest>
{
    public UpdateMenuSectionRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("اسم القسم مطلوب")
            .MaximumLength(200).WithMessage("اسم القسم لا يتجاوز 200 حرف");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("الوصف لا يتجاوز 500 حرف");

        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0).WithMessage("ترتيب العرض يجب أن يكون صفراً أو أكثر");
    }
}
