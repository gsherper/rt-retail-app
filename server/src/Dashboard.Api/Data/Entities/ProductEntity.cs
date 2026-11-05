namespace Dashboard.Api.Data.Entities;

public sealed class ProductEntity
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public decimal Price { get; set; }

    /// <summary>Row version; set at startup and changed on every write.</summary>
    public long Version { get; set; }
}
