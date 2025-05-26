using FluentValidation;
using MyPOS.Application.DTOs.Inventories;

namespace MyPOS.Application.Validators
{
    public class UpdateInventoryDtoValidator : AbstractValidator<UpdateInventoryDto>
    {
        public UpdateInventoryDtoValidator()
        {
            RuleFor(x => x.QuantityInStock).GreaterThanOrEqualTo(0).WithMessage("Quantity in stock must be 0 or greater.");
        }
    }
}
