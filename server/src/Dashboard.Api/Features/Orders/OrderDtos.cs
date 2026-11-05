using Dashboard.Api.Common;

namespace Dashboard.Api.Features.Orders;

/// <summary>
/// An order as returned by the hub and REST and pushed in <c>OrderChanged</c>
/// events, with the names the Orders page shows. Version is the highest of the
/// order's own version and those of its customer, sales person and product, so
/// renaming any of them makes the row newer. See <see cref="OrderView"/>.
/// TotalUsd is the product's current price times the quantity.
/// </summary>
public sealed record OrderDto(
    int OrderID,
    OrderCustomerDto Customer,
    OrderSalesPersonDto SalesPerson,
    OrderProductDto Product,
    int Quantity,
    decimal TotalUsd,
    long Version);

public sealed record OrderCustomerDto(int CustomerID, string FirstName, string? MiddleInitial, string LastName);

/// <summary>The employee who made the sale; EmployeeID is the order's SalesPersonID.</summary>
public sealed record OrderSalesPersonDto(int EmployeeID, string FirstName, string? MiddleInitial, string LastName);

public sealed record OrderProductDto(int ProductID, string Name);

/// <summary>
/// The customer comes from the route or hub argument. SalesPersonID is an
/// employee and ProductID a product; the service checks both exist.
/// </summary>
public sealed record CreateOrderRequest(int? SalesPersonID, int? ProductID, int? Quantity)
{
    public Dictionary<string, string[]> Validate()
    {
        var errors = new ValidationErrors();
        OrderRules.Validate(errors, SalesPersonID, ProductID, Quantity);
        return errors.ToDictionary();
    }
}

/// <summary>
/// An order can't move to another customer. Version is the one the client last
/// saw; a mismatch means someone else saved first.
/// </summary>
public sealed record UpdateOrderRequest(int? SalesPersonID, int? ProductID, int? Quantity, long? Version)
{
    public Dictionary<string, string[]> Validate()
    {
        var errors = new ValidationErrors();
        OrderRules.Validate(errors, SalesPersonID, ProductID, Quantity);
        if (Version is null)
            errors.Add("version", "Version is required.");
        return errors.ToDictionary();
    }
}

internal static class OrderRules
{
    public const int MaxQuantity = 10_000;

    public static void Validate(ValidationErrors errors, int? salesPersonId, int? productId, int? quantity)
    {
        errors.Range("salesPersonID", "Sales person", salesPersonId, 1, int.MaxValue);
        errors.Range("productID", "Product", productId, 1, int.MaxValue);
        errors.Range("quantity", "Quantity", quantity, 1, MaxQuantity);
    }
}
