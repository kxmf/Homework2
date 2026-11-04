using FluentValidation;
using Homework2.Application.UseCases.Products.GetProducts;

namespace Homework2.Api.Validators;

public class GetProductsValidator : AbstractValidator<GetProductsDto>
{
    public GetProductsValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("Page number must be greater than or equal to 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100.");

        RuleFor(x => x.StorageId)
            .Must(id => !id.HasValue || id.Value != Guid.Empty)
            .WithMessage("StorageId cannot be an empty GUID.");

        RuleFor(x => x.Type)
            .IsInEnum()
            .When(x => x.Type.HasValue)
            .WithMessage("Invalid product type specified.");
    }
}
