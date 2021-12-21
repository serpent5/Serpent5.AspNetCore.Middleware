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

    [Fact]
    public async Task SetsCacheControl_NoStore()
    {
        var httpResponseMessage = await RunCacheHeadersMiddlewarePipeline();

        Assert.Equal("no-store", httpResponseMessage.Headers.CacheControl?.ToString());
    }

    [Fact]
    public async Task CacheControlSet_DoesNotSetCacheControl_NoStore()
    {
        var httpResponseMessage = await RunCacheHeadersMiddlewarePipeline(applicationBuilder =>
        {
            applicationBuilder.Use(async (ctx, nextMiddleware) =>
            {
                ctx.Response.GetTypedHeaders().CacheControl = anyCacheControlValueExceptNoCacheNoStore;
                await nextMiddleware(ctx);
            });
        });

        Assert.Equal(
            anyCacheControlValueExceptNoCacheNoStore.ToString(), httpResponseMessage.Headers.CacheControl?.ToString());
    }

    [Fact]
    public async Task CacheControlSet_NoCacheNoStore_SetsCacheControl_NoStore()
    {
        var httpResponseMessage = await RunCacheHeadersMiddlewarePipeline(applicationBuilder =>
        {
            applicationBuilder.Use(async (ctx, nextMiddleware) =>
            {
                ctx.Response.GetTypedHeaders().CacheControl = new()
                {
                    NoCache = true,
                    NoStore = true
                };

                await nextMiddleware(ctx);
            });
        });

        Assert.Equal("no-store", httpResponseMessage.Headers.CacheControl?.ToString());
    }

    [Fact]
    public async Task PragmaSet_RemovesPragma()
    {
        var httpResponseMessage = await RunCacheHeadersMiddlewarePipeline(applicationBuilder =>
        {
            applicationBuilder.Use(async (ctx, nextMiddleware) =>
            {
                ctx.Response.Headers.Pragma = "no-cache";
                await nextMiddleware(ctx);
            });
        });

        Assert.Empty(httpResponseMessage.Headers.Pragma);
    }

    private static async ValueTask<HttpResponseMessage> RunCacheHeadersMiddlewarePipeline(
        Action<IApplicationBuilder>? configureApplicationBuilder = null)
    {
        var testHost = await new HostBuilder()
            .ConfigureWebHostDefaults(webHostBuilder =>
            {
                webHostBuilder.UseTestServer()
                    .Configure(applicationBuilder =>
                    {
                        applicationBuilder.UseCacheHeaders();
                        configureApplicationBuilder?.Invoke(applicationBuilder);
                    });
            })
            .StartAsync();

        return await testHost.GetTestClient().GetAsync("/");
    }
}
