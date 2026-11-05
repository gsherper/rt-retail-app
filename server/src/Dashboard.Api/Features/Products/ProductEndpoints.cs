using Dashboard.Api.Common;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Dashboard.Api.Features.Products;

public static class ProductEndpoints
{
    private const string EntityName = "product";

    public static RouteGroupBuilder MapProductEndpoints(this RouteGroupBuilder api)
    {
        var products = api.MapGroup("/products").WithTags("Products");

        // The same list SubscribeProducts returns
        products.MapGet("/", async Task<Ok<IReadOnlyList<ProductDto>>> (ProductService service, CancellationToken ct) =>
            TypedResults.Ok(await service.GetAllAsync(ct)));

        products.MapGet("/{productId:int}", async Task<Results<Ok<ProductDto>, NotFound>> (
            int productId, ProductService service, CancellationToken ct) =>
            await service.GetAsync(productId, ct) is { } product
                ? TypedResults.Ok(product)
                : TypedResults.NotFound());

        products.MapPost("/", async Task<Results<Created<ProductDto>, ValidationProblem>> (
            CreateProductRequest request, ProductService service, CancellationToken ct) =>
        {
            var errors = request.Validate();
            if (errors.Count > 0) return TypedResults.ValidationProblem(errors);

            var product = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/products/{product.ProductID}", product);
        });

        products.MapPut("/{productId:int}",
            async Task<Results<Ok<ProductDto>, NotFound, ProblemHttpResult, ValidationProblem>> (
                int productId, UpdateProductRequest request, ProductService service, CancellationToken ct) =>
        {
            var errors = request.Validate();
            if (errors.Count > 0) return TypedResults.ValidationProblem(errors);

            var result = await service.UpdateAsync(productId, request, ct);
            return result.Status switch
            {
                WriteStatus.Ok => TypedResults.Ok(result.Value!),
                WriteStatus.NotFound => TypedResults.NotFound(),
                _ => ProblemResults.Conflict(EntityName),
            };
        });

        // Optional ?version=: deletes only if it still matches
        products.MapDelete("/{productId:int}", async Task<Results<NoContent, NotFound, ProblemHttpResult>> (
            int productId, long? version, ProductService service, CancellationToken ct) =>
            await service.DeleteAsync(productId, version, ct) switch
            {
                WriteStatus.Ok => TypedResults.NoContent(),
                WriteStatus.NotFound => TypedResults.NotFound(),
                WriteStatus.InUse => ProblemResults.InUse(EntityName),
                _ => ProblemResults.Conflict(EntityName),
            });

        return api;
    }
}
