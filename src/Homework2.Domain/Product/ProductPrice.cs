using Homework2.Domain.Common;

namespace Homework2.Domain.Product;

public sealed record ProductPrice
{
    public decimal Amount { get; }

    private ProductPrice(decimal amount)
    {
        Amount = amount;
    }

    public static Result<ProductPrice> Create(decimal amount)
    {
        if (amount <= 0)
            return Error.Validation("Product.Price", "Price must be greater than zero.");

        if (decimal.Round(amount, 2) != amount)
            return Error.Validation("Product.Price", "Price cannot have more than two decimal places.");

        return new ProductPrice(amount);
    }
}
