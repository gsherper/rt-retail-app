using Dashboard.Api.Features.Customers;
using Dashboard.Api.Features.Employees;
using Dashboard.Api.Features.Orders;
using Dashboard.Api.Features.Products;
using Microsoft.AspNetCore.SignalR;

namespace Dashboard.Api.Realtime;

/// <summary>Pushes changes to their topics. Call only after the write is saved.</summary>
public interface IChangePublisher
{
    Task CustomerChangedAsync(ChangeEvent<CustomerDto> change);

    Task OrderChangedAsync(int customerId, ChangeEvent<OrderDto> change);

    /// <summary>Pushes <c>updated</c> for order rows whose customer, employee or product changed.</summary>
    Task OrdersUpdatedAsync(IReadOnlyList<OrderDto> orders);

    Task EmployeeChangedAsync(ChangeEvent<EmployeeDto> change);

    Task ProductChangedAsync(ChangeEvent<ProductDto> change);
}

// No CancellationToken: the write is saved, so its event must go out even if the request is cancelled
public sealed class ChangePublisher(
    IHubContext<DashboardHub, IDashboardClient> hub,
    ILogger<ChangePublisher> logger) : IChangePublisher
{
    public Task CustomerChangedAsync(ChangeEvent<CustomerDto> change) =>
        SendAsync(Topics.Customers, change.Type, change.Id, () =>
            hub.Clients.Group(Topics.Customers).CustomerChanged(change));

    // A client on both topics gets the event twice and drops the second by version
    public Task OrderChangedAsync(int customerId, ChangeEvent<OrderDto> change) =>
        Task.WhenAll(
            SendAsync(Topics.Orders, change.Type, change.Id, () =>
                hub.Clients.Group(Topics.Orders).OrderChanged(change)),
            SendAsync(Topics.CustomerOrders(customerId), change.Type, change.Id, () =>
                hub.Clients.Group(Topics.CustomerOrders(customerId)).OrderChanged(change)));

    public async Task OrdersUpdatedAsync(IReadOnlyList<OrderDto> orders)
    {
        foreach (var order in orders)
        {
            await OrderChangedAsync(
                order.Customer.CustomerID, ChangeEvent<OrderDto>.Updated(order.OrderID, order.Version, order));
        }
    }

    public Task EmployeeChangedAsync(ChangeEvent<EmployeeDto> change) =>
        SendAsync(Topics.Employees, change.Type, change.Id, () =>
            hub.Clients.Group(Topics.Employees).EmployeeChanged(change));

    public Task ProductChangedAsync(ChangeEvent<ProductDto> change) =>
        SendAsync(Topics.Products, change.Type, change.Id, () =>
            hub.Clients.Group(Topics.Products).ProductChanged(change));

    private async Task SendAsync(string topic, string type, int id, Func<Task> send)
    {
        try
        {
            await send();
        }
        catch (Exception ex)
        {
            // Don't fail a saved write; clients catch up when they resubscribe
            logger.LogError(ex, "Failed to publish {ChangeType} for {Id} to {Topic}", type, id, topic);
        }
    }
}
