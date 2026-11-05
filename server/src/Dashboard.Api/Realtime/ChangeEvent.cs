namespace Dashboard.Api.Realtime;

public static class ChangeTypes
{
    public const string Created = "created";
    public const string Updated = "updated";
    public const string Deleted = "deleted";
}

/// <summary>
/// A pushed change. Data is the resource's DTO, null for deletes. Clients use Version to
/// drop stale or duplicate events.
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
