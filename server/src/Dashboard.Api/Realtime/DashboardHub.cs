using Dashboard.Api.Common;
using Dashboard.Api.Features.Customers;
using Dashboard.Api.Features.Employees;
using Dashboard.Api.Features.Orders;
using Dashboard.Api.Features.Products;
using Microsoft.AspNetCore.SignalR;

namespace Dashboard.Api.Realtime;

/// <summary>
/// The client API: subscribing (which returns the initial list), unsubscribing,
/// and writes. A thin adapter like the REST endpoints: it validates, calls the
/// same services, and maps the outcome to <see cref="HubResult{T}"/>.
/// Group membership doesn't survive a reconnect, so clients subscribe again
/// after reconnecting; the server keeps no subscription state.
/// Methods for each domain are in DashboardHub.&lt;Domain&gt;.cs.
/// </summary>
public sealed partial class DashboardHub(
    CustomerService customers,
    OrderService orders,
    EmployeeService employees,
    ProductService products) : Hub<IDashboardClient>
{
    public const string Path = "/hub";

    // No authentication yet, so every caller may see and change everything.
    // Once there is, check access in each method before joining a group or
    // calling the service.

    // Subscribe methods join the group first and then read the list, so a change
    // committed in between is in the list, pushed to us, or both; never neither.
    // The client drops duplicates by version. Writes return the saved row, and
    // the caller gets it again as a pushed event.

    private readonly CustomerService _customers = customers;
    private readonly OrderService _orders = orders;
    private readonly EmployeeService _employees = employees;
    private readonly ProductService _products = products;

    private CancellationToken Aborted => Context.ConnectionAborted;

    /// <summary>Joins the topic, then returns its current list.</summary>
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

    private static HubResult<T>? Invalid<T>(Dictionary<string, string[]> errors) =>
        errors.Count > 0 ? HubResult<T>.Failure(HubError.Validation(errors)) : null;

    private static HubError? InvalidId(string field, string label, int id)
    {
        if (id > 0) return null;

        var errors = new ValidationErrors();
        errors.Add(field, $"{label} must be a positive number.");
        return HubError.Validation(errors.ToDictionary());
    }
}
