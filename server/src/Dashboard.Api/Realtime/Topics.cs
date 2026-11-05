namespace Dashboard.Api.Realtime;

/// <summary>SignalR group names, built only here. Clients never send topic strings.</summary>
public static class Topics
{
    /// <summary>Customer List page.</summary>
    public const string Customers = "customers";

    /// <summary>Orders page: every customer's orders.</summary>
    public const string Orders = "orders";

    public const string Employees = "employees";

    public const string Products = "products";

    /// <summary>Customer Orders page: one customer's orders.</summary>
    public static string CustomerOrders(int customerId) => $"customer:{customerId}:orders";
}
