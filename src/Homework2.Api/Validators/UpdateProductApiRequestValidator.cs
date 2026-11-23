using FluentValidation;
using Homework2.Api.Controllers;

namespace Homework2.Api.Validators;

public class UpdateProductApiRequestValidator : AbstractValidator<UpdateProductApiRequest>
{
    public UpdateProductApiRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Product title is required.")
            .MaximumLength(150).WithMessage("Product title length must not exceed 150 characters.");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than zero.");

        RuleFor(x => x.Weight)
            .GreaterThan(0).WithMessage("Weight must be greater than zero.");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Invalid product type specified.");

        RuleFor(x => x.StorageId)
            .NotEmpty().WithMessage("StorageId is required and cannot be empty.");
    }
}
