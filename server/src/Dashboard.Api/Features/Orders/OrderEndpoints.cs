using Dashboard.Api.Common;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Dashboard.Api.Features.Orders;

public static class OrderEndpoints
{
    private const string EntityName = "order";

    public static RouteGroupBuilder MapOrderEndpoints(this RouteGroupBuilder api)
    {
        // Per customer, like the customer:{id}:orders topic
        var customerOrders = api.MapGroup("/customers/{customerId:int}/orders").WithTags("Orders");

        // The same list SubscribeCustomerOrders returns
        customerOrders.MapGet("/", async Task<Results<Ok<IReadOnlyList<OrderDto>>, NotFound>> (
            int customerId, OrderService service, CancellationToken ct) =>
            await service.GetAllForCustomerAsync(customerId, ct) is { } orders
                ? TypedResults.Ok(orders)
                : TypedResults.NotFound());

        customerOrders.MapPost("/", async Task<Results<Created<OrderDto>, NotFound, ValidationProblem>> (
            int customerId, CreateOrderRequest request, OrderService service, CancellationToken ct) =>
        {
            var errors = request.Validate();
            if (errors.Count > 0) return TypedResults.ValidationProblem(errors);

            var result = await service.CreateAsync(customerId, request, ct);
            return result.Status switch
            {
                WriteStatus.Ok => TypedResults.Created($"/api/orders/{result.Value!.OrderID}", result.Value),
                WriteStatus.Invalid => TypedResults.ValidationProblem(result.Errors!),
                _ => TypedResults.NotFound(),
            };
        });

        var orders = api.MapGroup("/orders").WithTags("Orders");

        // The same list SubscribeOrders returns
        orders.MapGet("/", async Task<Ok<IReadOnlyList<OrderDto>>> (OrderService service, CancellationToken ct) =>
            TypedResults.Ok(await service.GetAllAsync(ct)));

        orders.MapGet("/{orderId:int}", async Task<Results<Ok<OrderDto>, NotFound>> (
            int orderId, OrderService service, CancellationToken ct) =>
            await service.GetAsync(orderId, ct) is { } order
                ? TypedResults.Ok(order)
                : TypedResults.NotFound());

        orders.MapPut("/{orderId:int}",
            async Task<Results<Ok<OrderDto>, NotFound, ProblemHttpResult, ValidationProblem>> (
                int orderId, UpdateOrderRequest request, OrderService service, CancellationToken ct) =>
        {
            var errors = request.Validate();
            if (errors.Count > 0) return TypedResults.ValidationProblem(errors);

            var result = await service.UpdateAsync(orderId, request, ct);
            return result.Status switch
            {
                WriteStatus.Ok => TypedResults.Ok(result.Value!),
                WriteStatus.NotFound => TypedResults.NotFound(),
                WriteStatus.Invalid => TypedResults.ValidationProblem(result.Errors!),
                _ => ProblemResults.Conflict(EntityName),
            };
        });

        // Optional ?version=: deletes only if it still matches
        orders.MapDelete("/{orderId:int}", async Task<Results<NoContent, NotFound, ProblemHttpResult>> (
            int orderId, long? version, OrderService service, CancellationToken ct) =>
            await service.DeleteAsync(orderId, version, ct) switch
            {
                WriteStatus.Ok => TypedResults.NoContent(),
                WriteStatus.NotFound => TypedResults.NotFound(),
                _ => ProblemResults.Conflict(EntityName),
            });

        return api;
    }
}
