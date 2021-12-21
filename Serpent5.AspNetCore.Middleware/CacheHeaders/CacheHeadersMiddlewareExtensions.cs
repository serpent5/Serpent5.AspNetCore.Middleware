using Serpent5.AspNetCore.Middleware.CacheHeaders;

namespace Microsoft.AspNetCore.Builder;

public static class CacheHeadersMiddlewareExtensions
{
    public static IApplicationBuilder UseCacheHeaders(this IApplicationBuilder applicationBuilder)
        => applicationBuilder.UseMiddleware<CacheHeadersMiddleware>();
}
