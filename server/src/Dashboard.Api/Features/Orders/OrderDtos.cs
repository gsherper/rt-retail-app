using Dashboard.Api.Common;

namespace Dashboard.Api.Features.Orders;

/// <summary>An order row with the names the Orders page shows; built by <see cref="OrderView"/>.</summary>
public sealed record OrderDto(
    int OrderID,
    OrderCustomerDto Customer,
    OrderSalesPersonDto SalesPerson,
    OrderProductDto Product,
    int Quantity,
    decimal TotalUsd,
    long Version);

public sealed record OrderCustomerDto(int CustomerID, string FirstName, string? MiddleInitial, string LastName);

/// <summary><c>EmployeeID</c> is the order's <c>SalesPersonID</c>.</summary>
public sealed record OrderSalesPersonDto(int EmployeeID, string FirstName, string? MiddleInitial, string LastName);

public sealed record OrderProductDto(int ProductID, string Name);

/// <summary>The customer comes from the route or hub argument.</summary>
public sealed record CreateOrderRequest(int? SalesPersonID, int? ProductID, int? Quantity)
{
    public Dictionary<string, string[]> Validate()
    {
        var errors = new ValidationErrors();
        OrderRules.Validate(errors, SalesPersonID, ProductID, Quantity);
        return errors.ToDictionary();
    }
}

/// <summary>Can't change the customer. Version is the one the client last saw; a mismatch is a 409.</summary>
public sealed record UpdateOrderRequest(int? SalesPersonID, int? ProductID, int? Quantity, long? Version)
{
    public Dictionary<string, string[]> Validate()
    {
        var errors = new ValidationErrors();
        OrderRules.Validate(errors, SalesPersonID, ProductID, Quantity);
        errors.RequiredVersion(Version);
        return errors.ToDictionary();
    }
}

/// <summary>Shape only; <see cref="OrderService"/> checks the employee and product exist.</summary>
internal static class OrderRules
{
    public const int MaxQuantity = 10_000;

    public static void Validate(ValidationErrors errors, int? salesPersonId, int? productId, int? quantity)
    {
        errors.RequiredId("salesPersonID", "Sales person", salesPersonId);
        errors.RequiredId("productID", "Product", productId);
        errors.Range("quantity", "Quantity", quantity, 1, MaxQuantity);
    }
}
