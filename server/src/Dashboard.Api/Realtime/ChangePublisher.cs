using Dashboard.Api.Features.Customers;
using Dashboard.Api.Features.Employees;
using Dashboard.Api.Features.Orders;
using Dashboard.Api.Features.Products;
using Microsoft.AspNetCore.SignalR;

namespace Dashboard.Api.Realtime;

/// <summary>Pushes committed changes to the matching topic. Call only after the write is saved.</summary>
public interface IChangePublisher
{
    Task CustomerChangedAsync(ChangeEvent<CustomerDto> change);

    Task OrderChangedAsync(int customerId, ChangeEvent<OrderDto> change);

    /// <summary>
    /// Sends an <c>updated</c> event for each order row, for when a customer,
    /// employee or product shown in those rows was renamed.
    /// </summary>
    Task OrdersUpdatedAsync(IReadOnlyList<OrderDto> orders);

    Task EmployeeChangedAsync(ChangeEvent<EmployeeDto> change);

    Task ProductChangedAsync(ChangeEvent<ProductDto> change);
}

// No CancellationToken on purpose: the write is already committed, so the event
// must go out even if the request that caused it is cancelled.
public sealed class ChangePublisher(
    IHubContext<DashboardHub, IDashboardClient> hub,
    ILogger<ChangePublisher> logger) : IChangePublisher
{
    public Task CustomerChangedAsync(ChangeEvent<CustomerDto> change) =>
        SendAsync(Topics.Customers, change.Type, change.Id, () =>
            hub.Clients.Group(Topics.Customers).CustomerChanged(change));

    // A client subscribed to both topics can get the event twice; it drops the
    // second one because the version is no newer than what it already applied
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
            // The write succeeded, so don't fail the request. Clients catch up on
            // their next reload or reconnect.
            logger.LogError(ex, "Failed to publish {ChangeType} for {Id} to {Topic}", type, id, topic);
        }
    }
}
