using Dashboard.Api.Data.Entities;

namespace Dashboard.Api.Data;

/// <summary>
/// All data, held in memory for the server's lifetime. Starts from
/// <see cref="SeedData"/> and resets when the server restarts.
/// Callers hold <see cref="Sync"/> for the whole read or write, so each one
/// sees and leaves the data consistent.
/// </summary>
public sealed class InMemoryDataStore
{
    private long _version;
    private int _nextCustomerId;
    private int _nextOrderId;
    private int _nextEmployeeId;
    private int _nextProductId;

    public InMemoryDataStore()
    {
        Customers = SeedData.Customers().ToDictionary(c => c.Id);
        Employees = SeedData.Employees().ToDictionary(e => e.Id);
        Products = SeedData.Products().ToDictionary(p => p.Id);
        Orders = SeedData.Orders(Customers.Keys, Employees.Keys, Products.Keys).ToDictionary(o => o.Id);

        foreach (var customer in Customers.Values) customer.Version = NextVersion();
        foreach (var employee in Employees.Values) employee.Version = NextVersion();
        foreach (var product in Products.Values) product.Version = NextVersion();
        foreach (var order in Orders.Values) order.Version = NextVersion();

        _nextCustomerId = Customers.Keys.Max() + 1;
        _nextEmployeeId = Employees.Keys.Max() + 1;
        _nextProductId = Products.Keys.Max() + 1;
        _nextOrderId = Orders.Keys.DefaultIfEmpty(SeedData.FirstOrderId - 1).Max() + 1;
    }

    public Lock Sync { get; } = new();

    public Dictionary<int, CustomerEntity> Customers { get; }

    public Dictionary<int, EmployeeEntity> Employees { get; }

    public Dictionary<int, ProductEntity> Products { get; }

    public Dictionary<int, OrderEntity> Orders { get; }

    /// <summary>
    /// A new row version, higher than any before it. One shared counter, so a
    /// delete's version is always newer than the row's last update, and an order
    /// row's version moves when anything it shows changes.
    /// </summary>
    public long NextVersion() => ++_version;

    public int NextCustomerId() => _nextCustomerId++;

    public int NextEmployeeId() => _nextEmployeeId++;

    public int NextProductId() => _nextProductId++;

    public int NextOrderId() => _nextOrderId++;
}
