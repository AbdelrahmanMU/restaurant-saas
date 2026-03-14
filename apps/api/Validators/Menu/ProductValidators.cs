using FluentValidation;
using RestaurantSaas.Api.DTOs.Menu;

namespace RestaurantSaas.Api.Validators.Menu;

public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    private static readonly string[] ValidTypes = { "Simple", "VariantBased", "Customizable", "Bundle" };

    public CreateProductRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("اسم المنتج مطلوب")
            .MaximumLength(200).WithMessage("اسم المنتج لا يتجاوز 200 حرف");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("الوصف لا يتجاوز 1000 حرف");

        RuleFor(x => x.ImageUrl)
            .MaximumLength(500).WithMessage("رابط الصورة لا يتجاوز 500 حرف");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("نوع المنتج مطلوب")
            .Must(t => ValidTypes.Contains(t))
            .WithMessage("نوع المنتج غير صالح، القيم المقبولة: Simple, VariantBased, Customizable, Bundle");

        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0).WithMessage("ترتيب العرض يجب أن يكون صفراً أو أكثر");
    }
}

public class UpdateProductRequestValidator : AbstractValidator<UpdateProductRequest>
{
    public UpdateProductRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("اسم المنتج مطلوب")
            .MaximumLength(200).WithMessage("اسم المنتج لا يتجاوز 200 حرف");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("الوصف لا يتجاوز 1000 حرف");

        RuleFor(x => x.ImageUrl)
            .MaximumLength(500).WithMessage("رابط الصورة لا يتجاوز 500 حرف");

        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0).WithMessage("ترتيب العرض يجب أن يكون صفراً أو أكثر");
    }
}

public class CreateVariantRequestValidator : AbstractValidator<CreateVariantRequest>
{
    public CreateVariantRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("اسم المتغير مطلوب")
            .MaximumLength(200).WithMessage("اسم المتغير لا يتجاوز 200 حرف");

        RuleFor(x => x.Sku)
            .MaximumLength(100).WithMessage("رمز المنتج (SKU) لا يتجاوز 100 حرف");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("السعر يجب أن يكون صفراً أو أكثر");

        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0).WithMessage("ترتيب العرض يجب أن يكون صفراً أو أكثر");
    }
}

public class UpdateVariantRequestValidator : AbstractValidator<UpdateVariantRequest>
{
    public UpdateVariantRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("اسم المتغير مطلوب")
            .MaximumLength(200).WithMessage("اسم المتغير لا يتجاوز 200 حرف");

        RuleFor(x => x.Sku)
            .MaximumLength(100).WithMessage("رمز المنتج (SKU) لا يتجاوز 100 حرف");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("السعر يجب أن يكون صفراً أو أكثر");

        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0).WithMessage("ترتيب العرض يجب أن يكون صفراً أو أكثر");
    }
}
