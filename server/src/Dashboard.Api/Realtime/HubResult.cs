using Dashboard.Api.Common;

namespace Dashboard.Api.Realtime;

/// <summary>
/// An expected failure from a hub method. Mirrors ProblemDetails, so the client
/// handles hub errors and REST errors the same way.
/// </summary>
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

/// <summary>Result of a hub method that returns data.</summary>
public sealed record HubResult<T>(bool Ok, T? Data, HubError? Error)
{
    public static HubResult<T> Success(T data) => new(true, data, null);

    public static HubResult<T> Failure(HubError error) => new(false, default, error);

    /// <param name="notFoundEntity">What to call the missing thing in a 404.</param>
    public static HubResult<T> From(WriteResult<T> result, string entityName, string? notFoundEntity = null) =>
        result.Status switch
        {
            WriteStatus.Ok => Success(result.Value!),
            WriteStatus.NotFound => Failure(HubError.NotFound(notFoundEntity ?? entityName)),
            WriteStatus.Invalid => Failure(HubError.Validation(result.Errors!)),
            WriteStatus.InUse => Failure(HubError.InUse(entityName)),
            _ => Failure(HubError.Conflict(entityName)),
        };
}

/// <summary>Result of a hub method with no data, such as a delete.</summary>
public sealed record HubResult(bool Ok, HubError? Error)
{
    public static readonly HubResult Success = new(true, null);

    public static HubResult Failure(HubError error) => new(false, error);

    public static HubResult From(WriteStatus status, string entityName) =>
        status switch
        {
            WriteStatus.Ok => Success,
            WriteStatus.NotFound => Failure(HubError.NotFound(entityName)),
            WriteStatus.InUse => Failure(HubError.InUse(entityName)),
            _ => Failure(HubError.Conflict(entityName)),
        };
}
