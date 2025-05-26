using FluentValidation;
using MyPOS.Application.DTOs.Customers;

namespace MyPOS.Application.Validators
{
    public class UpdateCustomerDtoValidator : AbstractValidator<UpdateCustomerDto>
    {
        public UpdateCustomerDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Customer name is required.");
            RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrEmpty(x.Email)).WithMessage("A valid email is required if provided.");
        }
    }
}
