# Serpent5.AspNetCore.Middleware

[![Nuget](https://img.shields.io/nuget/v/Serpent5.AspNetCore.Middleware.svg)](https://www.nuget.org/packages/Serpent5.AspNetCore.Middleware)

A collection of middleware I've found useful for ASP.NET Core.

## `UseCacheHeaders`

- Sets a default `Cache-Control: no-store` response header.
- Sets `Cache-Control: no-cache` for responses that include `ETag`.
- Replaces `Cache-Control: no-cache,no-store` with `Cache-Control: no-store`.
- Removes `Expires` and `Pragma`.
- Removes `LastModified` in favour of `ETag`.
