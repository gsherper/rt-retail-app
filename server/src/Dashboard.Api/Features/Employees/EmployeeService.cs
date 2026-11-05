using Dashboard.Api.Common;
using Dashboard.Api.Data;
using Dashboard.Api.Data.Entities;
using Dashboard.Api.Features.Orders;
using Dashboard.Api.Realtime;

namespace Dashboard.Api.Features.Employees;

public sealed class EmployeeService(
    InMemoryDataStore store,
    IChangePublisher publisher,
    ILogger<EmployeeService> logger)
{
    /// <summary>Every employee, sorted by ID: the hub subscribe's initial list and the REST list.</summary>
    public Task<IReadOnlyList<EmployeeDto>> GetAllAsync(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        lock (store.Sync)
        {
            IReadOnlyList<EmployeeDto> items = store.Employees.Values.OrderBy(e => e.Id).Select(ToDto).ToList();
            return Task.FromResult(items);
        }
    }

    public Task<EmployeeDto?> GetAsync(int employeeId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        lock (store.Sync)
        {
            return Task.FromResult(
                store.Employees.TryGetValue(employeeId, out var entity) ? ToDto(entity) : null);
        }
    }

    public async Task<EmployeeDto> CreateAsync(CreateEmployeeRequest request, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        EmployeeDto created;
        lock (store.Sync)
        {
            var entity = new EmployeeEntity
            {
                Id = store.NextEmployeeId(),
                FirstName = request.FirstName!.Trim(),
                MiddleInitial = PersonNameRules.NormalizeInitial(request.MiddleInitial),
                LastName = request.LastName!.Trim(),
                Version = store.NextVersion(),
            };
            store.Employees.Add(entity.Id, entity);
            created = ToDto(entity);
        }

        logger.LogInformation("Created employee {EmployeeID}", created.EmployeeID);
        await publisher.EmployeeChangedAsync(
            ChangeEvent<EmployeeDto>.Created(created.EmployeeID, created.Version, created));
        return created;
    }

    public async Task<WriteResult<EmployeeDto>> UpdateAsync(
        int employeeId, UpdateEmployeeRequest request, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        EmployeeDto updated;
        List<OrderDto> ordersShowingName;
        lock (store.Sync)
        {
            if (!store.Employees.TryGetValue(employeeId, out var entity))
                return WriteResult<EmployeeDto>.NotFound;
            if (entity.Version != request.Version)
                return WriteResult<EmployeeDto>.Conflict;

            entity.FirstName = request.FirstName!.Trim();
            entity.MiddleInitial = PersonNameRules.NormalizeInitial(request.MiddleInitial);
            entity.LastName = request.LastName!.Trim();
            entity.Version = store.NextVersion();
            updated = ToDto(entity);
            ordersShowingName = OrderView.BuildWhere(store, o => o.SalesPersonId == employeeId);
        }

        logger.LogInformation("Updated employee {EmployeeID} to version {Version}", employeeId, updated.Version);
        await publisher.EmployeeChangedAsync(
            ChangeEvent<EmployeeDto>.Updated(employeeId, updated.Version, updated));
        await publisher.OrdersUpdatedAsync(ordersShowingName);
        return WriteResult<EmployeeDto>.Ok(updated);
    }

    /// <summary>
    /// In use when any order names this employee as its sales person. When
    /// expectedVersion is given and doesn't match, the result is a conflict.
    /// </summary>
    public async Task<WriteStatus> DeleteAsync(int employeeId, long? expectedVersion, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        long version;
        lock (store.Sync)
        {
            if (!store.Employees.TryGetValue(employeeId, out var entity))
                return WriteStatus.NotFound;
            if (expectedVersion is not null && entity.Version != expectedVersion)
                return WriteStatus.Conflict;
            if (store.Orders.Values.Any(o => o.SalesPersonId == employeeId))
                return WriteStatus.InUse;

            store.Employees.Remove(employeeId);
            version = store.NextVersion();
        }

        logger.LogInformation("Deleted employee {EmployeeID}", employeeId);
        await publisher.EmployeeChangedAsync(ChangeEvent<EmployeeDto>.Deleted(employeeId, version));
        return WriteStatus.Ok;
    }

    private static EmployeeDto ToDto(EmployeeEntity entity) =>
        new(entity.Id, entity.FirstName, entity.MiddleInitial, entity.LastName, entity.Version);
}
