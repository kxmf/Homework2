using Homework2.Domain.Common;

namespace Homework2.Domain.Product;

public readonly record struct ProductWeight
{
    public int Grams { get; }
    public decimal Kilograms => Grams / 1000m;

    private ProductWeight(int grams)
    {
        Grams = grams;
    }

    public static Result<ProductWeight> FromGrams(int grams)
    {
        if (grams <= 0)
            return Error.Validation("Product.Weight", "Weight must be greater than zero.");

        return new ProductWeight(grams);
    }

    public static Result<ProductWeight> FromKilograms(decimal kilograms)
    {
        if (kilograms <= 0)
            return Error.Validation("Product.Weight", "Weight must be greater than zero.");

        if (decimal.Round(kilograms, 3) != kilograms)
            return Error.Validation("Product.Weight", "Weight in kg cannot have more than 3 decimal places.");

        int grams = (int)(kilograms * 1000);
        return new ProductWeight(grams);
    }
}
