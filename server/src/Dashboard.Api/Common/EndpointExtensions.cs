namespace Dashboard.Api.Common;

public static class EndpointExtensions
{
    /// <summary><c>Cache-Control: no-store</c>, so responses are always current.</summary>
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
