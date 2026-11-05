namespace Dashboard.Api.Common;

public enum WriteStatus
{
    Ok,
    NotFound,
    /// <summary>The caller's version is out of date; someone else changed the row first.</summary>
    Conflict,
    /// <summary>Failed a check that needs current data, such as a reference to a row that doesn't exist.</summary>
    Invalid,
    /// <summary>Other rows still refer to this one, so it can't be deleted.</summary>
    InUse,
}

/// <summary>Outcome of a service write, so expected failures don't need exceptions.</summary>
public sealed record WriteResult<T>(
    WriteStatus Status,
    T? Value = default,
    Dictionary<string, string[]>? Errors = null)
{
    public static WriteResult<T> Ok(T value) => new(WriteStatus.Ok, value);

    public static readonly WriteResult<T> NotFound = new(WriteStatus.NotFound);

    public static readonly WriteResult<T> Conflict = new(WriteStatus.Conflict);

    public static WriteResult<T> Invalid(Dictionary<string, string[]> errors) =>
        new(WriteStatus.Invalid, Errors: errors);
}
