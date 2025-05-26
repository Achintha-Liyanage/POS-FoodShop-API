using FluentValidation;
using MyPOS.Application.DTOs.Customers;

namespace MyPOS.Application.Validators
{
    public class CreateCustomerDtoValidator : AbstractValidator<CreateCustomerDto>
    {
        public CreateCustomerDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Customer name is required.");
            RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrEmpty(x.Email)).WithMessage("A valid email is required if provided.");
        }
    }
}
