using Microsoft.AspNetCore.Http.HttpResults;

namespace Dashboard.Api.Common;

/// <summary>REST 409s; <c>HubError</c> reuses the details.</summary>
public static class ProblemResults
{
    public static string ConflictDetail(string entityName) =>
        $"This {entityName} was changed by someone else. Reload it and try again.";

    public static ProblemHttpResult Conflict(string entityName) =>
        TypedResults.Problem(
            statusCode: StatusCodes.Status409Conflict,
            title: "Conflict",
            detail: ConflictDetail(entityName));

    public static string InUseDetail(string entityName) =>
        $"This {entityName} is used by existing orders, so it can't be deleted.";

    public static ProblemHttpResult InUse(string entityName) =>
        TypedResults.Problem(
            statusCode: StatusCodes.Status409Conflict,
            title: "In use",
            detail: InUseDetail(entityName));
}
