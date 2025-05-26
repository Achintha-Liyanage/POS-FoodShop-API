using FluentValidation;
using MyPOS.Application.DTOs.Products;

namespace MyPOS.Application.Validators
{
    public class CreateProductDtoValidator : AbstractValidator<CreateProductDto>
    {
        public CreateProductDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Product name is required.");
            RuleFor(x => x.Price).GreaterThan(0).WithMessage("Product price must be greater than 0.");
        }
    }
}
