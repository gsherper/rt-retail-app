namespace Dashboard.Api.Data.Entities;

public sealed class OrderEntity
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public int SalesPersonId { get; set; }

    public int ProductId { get; set; }

    public int Quantity { get; set; }

    /// <summary>Row version; set at startup and changed on every write.</summary>
    public long Version { get; set; }
}
