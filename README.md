# A Scss compiler for ASP.NET Core

[![Build status](https://ci.appveyor.com/api/projects/status/i4uo3yef1gpyu00y?svg=true)](https://ci.appveyor.com/project/madskristensen/weboptimizer-sass)
[![NuGet](https://img.shields.io/nuget/v/LigerShark.WebOptimizer.Sass.svg)](https://nuget.org/packages/LigerShark.WebOptimizer.Sass/)

This package compiles Sass/Scss into CSS by hooking into the [LigerShark.WebOptimizer](https://github.com/ligershark/WebOptimizer) pipeline.

## Install
Add the NuGet package [LigerShark.WebOptimizer.Sass](https://nuget.org/packages/LigerShark.WebOptimizer.Sass/) to any ASP.NET Core project supporting .NET Standard 2.0 or higher.

> &gt; dotnet add package LigerShark.WebOptimizer.Sass

Since the [original library](https://www.npmjs.com/package/sass) is written in JavaScript, you will need a JS engine to run it.
As a JS engine is used the [JavaScript Engine Switcher](https://github.com/Taritsyn/JavaScriptEngineSwitcher) library.
First of all, you need to install the [JavaScriptEngineSwitcher.Extensions.MsDependencyInjection](https://www.nuget.org/packages/JavaScriptEngineSwitcher.Extensions.MsDependencyInjection) package which makes registration of JS engines more convenient.
Then you need to install one of the NuGet packages containing a JS engine provider:

 * [JavaScriptEngineSwitcher.ChakraCore](https://nuget.org/packages/JavaScriptEngineSwitcher.ChakraCore)
 * [JavaScriptEngineSwitcher.Jint](https://www.nuget.org/packages/JavaScriptEngineSwitcher.Jint)
 * [JavaScriptEngineSwitcher.Msie](https://nuget.org/packages/JavaScriptEngineSwitcher.Msie) (only in the Chakra “Edge” JsRT mode)
 * [JavaScriptEngineSwitcher.V8](https://nuget.org/packages/JavaScriptEngineSwitcher.V8)

After installing the packages, you will need to [register the default JS engine](https://github.com/Taritsyn/JavaScriptEngineSwitcher/wiki/Registration-of-JS-engines#aspnet-core).

## Versions
The `master` branch is being updated for currently supported .NET releases (currently, .NET 8.0 and .NET 9.0).
For ```ASP.NET Core 2.x```, use the **[`2.0` branch.](https://github.com/ligershark/WebOptimizer.Sass/tree/2.0)**

## Usage
Here's an example of how to compile `a.scss` and `b.scss` from inside the wwwroot folder and bundle them into a single .css file called `/all.css`:

In **Startup.cs**, modify the *ConfigureServices* method:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddMvc();

    // Add JavaScriptEngineSwitcher services to the services container.
    services.AddJsEngineSwitcher(options =>
    {
        options.AllowCurrentProperty = false;
        options.DefaultEngineName = V8JsEngine.EngineName;
    })
        .AddV8()
        ;

    services.AddWebOptimizer(pipeline =>
    {
        pipeline.AddScssBundle("/all.css", "a.scss", "b.scss");
    });
}
```
...and add `app.UseWebOptimizer()` to the `Configure` method anywhere before `app.UseStaticFiles`, like so:

```csharp
public void Configure(IApplicationBuilder app, IHostingEnvironment env)
{
    if (env.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }

    app.UseWebOptimizer();

    app.UseStaticFiles();
    app.UseMvc(routes =>
    {
        routes.MapRoute(
            name: "default",
            template: "{controller=Home}/{action=Index}/{id?}");
    });
}
```

Now the path *`http://domain/all.css`* will return a compiled, bundled and minified CSS document based on the two source files.

You can also reference any .scss files directly in the browser (*`http://domain/a.scss`*) and a compiled and minified CSS document will be served. To set that up, do this:

```csharp
services.AddWebOptimizer(pipeline =>
{
    pipeline.CompileScssFiles();
});
```

Or if you just want to limit what .scss files will be compiled, do this:

```csharp
services.AddWebOptimizer(pipeline =>
{
    pipeline.CompileScssFiles("/path/file1.scss", "/path/file2.scss");
});
```

## Setup TagHelpers
In `_ViewImports.cshtml` register the TagHelpers by adding `@addTagHelper *, WebOptimizer.Core` to the file. It may look something like this:
> [!NOTE]
> The TagHelpers are required in order for cache-busting version strings to be appended to the url. The ASP.NET LinkTagHelper attribute `asp-append-version="true"` is ignored.
```text
@addTagHelper *, WebOptimizer.Core
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
```
