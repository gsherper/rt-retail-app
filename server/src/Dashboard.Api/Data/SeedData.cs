using Dashboard.Api.Data.Entities;

namespace Dashboard.Api.Data;

/// <summary>Loaded on every start.</summary>
internal static class SeedData
{
    public const int FirstOrderId = 5001;

    public static IEnumerable<CustomerEntity> Customers() =>
    [
        Customer(1001, "James", "A", "Anderson"),
        Customer(1002, "Mary", "B", "Thomas"),
        Customer(1003, "John", null, "Jackson"),
        Customer(1004, "Patricia", "C", "White"),
        Customer(1005, "Robert", "D", "Harris"),
        Customer(1006, "Jennifer", "E", "Martin"),
        Customer(1007, "Michael", null, "Thompson"),
        Customer(1008, "Linda", "F", "Garcia"),
        Customer(1009, "William", "G", "Martinez"),
        Customer(1010, "Elizabeth", null, "Robinson"),
        Customer(1011, "David", "H", "Clark"),
        Customer(1012, "Barbara", "I", "Rodriguez"),
        Customer(1013, "Richard", null, "Lewis"),
        Customer(1014, "Susan", "J", "Lee"),
        Customer(1015, "Joseph", "K", "Walker"),
        Customer(1016, "Jessica", null, "Hall"),
    ];

    public static IEnumerable<EmployeeEntity> Employees() =>
    [
        Employee(121, "Thomas", "R", "Reed"),
        Employee(122, "Karen", null, "Diaz"),
        Employee(123, "Daniel", "S", "Brooks"),
        Employee(124, "Nancy", "L", "Foster"),
        Employee(125, "Steven", null, "Price"),
        Employee(126, "Laura", "M", "Hughes"),
        Employee(127, "David", null, "Cheung"),
    ];

    public static IEnumerable<ProductEntity> Products() =>
    [
        Product(120, "Coffee Mug", 12.50m),
        Product(121, "Tote Bag", 18.00m),
        Product(122, "Notebook", 6.75m),
        Product(123, "Water Bottle", 22.00m),
        Product(124, "Desk Lamp", 39.99m),
        Product(125, "Wireless Mouse", 24.95m),
        Product(126, "Backpack", 54.00m),
        Product(127, "Pen Set", 9.25m),
    ];

    /// <summary>2–7 random orders per customer; fixed seed, so every run is the same.</summary>
    public static IEnumerable<OrderEntity> Orders(
        IEnumerable<int> customerIds, IEnumerable<int> employeeIds, IEnumerable<int> productIds)
    {
        var random = new Random(20261031);
        var orderId = FirstOrderId;
        var employees = employeeIds.Order().ToArray();
        var products = productIds.Order().ToArray();

        foreach (var customerId in customerIds.Order())
        {
            var count = random.Next(2, 8);
            for (var i = 0; i < count; i++)
            {
                yield return new OrderEntity
                {
                    Id = orderId++,
                    CustomerId = customerId,
                    SalesPersonId = employees[random.Next(employees.Length)],
                    ProductId = products[random.Next(products.Length)],
                    Quantity = random.Next(1, 9),
                };
            }
        }
    }

    private static CustomerEntity Customer(int id, string first, string? middle, string last) =>
        new() { Id = id, FirstName = first, MiddleInitial = middle, LastName = last };

    private static EmployeeEntity Employee(int id, string first, string? middle, string last) =>
        new() { Id = id, FirstName = first, MiddleInitial = middle, LastName = last };

    private static ProductEntity Product(int id, string name, decimal price) =>
        new() { Id = id, Name = name, Price = price };
}
