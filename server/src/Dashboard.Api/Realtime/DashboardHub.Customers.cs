using Dashboard.Api.Features.Customers;

namespace Dashboard.Api.Realtime;

public sealed partial class DashboardHub
{
    private const string Customer = "customer";

    public Task<HubResult<IReadOnlyList<CustomerDto>>> SubscribeCustomers() =>
        SubscribeAsync(Topics.Customers, _customers.GetAllAsync);

    public Task UnsubscribeCustomers() => UnsubscribeAsync(Topics.Customers);

    public async Task<HubResult<CustomerDto>> GetCustomer(int customerId) =>
        Found(await _customers.GetAsync(customerId, Aborted), Customer);

    public async Task<HubResult<CustomerDto>> CreateCustomer(CreateCustomerRequest request) =>
        Invalid<CustomerDto>(request.Validate())
            ?? HubResult<CustomerDto>.Success(await _customers.CreateAsync(request, Aborted));

    public async Task<HubResult<CustomerDto>> UpdateCustomer(int customerId, UpdateCustomerRequest request) =>
        Invalid<CustomerDto>(request.Validate())
            ?? HubResult<CustomerDto>.From(await _customers.UpdateAsync(customerId, request, Aborted), Customer);

    /// <summary>Also deletes the customer's orders.</summary>
    public async Task<HubResult> DeleteCustomer(int customerId, long? version) =>
        HubResult.From(await _customers.DeleteAsync(customerId, version, Aborted), Customer);
}
