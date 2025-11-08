using FluentValidation;

namespace MultitenantPerDb.Modules.Products.Application.Features.Products.CreateProduct;

/// <summary>
/// Validator for CreateProductCommand
/// </summary>
public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Ürün adı boş olamaz")
            .MaximumLength(200).WithMessage("Ürün adı en fazla 200 karakter olabilir")
            .MinimumLength(3).WithMessage("Ürün adı en az 3 karakter olmalıdır");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Açıklama boş olamaz")
            .MaximumLength(1000).WithMessage("Açıklama en fazla 1000 karakter olabilir");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Fiyat 0'dan büyük olmalıdır")
            .LessThanOrEqualTo(1000000).WithMessage("Fiyat 1.000.000'dan küçük veya eşit olmalıdır")
            .PrecisionScale(18, 2, false).WithMessage("Fiyat en fazla 2 ondalık basamak içerebilir");

        RuleFor(x => x.Stock)
            .GreaterThanOrEqualTo(0).WithMessage("Stok negatif olamaz")
            .LessThanOrEqualTo(100000).WithMessage("Stok 100.000'den küçük veya eşit olmalıdır");
    }
}
