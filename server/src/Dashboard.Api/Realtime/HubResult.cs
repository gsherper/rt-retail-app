using Dashboard.Api.Common;

namespace Dashboard.Api.Realtime;

/// <summary>An expected hub failure, shaped like ProblemDetails.</summary>
public sealed record HubError(
    int Status,
    string Title,
    string? Detail = null,
    IDictionary<string, string[]>? Errors = null)
{
    public static HubError Validation(IDictionary<string, string[]> errors) =>
        new(StatusCodes.Status400BadRequest, "One or more validation errors occurred.", Errors: errors);

    public static HubError NotFound(string entityName) =>
        new(StatusCodes.Status404NotFound, "Not Found", $"The {entityName} was not found.");

    public static HubError Conflict(string entityName) =>
        new(StatusCodes.Status409Conflict, "Conflict", ProblemResults.ConflictDetail(entityName));

    public static HubError InUse(string entityName) =>
        new(StatusCodes.Status409Conflict, "In use", ProblemResults.InUseDetail(entityName));
}

public sealed record HubResult<T>(bool Ok, T? Data, HubError? Error)
{
    public static HubResult<T> Success(T data) => new(true, data, null);

    public static HubResult<T> Failure(HubError error) => new(false, default, error);

    /// <param name="notFoundEntity">The entity named in a 404, if not <paramref name="entityName"/>.</param>
    public static HubResult<T> From(WriteResult<T> result, string entityName, string? notFoundEntity = null) =>
        result.Status switch
        {
            WriteStatus.Ok => Success(result.Value!),
            WriteStatus.NotFound => Failure(HubError.NotFound(notFoundEntity ?? entityName)),
            WriteStatus.Conflict => Failure(HubError.Conflict(entityName)),
            WriteStatus.Invalid => Failure(HubError.Validation(result.Errors!)),
            WriteStatus.InUse => Failure(HubError.InUse(entityName)),
            _ => throw new ArgumentOutOfRangeException(nameof(result), result.Status, null),
        };
}

/// <summary>For hub methods with no data, such as deletes.</summary>
public sealed record HubResult(bool Ok, HubError? Error)
{
    public static readonly HubResult Success = new(true, null);

    public static HubResult Failure(HubError error) => new(false, error);

    public static HubResult From(WriteStatus status, string entityName) =>
        status switch
        {
            WriteStatus.Ok => Success,
            WriteStatus.NotFound => Failure(HubError.NotFound(entityName)),
            WriteStatus.Conflict => Failure(HubError.Conflict(entityName)),
            WriteStatus.InUse => Failure(HubError.InUse(entityName)),
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null),
        };
}
