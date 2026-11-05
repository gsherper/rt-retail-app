using Dashboard.Api.Features.Orders;

namespace Dashboard.Api.Realtime;

public sealed partial class DashboardHub
{
    private const string Order = "order";

    public Task<HubResult<IReadOnlyList<OrderDto>>> SubscribeOrders() =>
        SubscribeAsync(Topics.Orders, _orders.GetAllAsync);

    public Task UnsubscribeOrders() => UnsubscribeAsync(Topics.Orders);

    public async Task<HubResult<OrderDto>> GetOrder(int orderId) =>
        Found(await _orders.GetAsync(orderId, Aborted), Order);

    public async Task<HubResult<IReadOnlyList<OrderDto>>> SubscribeCustomerOrders(int customerId)
    {
        if (InvalidId("customerID", "Customer ID", customerId) is { } invalid)
            return HubResult<IReadOnlyList<OrderDto>>.Failure(invalid);

        var topic = Topics.CustomerOrders(customerId);
        await Groups.AddToGroupAsync(Context.ConnectionId, topic, Aborted);

        if (await _orders.GetAllForCustomerAsync(customerId, Aborted) is { } list)
            return HubResult<IReadOnlyList<OrderDto>>.Success(list);

        // Unknown customer: don't leave the caller in a group nothing will publish to
        await UnsubscribeAsync(topic);
        return HubResult<IReadOnlyList<OrderDto>>.Failure(HubError.NotFound(Customer));
    }

    public async Task UnsubscribeCustomerOrders(int customerId)
    {
        if (InvalidId("customerID", "Customer ID", customerId) is not null) return;
        await UnsubscribeAsync(Topics.CustomerOrders(customerId));
    }

    /// <summary>404 if the customer doesn't exist; 400 if the sales person or product doesn't.</summary>
    public async Task<HubResult<OrderDto>> CreateOrder(int customerId, CreateOrderRequest request) =>
        Invalid<OrderDto>(request.Validate())
            ?? HubResult<OrderDto>.From(
                await _orders.CreateAsync(customerId, request, Aborted), Order, notFoundEntity: Customer);

    public async Task<HubResult<OrderDto>> UpdateOrder(int orderId, UpdateOrderRequest request) =>
        Invalid<OrderDto>(request.Validate())
            ?? HubResult<OrderDto>.From(await _orders.UpdateAsync(orderId, request, Aborted), Order);

    public async Task<HubResult> DeleteOrder(int orderId, long? version) =>
        HubResult.From(await _orders.DeleteAsync(orderId, version, Aborted), Order);
}
