namespace Dashboard.Api.Common;

public enum WriteStatus
{
    Ok,
    NotFound,
    /// <summary>The caller's version is stale.</summary>
    Conflict,
    /// <summary>Refers to a row that doesn't exist.</summary>
    Invalid,
    /// <summary>Other rows still refer to it.</summary>
    InUse,
}

/// <summary>A service write's outcome; expected failures don't throw.</summary>
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
