using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;
using Xunit;

namespace Serpent5.AspNetCore.Middleware.Tests.CacheHeaders;

public class CacheHeadersMiddlewareTests
{
    private readonly CacheControlHeaderValue anyCacheControlValueExceptNoCacheNoStore = new()
    {
        NoCache = true
    };

    private readonly DateTimeOffset anyDateTimeOffset = DateTimeOffset.Now;

    [Fact]
    public async Task LastModifiedAndETagSet_RemovesLastModified()
    {
        var httpResponseMessage = await RunCacheHeadersMiddlewarePipeline(ctx =>
        {
            var httpResponseHeaders = ctx.Response.GetTypedHeaders();

            httpResponseHeaders.ETag = EntityTagHeaderValue.Any;
            httpResponseHeaders.LastModified = anyDateTimeOffset;
        });

        Assert.Null(httpResponseMessage.Content.Headers.LastModified);
    }

    [Fact]
    public async Task SetsCacheControl_NoStore()
    {
        var httpResponseMessage = await RunCacheHeadersMiddlewarePipeline();

        Assert.Equal("no-store", httpResponseMessage.Headers.CacheControl?.ToString());
    }

    [Fact]
    public async Task CacheControlSet_DoesNotSetCacheControl_NoStore()
    {
        var httpResponseMessage = await RunCacheHeadersMiddlewarePipeline(ctx =>
        {
            ctx.Response.GetTypedHeaders().CacheControl = anyCacheControlValueExceptNoCacheNoStore;
        });

        Assert.Equal(
            anyCacheControlValueExceptNoCacheNoStore.ToString(), httpResponseMessage.Headers.CacheControl?.ToString());
    }

    [Fact]
    public async Task CacheControlSet_NoCacheNoStore_SetsCacheControl_NoStore()
    {
        var httpResponseMessage = await RunCacheHeadersMiddlewarePipeline(ctx =>
        {
            ctx.Response.GetTypedHeaders().CacheControl = new()
            {
                NoCache = true,
                NoStore = true
            };
        });

        Assert.Equal("no-store", httpResponseMessage.Headers.CacheControl?.ToString());
    }

    [Fact]
    public async Task ETagSet_SetsCacheControl_NoCache()
    {
        var httpResponseMessage = await RunCacheHeadersMiddlewarePipeline(ctx =>
        {
            ctx.Response.GetTypedHeaders().ETag = EntityTagHeaderValue.Any;
        });

        Assert.Equal("no-cache", httpResponseMessage.Headers.CacheControl?.ToString());
    }

    [Fact]
    public async Task ExpiresSet_RemovesExpires()
    {
        var httpResponseMessage = await RunCacheHeadersMiddlewarePipeline(ctx =>
        {
            ctx.Response.GetTypedHeaders().Expires = anyDateTimeOffset;
        });

        Assert.Null(httpResponseMessage.Content.Headers.Expires);
    }

    [Fact]
    public async Task PragmaSet_RemovesPragma()
    {
        var httpResponseMessage = await RunCacheHeadersMiddlewarePipeline(ctx =>
        {
            ctx.Response.Headers.Pragma = "no-cache";
        });

        Assert.Empty(httpResponseMessage.Headers.Pragma);
    }

    private static async ValueTask<HttpResponseMessage> RunCacheHeadersMiddlewarePipeline(
        Action<HttpContext>? configureHttpContext = null)
    {
        var testHost = await new HostBuilder()
            .ConfigureWebHostDefaults(webHostBuilder =>
            {
                webHostBuilder.UseTestServer()
                    .Configure(applicationBuilder =>
                    {
                        applicationBuilder.UseCacheHeaders();

                        if (configureHttpContext is not null)
                        {
                            applicationBuilder.Use(async (ctx, nextMiddleware) =>
                            {
                                configureHttpContext(ctx);
                                await nextMiddleware(ctx);
                            });
                        }
                    });
            })
            .StartAsync();

        return await testHost.GetTestClient().GetAsync("/");
    }
}
