using Dashboard.Api.Data;
using Dashboard.Api.Data.Entities;

namespace Dashboard.Api.Features.Orders;

/// <summary>Builds <see cref="OrderDto"/> rows. Call while holding store.Sync.</summary>
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
            // Prices have at most 2 decimals, so this is exact
            product.Price * order.Quantity,
            // One shared counter, so the max moves when anything in the row changes
            Math.Max(Math.Max(order.Version, customer.Version), Math.Max(salesPerson.Version, product.Version)));
    }

    /// <summary>Rows to push after a customer, employee or product changes.</summary>
    public static List<OrderDto> BuildWhere(InMemoryDataStore store, Func<OrderEntity, bool> predicate) =>
        store.Orders.Values.Where(predicate).OrderBy(o => o.Id).Select(o => Build(store, o)).ToList();
}
