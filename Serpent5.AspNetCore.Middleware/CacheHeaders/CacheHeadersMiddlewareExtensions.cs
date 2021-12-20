using Microsoft.AspNetCore.Builder;

namespace Serpent5.AspNetCore.Middleware.CacheHeaders;

public static class CacheHeadersMiddlewareExtensions
{
    public static IApplicationBuilder UseCacheHeaders(this IApplicationBuilder applicationBuilder)
        => applicationBuilder.UseMiddleware<CacheHeadersMiddleware>();
}
