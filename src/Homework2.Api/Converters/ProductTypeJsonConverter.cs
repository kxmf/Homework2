using System.Text.Json;
using System.Text.Json.Serialization;
using Homework2.Domain.Product;

namespace Homework2.Api.Converters;

public class ProductTypeJsonConverter : JsonConverter<ProductType>
{
    public override ProductType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number && reader.TryGetInt32(out var intValue))
        {
            if (Enum.IsDefined(typeof(ProductType), intValue))
                return (ProductType)intValue;

            throw new JsonException($"Invalid numeric value for product type: {intValue}.");
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            var value = reader.GetString();
            if (string.IsNullOrWhiteSpace(value))
                throw new JsonException("Product type cannot be empty.");

            return ParseProductType(value);
        }

        throw new JsonException($"Expected a string or number for product type, but got {reader.TokenType}.");
    }

    public override void Write(Utf8JsonWriter writer, ProductType value, JsonSerializerOptions options)
    {
        var stringValue = value switch
        {
            ProductType.General => nameof(ProductType.General),
            ProductType.Homecare => nameof(ProductType.Homecare),
            ProductType.Electronics => nameof(ProductType.Electronics),
            ProductType.Groceries => nameof(ProductType.Groceries),
            _ => value.ToString()
        };

        writer.WriteStringValue(stringValue);
    }

    private static ProductType ParseProductType(string value) =>
        value.Trim().ToLowerInvariant() switch
        {
            "general" or "общий" or "0" => ProductType.General,
            "homecare" or "householdchemicals" or "household_chemicals" or "бытовая химия" or "бытоваяхимия" or "1" => ProductType.Homecare,
            "electronics" or "appliances" or "техника" or "2" => ProductType.Electronics,
            "groceries" or "продукты" or "3" => ProductType.Groceries,
            _ => throw new JsonException(
                $"Unknown product type: '{value}'. Allowed values are: General, Homecare, Electronics, Groceries.")
        };
}
