namespace Dashboard.Api.Realtime;

public static class ChangeTypes
{
    public const string Created = "created";
    public const string Updated = "updated";
    public const string Deleted = "deleted";
}

/// <summary>
/// Envelope for every pushed change. Data is the same DTO the REST endpoint
/// returns for that resource, and null for deletes. Clients compare Version with
/// the row they have to drop stale or duplicate events.
/// </summary>
public sealed record ChangeEvent<T>(string Type, int Id, long Version, T? Data)
{
    public static ChangeEvent<T> Created(int id, long version, T data) =>
        new(ChangeTypes.Created, id, version, data);

    public static ChangeEvent<T> Updated(int id, long version, T data) =>
        new(ChangeTypes.Updated, id, version, data);

    public static ChangeEvent<T> Deleted(int id, long version) =>
        new(ChangeTypes.Deleted, id, version, default);
}
