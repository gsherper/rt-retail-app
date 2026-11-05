using Dashboard.Api.Common;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Dashboard.Api.Features.Customers;

public static class CustomerEndpoints
{
    private const string EntityName = "customer";

    public static RouteGroupBuilder MapCustomerEndpoints(this RouteGroupBuilder api)
    {
        var customers = api.MapGroup("/customers").WithTags("Customers");

        // The same list SubscribeCustomers returns
        customers.MapGet("/", async Task<Ok<IReadOnlyList<CustomerDto>>> (CustomerService service, CancellationToken ct) =>
            TypedResults.Ok(await service.GetAllAsync(ct)));

        customers.MapGet("/{customerId:int}", async Task<Results<Ok<CustomerDto>, NotFound>> (
            int customerId, CustomerService service, CancellationToken ct) =>
            await service.GetAsync(customerId, ct) is { } customer
                ? TypedResults.Ok(customer)
                : TypedResults.NotFound());

        customers.MapPost("/", async Task<Results<Created<CustomerDto>, ValidationProblem>> (
            CreateCustomerRequest request, CustomerService service, CancellationToken ct) =>
        {
            var errors = request.Validate();
            if (errors.Count > 0) return TypedResults.ValidationProblem(errors);

            var customer = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/customers/{customer.CustomerID}", customer);
        });

        customers.MapPut("/{customerId:int}",
            async Task<Results<Ok<CustomerDto>, NotFound, ProblemHttpResult, ValidationProblem>> (
                int customerId, UpdateCustomerRequest request, CustomerService service, CancellationToken ct) =>
        {
            var errors = request.Validate();
            if (errors.Count > 0) return TypedResults.ValidationProblem(errors);

            var result = await service.UpdateAsync(customerId, request, ct);
            return result.Status switch
            {
                WriteStatus.Ok => TypedResults.Ok(result.Value!),
                WriteStatus.NotFound => TypedResults.NotFound(),
                _ => ProblemResults.Conflict(EntityName),
            };
        });

        // ?version= is optional; when sent, the delete only happens if it still matches
        customers.MapDelete("/{customerId:int}", async Task<Results<NoContent, NotFound, ProblemHttpResult>> (
            int customerId, long? version, CustomerService service, CancellationToken ct) =>
            await service.DeleteAsync(customerId, version, ct) switch
            {
                WriteStatus.Ok => TypedResults.NoContent(),
                WriteStatus.NotFound => TypedResults.NotFound(),
                _ => ProblemResults.Conflict(EntityName),
            });

        // Demo helper: adds a customer with a made-up name, pushed like any other add
        customers.MapPost("/random", async Task<Created<CustomerDto>> (
            CustomerService service, CancellationToken ct) =>
        {
            var customer = await service.CreateAsync(RandomCustomer.Create(), ct);
            return TypedResults.Created($"/api/customers/{customer.CustomerID}", customer);
        });

        return api;
    }
}
