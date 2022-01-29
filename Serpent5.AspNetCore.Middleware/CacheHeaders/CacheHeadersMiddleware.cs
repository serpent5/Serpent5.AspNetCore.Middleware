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
                var ctx = (HttpContext)ctxAsObject;
                var httpResponseHeaders = ctx.Response.GetTypedHeaders();

                // It's common to set "Cache-Control: no-cache, no-store".
                // According to MDN (https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Cache-Control):
                // - no-store means "Don't cache this, ever".
                // - no-cache means "Check with me before you use the cache".
                // Because this doesn't seem logical, let's clear the value and let the "no-store" fallback kick in.
                if (httpResponseHeaders.CacheControl is { NoCache: true, NoStore: true })
                    httpResponseHeaders.CacheControl = null;

                // Prefer ETag over Last-Modified.
                if (httpResponseHeaders.ETag is not null)
                    httpResponseHeaders.LastModified = null;

                // Set a default Cache-Control header
                if (httpResponseHeaders.ETag is not null || httpResponseHeaders.LastModified is not null)
                    // Resources with a version identifier should be checked first.
                    httpResponseHeaders.CacheControl ??= new() { NoCache = true };
                else
                    // All other resources aren't cacheable.
                    httpResponseHeaders.CacheControl ??= new() { NoStore = true };

                // The Cache-Control header supercedes Expires and Pragma.
                ctx.Response.Headers.Remove(HeaderNames.Expires);
                ctx.Response.Headers.Remove(HeaderNames.Pragma);

                return Task.CompletedTask;
            },
            ctx);

        return nextMiddleware(ctx);
    }
}
