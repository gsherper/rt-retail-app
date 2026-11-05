using Dashboard.Api.Common;
using Dashboard.Api.Data;
using Dashboard.Api.Data.Entities;
using Dashboard.Api.Features.Orders;
using Dashboard.Api.Realtime;

namespace Dashboard.Api.Features.Products;

public sealed class ProductService(
    InMemoryDataStore store,
    IChangePublisher publisher,
    ILogger<ProductService> logger)
{
    public Task<IReadOnlyList<ProductDto>> GetAllAsync(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        lock (store.Sync)
        {
            IReadOnlyList<ProductDto> items = store.Products.Values.OrderBy(p => p.Id).Select(ToDto).ToList();
            return Task.FromResult(items);
        }
    }

    public Task<ProductDto?> GetAsync(int productId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        lock (store.Sync)
        {
            return Task.FromResult(
                store.Products.TryGetValue(productId, out var entity) ? ToDto(entity) : null);
        }
    }

    public async Task<ProductDto> CreateAsync(CreateProductRequest request, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ProductDto created;
        lock (store.Sync)
        {
            var entity = new ProductEntity
            {
                Id = store.NextProductId(),
                Name = request.Name!.Trim(),
                Price = request.Price!.Value,
                Version = store.NextVersion(),
            };
            store.Products.Add(entity.Id, entity);
            created = ToDto(entity);
        }

        logger.LogInformation("Created product {ProductID}", created.ProductID);
        await publisher.ProductChangedAsync(
            ChangeEvent<ProductDto>.Created(created.ProductID, created.Version, created));
        return created;
    }

    public async Task<WriteResult<ProductDto>> UpdateAsync(
        int productId, UpdateProductRequest request, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ProductDto updated;
        List<OrderDto> ordersShowingProduct;
        lock (store.Sync)
        {
            if (!store.Products.TryGetValue(productId, out var entity))
                return WriteResult<ProductDto>.NotFound;
            if (entity.Version != request.Version)
                return WriteResult<ProductDto>.Conflict;

            entity.Name = request.Name!.Trim();
            entity.Price = request.Price!.Value;
            entity.Version = store.NextVersion();
            updated = ToDto(entity);
            ordersShowingProduct = OrderView.BuildWhere(store, o => o.ProductId == productId);
        }

        logger.LogInformation("Updated product {ProductID} to version {Version}", productId, updated.Version);
        await publisher.ProductChangedAsync(
            ChangeEvent<ProductDto>.Updated(productId, updated.Version, updated));
        // Order rows show its name and a total from its price
        await publisher.OrdersUpdatedAsync(ordersShowingProduct);
        return WriteResult<ProductDto>.Ok(updated);
    }

    /// <summary>InUse while any order uses this product. Conflict when expectedVersion is given and doesn't match.</summary>
    public async Task<WriteStatus> DeleteAsync(int productId, long? expectedVersion, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        long version;
        lock (store.Sync)
        {
            if (!store.Products.TryGetValue(productId, out var entity))
                return WriteStatus.NotFound;
            if (expectedVersion is not null && entity.Version != expectedVersion)
                return WriteStatus.Conflict;
            if (store.Orders.Values.Any(o => o.ProductId == productId))
                return WriteStatus.InUse;

            store.Products.Remove(productId);
            version = store.NextVersion();
        }

        logger.LogInformation("Deleted product {ProductID}", productId);
        await publisher.ProductChangedAsync(ChangeEvent<ProductDto>.Deleted(productId, version));
        return WriteStatus.Ok;
    }

    private static ProductDto ToDto(ProductEntity entity) =>
        new(entity.Id, entity.Name, entity.Price, entity.Version);
}
