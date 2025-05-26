using FluentValidation;
using MyPOS.Application.DTOs.Orders;

namespace MyPOS.Application.Validators
{
    public class CreateOrderDtoValidator : AbstractValidator<CreateOrderDto>
    {
        public CreateOrderDtoValidator()
        {
            RuleFor(x => x.CustomerId).GreaterThan(0).WithMessage("Customer ID must be valid.");
            RuleFor(x => x.OrderItems).NotEmpty().WithMessage("Order must have at least one item.");
            RuleForEach(x => x.OrderItems).SetValidator(new CreateOrderItemDtoValidator());
        }
    }
}
