namespace Dashboard.Api.Realtime;

/// <summary>
/// SignalR group names. The only place topic strings are built; clients never
/// send them, they call a typed subscribe method on <see cref="DashboardHub"/>.
/// </summary>
public static class Topics
{
    /// <summary>All customer changes (Customer List page).</summary>
    public const string Customers = "customers";

    /// <summary>All order changes, for every customer (Orders page).</summary>
    public const string Orders = "orders";

    /// <summary>All employee changes.</summary>
    public const string Employees = "employees";

    /// <summary>All product changes.</summary>
    public const string Products = "products";

    /// <summary>Order changes for one customer (Customer Orders page).</summary>
    public static string CustomerOrders(int customerId) => $"customer:{customerId}:orders";
}
