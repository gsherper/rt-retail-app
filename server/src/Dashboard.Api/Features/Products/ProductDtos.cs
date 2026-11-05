using Dashboard.Api.Common;

namespace Dashboard.Api.Features.Products;

/// <summary>A product as returned by the hub and REST and pushed in <c>ProductChanged</c> events.</summary>
public sealed record ProductDto(
    int ProductID,
    string Name,
    decimal Price,
    long Version);

public sealed record CreateProductRequest(string? Name, decimal? Price)
{
    public Dictionary<string, string[]> Validate()
    {
        var errors = new ValidationErrors();
        ProductRules.Validate(errors, Name, Price);
        return errors.ToDictionary();
    }
}

/// <summary>Version is the one the client last saw; a mismatch means someone else saved first.</summary>
public sealed record UpdateProductRequest(string? Name, decimal? Price, long? Version)
{
    public Dictionary<string, string[]> Validate()
    {
        var errors = new ValidationErrors();
        ProductRules.Validate(errors, Name, Price);
        if (Version is null)
            errors.Add("version", "Version is required.");
        return errors.ToDictionary();
    }
}

internal static class ProductRules
{
    public const int MaxNameLength = 100;
    public const decimal MaxPrice = 1_000_000m;

    public static void Validate(ValidationErrors errors, string? name, decimal? price)
    {
        errors.RequiredText("name", "Name", name, MaxNameLength);

        if (price is null)
            errors.Add("price", "Price is required.");
        else if (price < 0 || price > MaxPrice)
            errors.Add("price", $"Price must be between 0 and {MaxPrice:N0}.");
        else if (decimal.Round(price.Value, 2) != price)
            errors.Add("price", "Price can't have more than 2 decimal places.");
    }
}
