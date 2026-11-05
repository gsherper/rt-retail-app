using Dashboard.Api.Features.Products;

namespace Dashboard.Api.Realtime;

public sealed partial class DashboardHub
{
    private const string Product = "product";

    public Task<HubResult<IReadOnlyList<ProductDto>>> SubscribeProducts() =>
        SubscribeAsync(Topics.Products, _products.GetAllAsync);

    public Task UnsubscribeProducts() => UnsubscribeAsync(Topics.Products);

    public async Task<HubResult<ProductDto>> GetProduct(int productId) =>
        Found(await _products.GetAsync(productId, Aborted), Product);

    public async Task<HubResult<ProductDto>> CreateProduct(CreateProductRequest request) =>
        Invalid<ProductDto>(request.Validate())
            ?? HubResult<ProductDto>.Success(await _products.CreateAsync(request, Aborted));

    public async Task<HubResult<ProductDto>> UpdateProduct(int productId, UpdateProductRequest request) =>
        Invalid<ProductDto>(request.Validate())
            ?? HubResult<ProductDto>.From(await _products.UpdateAsync(productId, request, Aborted), Product);

    /// <summary>409 "In use" while any order is for this product.</summary>
    public async Task<HubResult> DeleteProduct(int productId, long? version) =>
        HubResult.From(await _products.DeleteAsync(productId, version, Aborted), Product);
}
