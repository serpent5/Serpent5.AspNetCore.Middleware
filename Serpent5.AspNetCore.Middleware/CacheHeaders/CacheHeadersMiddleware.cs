using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace Serpent5.AspNetCore.Middleware.CacheHeaders;

public class CacheHeadersMiddleware
{
    private readonly RequestDelegate nextMiddleware;

    public CacheHeadersMiddleware(RequestDelegate nextMiddleware)
        => this.nextMiddleware = nextMiddleware;

    public Task InvokeAsync(HttpContext ctx)
    {
        ArgumentNullException.ThrowIfNull(ctx);

        ctx.Response.OnStarting(
            static ctxAsObject =>
            {
                var httpContext = (HttpContext)ctxAsObject;
                var httpResponseHeaders = httpContext.Response.GetTypedHeaders();

                // It's common to set "Cache-Control: no-cache, no-store".
                // According to MDN (https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Cache-Control):
                // - no-store means "Don't cache this, ever".
                // - no-cache means "Check with me before you use the cache".
                // Because this doesn't seem logical, let's clear the value and let the "no-store" fallback kick in.
                if (httpResponseHeaders.CacheControl is { NoCache: true, NoStore: true })
                    httpResponseHeaders.CacheControl = null;

                httpResponseHeaders.CacheControl ??= new()
                {
                    NoStore = true
                };

                // The "Cache-Control" header supercedes "Pragma: no-cache".
                httpContext.Response.Headers.Remove(HeaderNames.Pragma);

                return Task.CompletedTask;
            },
            ctx);

        return nextMiddleware(ctx);
    }
}
