using FluentValidation;
using RestaurantSaas.Api.DTOs.Menu;

namespace RestaurantSaas.Api.Validators.Menu;

public class CreateModifierGroupRequestValidator : AbstractValidator<CreateModifierGroupRequest>
{
    private static readonly string[] ValidSelectionTypes = { "Single", "Multiple" };

    public CreateModifierGroupRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("اسم المجموعة مطلوب")
            .MaximumLength(200).WithMessage("اسم المجموعة لا يتجاوز 200 حرف");

        RuleFor(x => x.SelectionType)
            .NotEmpty().WithMessage("نوع الاختيار مطلوب")
            .Must(t => ValidSelectionTypes.Contains(t))
            .WithMessage("نوع الاختيار غير صالح، القيم المقبولة: Single, Multiple");

        RuleFor(x => x.MinSelections)
            .GreaterThanOrEqualTo(0).WithMessage("الحد الأدنى للاختيارات يجب أن يكون صفراً أو أكثر");

        RuleFor(x => x.MaxSelections)
            .GreaterThanOrEqualTo(x => x.MinSelections)
            .When(x => x.MaxSelections.HasValue)
            .WithMessage("الحد الأقصى يجب أن يكون أكبر من أو يساوي الحد الأدنى");

        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0).WithMessage("ترتيب العرض يجب أن يكون صفراً أو أكثر");
    }
}

public class UpdateModifierGroupRequestValidator : AbstractValidator<UpdateModifierGroupRequest>
{
    private static readonly string[] ValidSelectionTypes = { "Single", "Multiple" };

    public UpdateModifierGroupRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("اسم المجموعة مطلوب")
            .MaximumLength(200).WithMessage("اسم المجموعة لا يتجاوز 200 حرف");

        RuleFor(x => x.SelectionType)
            .NotEmpty().WithMessage("نوع الاختيار مطلوب")
            .Must(t => ValidSelectionTypes.Contains(t))
            .WithMessage("نوع الاختيار غير صالح، القيم المقبولة: Single, Multiple");

        RuleFor(x => x.MinSelections)
            .GreaterThanOrEqualTo(0).WithMessage("الحد الأدنى للاختيارات يجب أن يكون صفراً أو أكثر");

        RuleFor(x => x.MaxSelections)
            .GreaterThanOrEqualTo(x => x.MinSelections)
            .When(x => x.MaxSelections.HasValue)
            .WithMessage("الحد الأقصى يجب أن يكون أكبر من أو يساوي الحد الأدنى");

        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0).WithMessage("ترتيب العرض يجب أن يكون صفراً أو أكثر");
    }
}

public class CreateModifierOptionRequestValidator : AbstractValidator<CreateModifierOptionRequest>
{
    public CreateModifierOptionRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("اسم الخيار مطلوب")
            .MaximumLength(200).WithMessage("اسم الخيار لا يتجاوز 200 حرف");

        RuleFor(x => x.PriceDelta)
            .GreaterThanOrEqualTo(0).WithMessage("فارق السعر يجب أن يكون صفراً أو أكثر");

        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0).WithMessage("ترتيب العرض يجب أن يكون صفراً أو أكثر");
    }
}

public class UpdateModifierOptionRequestValidator : AbstractValidator<UpdateModifierOptionRequest>
{
    public UpdateModifierOptionRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("اسم الخيار مطلوب")
            .MaximumLength(200).WithMessage("اسم الخيار لا يتجاوز 200 حرف");

        RuleFor(x => x.PriceDelta)
            .GreaterThanOrEqualTo(0).WithMessage("فارق السعر يجب أن يكون صفراً أو أكثر");

        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0).WithMessage("ترتيب العرض يجب أن يكون صفراً أو أكثر");
    }
}
