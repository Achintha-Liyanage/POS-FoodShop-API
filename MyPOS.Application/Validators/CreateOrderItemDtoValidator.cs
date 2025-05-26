using FluentValidation;
using MyPOS.Application.DTOs.Orders;

namespace MyPOS.Application.Validators
{
    public class CreateOrderItemDtoValidator : AbstractValidator<CreateOrderItemDto>
    {
        public CreateOrderItemDtoValidator()
        {
            RuleFor(x => x.ProductId).GreaterThan(0).WithMessage("Product ID must be valid.");
            RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Quantity must be greater than 0.");
        }
    }
}
