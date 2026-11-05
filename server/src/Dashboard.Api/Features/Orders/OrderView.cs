using Dashboard.Api.Data;
using Dashboard.Api.Data.Entities;

namespace Dashboard.Api.Features.Orders;

/// <summary>
/// Builds <see cref="OrderDto"/> rows, which show the customer, sales person
/// and product names and the order total alongside the order. Call while
/// holding the store's lock.
/// </summary>
internal static class OrderView
{
    public static OrderDto Build(InMemoryDataStore store, OrderEntity order)
    {
        var customer = store.Customers[order.CustomerId];
        var salesPerson = store.Employees[order.SalesPersonId];
        var product = store.Products[order.ProductId];

        return new OrderDto(
            order.Id,
            new OrderCustomerDto(customer.Id, customer.FirstName, customer.MiddleInitial, customer.LastName),
            new OrderSalesPersonDto(salesPerson.Id, salesPerson.FirstName, salesPerson.MiddleInitial, salesPerson.LastName),
            new OrderProductDto(product.Id, product.Name),
            order.Quantity,
            // Prices have at most 2 decimal places, so this is exact to the cent
            product.Price * order.Quantity,
            // Versions come from one shared counter, so the highest one changes
            // whenever anything shown in the row does
            Math.Max(Math.Max(order.Version, customer.Version), Math.Max(salesPerson.Version, product.Version)));
    }

    /// <summary>Order rows showing a customer, employee or product that changed, to push as updates.</summary>
    public static List<OrderDto> BuildWhere(InMemoryDataStore store, Func<OrderEntity, bool> predicate) =>
        store.Orders.Values.Where(predicate).OrderBy(o => o.Id).Select(o => Build(store, o)).ToList();
}
