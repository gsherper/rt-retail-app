using Dashboard.Api.Common;
using Dashboard.Api.Features.Customers;
using Dashboard.Api.Features.Employees;
using Dashboard.Api.Features.Orders;
using Dashboard.Api.Features.Products;
using Microsoft.AspNetCore.SignalR;

namespace Dashboard.Api.Realtime;

/// <summary>
/// The client API. A thin adapter like the REST endpoints; each domain's methods are in
/// DashboardHub.&lt;Domain&gt;.cs.
/// </summary>
/// <remarks>
/// Groups don't survive a reconnect, so clients resubscribe. No auth yet; once there is,
/// check access in each method.
/// </remarks>
public sealed partial class DashboardHub(
    CustomerService customers,
    OrderService orders,
    EmployeeService employees,
    ProductService products) : Hub<IDashboardClient>
{
    public const string Path = "/hub";

    // Primary constructor parameters aren't visible in the other partial files
    private readonly CustomerService _customers = customers;
    private readonly OrderService _orders = orders;
    private readonly EmployeeService _employees = employees;
    private readonly ProductService _products = products;

    private CancellationToken Aborted => Context.ConnectionAborted;

    /// <summary>
    /// Joins before reading, so a change saved in between is never missed (a duplicate is
    /// dropped by version).
    /// </summary>
    private async Task<HubResult<IReadOnlyList<T>>> SubscribeAsync<T>(
        string topic, Func<CancellationToken, Task<IReadOnlyList<T>>> getAll)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, topic, Aborted);
        return HubResult<IReadOnlyList<T>>.Success(await getAll(Aborted));
    }

    private Task UnsubscribeAsync(string topic) =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, topic, Aborted);

    /// <summary>The row, or 404 when the service found nothing.</summary>
    private static HubResult<T> Found<T>(T? row, string entityName) where T : class =>
        row is null ? HubResult<T>.Failure(HubError.NotFound(entityName)) : HubResult<T>.Success(row);

    /// <summary>400 when there are errors, otherwise null.</summary>
    private static HubResult<T>? Invalid<T>(Dictionary<string, string[]> errors) =>
        errors.Count > 0 ? HubResult<T>.Failure(HubError.Validation(errors)) : null;

    private static HubError? InvalidId(string field, string label, int id)
    {
        var errors = new ValidationErrors();
        errors.RequiredId(field, label, id);
        return errors.HasErrors ? HubError.Validation(errors.ToDictionary()) : null;
    }
}
