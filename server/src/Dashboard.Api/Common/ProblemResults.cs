using Microsoft.AspNetCore.Http.HttpResults;

namespace Dashboard.Api.Common;

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

public static class EndpointExtensions
{
    /// <summary>
    /// Marks responses as never cacheable. Data here changes live, so browsers and
    /// proxies must not reuse an earlier response.
    /// </summary>
    public static TBuilder DisableHttpCaching<TBuilder>(this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder
    {
        builder.AddEndpointFilter(async (context, next) =>
        {
            context.HttpContext.Response.Headers.CacheControl = "no-store";
            return await next(context);
        });
        return builder;
    }
}
