using Dashboard.Api.Common;
using Dashboard.Api.Data;
using Dashboard.Api.Data.Entities;
using Dashboard.Api.Features.Orders;
using Dashboard.Api.Realtime;

namespace Dashboard.Api.Features.Customers;

public sealed class CustomerService(
    InMemoryDataStore store,
    IChangePublisher publisher,
    ILogger<CustomerService> logger)
{
    /// <summary>Every customer, sorted by ID: the hub subscribe's initial list and the REST list.</summary>
    public Task<IReadOnlyList<CustomerDto>> GetAllAsync(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        lock (store.Sync)
        {
            IReadOnlyList<CustomerDto> items = store.Customers.Values.OrderBy(c => c.Id).Select(ToDto).ToList();
            return Task.FromResult(items);
        }
    }

    public Task<CustomerDto?> GetAsync(int customerId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        lock (store.Sync)
        {
            return Task.FromResult(
                store.Customers.TryGetValue(customerId, out var entity) ? ToDto(entity) : null);
        }
    }

    public async Task<CustomerDto> CreateAsync(CreateCustomerRequest request, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        CustomerDto created;
        lock (store.Sync)
        {
            var entity = new CustomerEntity
            {
                Id = store.NextCustomerId(),
                FirstName = request.FirstName!.Trim(),
                MiddleInitial = PersonNameRules.NormalizeInitial(request.MiddleInitial),
                LastName = request.LastName!.Trim(),
                Version = store.NextVersion(),
            };
            store.Customers.Add(entity.Id, entity);
            created = ToDto(entity);
        }

        logger.LogInformation("Created customer {CustomerID}", created.CustomerID);
        await publisher.CustomerChangedAsync(
            ChangeEvent<CustomerDto>.Created(created.CustomerID, created.Version, created));
        return created;
    }

    public async Task<WriteResult<CustomerDto>> UpdateAsync(
        int customerId, UpdateCustomerRequest request, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        CustomerDto updated;
        List<OrderDto> ordersShowingName;
        lock (store.Sync)
        {
            if (!store.Customers.TryGetValue(customerId, out var entity))
                return WriteResult<CustomerDto>.NotFound;
            if (entity.Version != request.Version)
                return WriteResult<CustomerDto>.Conflict;

            entity.FirstName = request.FirstName!.Trim();
            entity.MiddleInitial = PersonNameRules.NormalizeInitial(request.MiddleInitial);
            entity.LastName = request.LastName!.Trim();
            entity.Version = store.NextVersion();
            updated = ToDto(entity);
            ordersShowingName = OrderView.BuildWhere(store, o => o.CustomerId == customerId);
        }

        logger.LogInformation("Updated customer {CustomerID} to version {Version}", customerId, updated.Version);
        await publisher.CustomerChangedAsync(
            ChangeEvent<CustomerDto>.Updated(customerId, updated.Version, updated));
        await publisher.OrdersUpdatedAsync(ordersShowingName);
        return WriteResult<CustomerDto>.Ok(updated);
    }

    /// <summary>
    /// Deletes the customer and their orders. When expectedVersion is given and
    /// doesn't match, nothing is deleted and the result is a conflict.
    /// </summary>
    public async Task<WriteStatus> DeleteAsync(int customerId, long? expectedVersion, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ChangeEvent<CustomerDto> customerDeleted;
        List<ChangeEvent<OrderDto>> ordersDeleted;
        lock (store.Sync)
        {
            if (!store.Customers.TryGetValue(customerId, out var entity))
                return WriteStatus.NotFound;
            if (expectedVersion is not null && entity.Version != expectedVersion)
                return WriteStatus.Conflict;

            var orderIds = store.Orders.Values
                .Where(o => o.CustomerId == customerId)
                .Select(o => o.Id)
                .ToList();
            ordersDeleted = [];
            foreach (var orderId in orderIds)
            {
                store.Orders.Remove(orderId);
                ordersDeleted.Add(ChangeEvent<OrderDto>.Deleted(orderId, store.NextVersion()));
            }

            store.Customers.Remove(customerId);
            customerDeleted = ChangeEvent<CustomerDto>.Deleted(customerId, store.NextVersion());
        }

        logger.LogInformation(
            "Deleted customer {CustomerID} and {OrderCount} orders", customerId, ordersDeleted.Count);
        foreach (var orderDeleted in ordersDeleted)
            await publisher.OrderChangedAsync(customerId, orderDeleted);
        await publisher.CustomerChangedAsync(customerDeleted);
        return WriteStatus.Ok;
    }

    private static CustomerDto ToDto(CustomerEntity entity) =>
        new(entity.Id, entity.FirstName, entity.MiddleInitial, entity.LastName, entity.Version);
}
