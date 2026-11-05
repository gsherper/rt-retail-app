using Dashboard.Api.Common;
using Dashboard.Api.Data;
using Dashboard.Api.Data.Entities;
using Dashboard.Api.Realtime;

namespace Dashboard.Api.Features.Orders;

public sealed class OrderService(
    InMemoryDataStore store,
    IChangePublisher publisher,
    ILogger<OrderService> logger)
{
    public Task<IReadOnlyList<OrderDto>> GetAllAsync(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        lock (store.Sync)
        {
            IReadOnlyList<OrderDto> items = store.Orders.Values.OrderBy(o => o.Id).Select(ToDto).ToList();
            return Task.FromResult(items);
        }
    }

    /// <summary>Null when the customer doesn't exist.</summary>
    public Task<IReadOnlyList<OrderDto>?> GetAllForCustomerAsync(int customerId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        lock (store.Sync)
        {
            if (!store.Customers.ContainsKey(customerId))
                return Task.FromResult<IReadOnlyList<OrderDto>?>(null);

            IReadOnlyList<OrderDto> items = store.Orders.Values
                .Where(o => o.CustomerId == customerId)
                .OrderBy(o => o.Id)
                .Select(ToDto)
                .ToList();
            return Task.FromResult<IReadOnlyList<OrderDto>?>(items);
        }
    }

    public Task<OrderDto?> GetAsync(int orderId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        lock (store.Sync)
        {
            return Task.FromResult(store.Orders.TryGetValue(orderId, out var entity) ? ToDto(entity) : null);
        }
    }

    /// <summary>NotFound: no such customer. Invalid: no such sales person or product.</summary>
    public async Task<WriteResult<OrderDto>> CreateAsync(
        int customerId, CreateOrderRequest request, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        OrderDto created;
        lock (store.Sync)
        {
            if (!store.Customers.ContainsKey(customerId))
                return WriteResult<OrderDto>.NotFound;
            if (MissingReferences(request.SalesPersonID!.Value, request.ProductID!.Value) is { } errors)
                return WriteResult<OrderDto>.Invalid(errors);

            var entity = new OrderEntity
            {
                Id = store.NextOrderId(),
                CustomerId = customerId,
                SalesPersonId = request.SalesPersonID.Value,
                ProductId = request.ProductID.Value,
                Quantity = request.Quantity!.Value,
                Version = store.NextVersion(),
            };
            store.Orders.Add(entity.Id, entity);
            created = ToDto(entity);
        }

        logger.LogInformation("Created order {OrderID} for customer {CustomerID}", created.OrderID, customerId);
        await publisher.OrderChangedAsync(
            customerId, ChangeEvent<OrderDto>.Created(created.OrderID, created.Version, created));
        return WriteResult<OrderDto>.Ok(created);
    }

    /// <summary>
    /// Invalid: no such sales person or product. Checks the row's version (see
    /// <see cref="OrderView"/>), so a change to anything the row shows is also a conflict.
    /// </summary>
    public async Task<WriteResult<OrderDto>> UpdateAsync(
        int orderId, UpdateOrderRequest request, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        OrderDto updated;
        lock (store.Sync)
        {
            if (!store.Orders.TryGetValue(orderId, out var entity))
                return WriteResult<OrderDto>.NotFound;
            if (ToDto(entity).Version != request.Version)
                return WriteResult<OrderDto>.Conflict;
            if (MissingReferences(request.SalesPersonID!.Value, request.ProductID!.Value) is { } errors)
                return WriteResult<OrderDto>.Invalid(errors);

            entity.SalesPersonId = request.SalesPersonID.Value;
            entity.ProductId = request.ProductID.Value;
            entity.Quantity = request.Quantity!.Value;
            entity.Version = store.NextVersion();
            updated = ToDto(entity);
        }

        logger.LogInformation("Updated order {OrderID} to version {Version}", orderId, updated.Version);
        await publisher.OrderChangedAsync(
            updated.Customer.CustomerID, ChangeEvent<OrderDto>.Updated(orderId, updated.Version, updated));
        return WriteResult<OrderDto>.Ok(updated);
    }

    /// <summary>Conflict when expectedVersion is given and isn't the row's version.</summary>
    public async Task<WriteStatus> DeleteAsync(int orderId, long? expectedVersion, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        int customerId;
        long version;
        lock (store.Sync)
        {
            if (!store.Orders.TryGetValue(orderId, out var entity))
                return WriteStatus.NotFound;
            if (expectedVersion is not null && ToDto(entity).Version != expectedVersion)
                return WriteStatus.Conflict;

            store.Orders.Remove(orderId);
            customerId = entity.CustomerId;
            version = store.NextVersion();
        }

        logger.LogInformation("Deleted order {OrderID} for customer {CustomerID}", orderId, customerId);
        await publisher.OrderChangedAsync(customerId, ChangeEvent<OrderDto>.Deleted(orderId, version));
        return WriteStatus.Ok;
    }

    // Foreign-key checks. Call while holding store.Sync.
    private Dictionary<string, string[]>? MissingReferences(int salesPersonId, int productId)
    {
        var errors = new ValidationErrors();
        if (!store.Employees.ContainsKey(salesPersonId))
            errors.Add("salesPersonID", $"Employee {salesPersonId} doesn't exist.");
        if (!store.Products.ContainsKey(productId))
            errors.Add("productID", $"Product {productId} doesn't exist.");
        return errors.HasErrors ? errors.ToDictionary() : null;
    }

    // Call while holding store.Sync.
    private OrderDto ToDto(OrderEntity entity) => OrderView.Build(store, entity);
}
