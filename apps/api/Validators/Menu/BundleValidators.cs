using FluentValidation;
using RestaurantSaas.Api.DTOs.Menu;

namespace RestaurantSaas.Api.Validators.Menu;

public class CreateBundleSlotRequestValidator : AbstractValidator<CreateBundleSlotRequest>
{
    public CreateBundleSlotRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("اسم الخانة مطلوب")
            .MaximumLength(200).WithMessage("اسم الخانة لا يتجاوز 200 حرف");

        RuleFor(x => x.MinChoices)
            .GreaterThanOrEqualTo(1).WithMessage("الحد الأدنى للاختيارات يجب أن يكون 1 على الأقل");

        RuleFor(x => x.MaxChoices)
            .GreaterThanOrEqualTo(x => x.MinChoices)
            .WithMessage("الحد الأقصى يجب أن يكون أكبر من أو يساوي الحد الأدنى");
    }
}

public class UpdateBundleSlotRequestValidator : AbstractValidator<UpdateBundleSlotRequest>
{
    public UpdateBundleSlotRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("اسم الخانة مطلوب")
            .MaximumLength(200).WithMessage("اسم الخانة لا يتجاوز 200 حرف");

        RuleFor(x => x.MinChoices)
            .GreaterThanOrEqualTo(1).WithMessage("الحد الأدنى للاختيارات يجب أن يكون 1 على الأقل");

        RuleFor(x => x.MaxChoices)
            .GreaterThanOrEqualTo(x => x.MinChoices)
            .WithMessage("الحد الأقصى يجب أن يكون أكبر من أو يساوي الحد الأدنى");
    }
}

public class AddBundleSlotChoiceRequestValidator : AbstractValidator<AddBundleSlotChoiceRequest>
{
    public AddBundleSlotChoiceRequestValidator()
    {
        RuleFor(x => x.ProductVariantId)
            .NotEmpty().WithMessage("معرّف متغير المنتج مطلوب");

        RuleFor(x => x.PriceDelta)
            .GreaterThanOrEqualTo(0).WithMessage("فارق السعر يجب أن يكون صفراً أو أكثر");
    }
}

public class UpdateBundleSlotChoiceRequestValidator : AbstractValidator<UpdateBundleSlotChoiceRequest>
{
    public UpdateBundleSlotChoiceRequestValidator()
    {
        RuleFor(x => x.PriceDelta)
            .GreaterThanOrEqualTo(0).WithMessage("فارق السعر يجب أن يكون صفراً أو أكثر");
    }
}
