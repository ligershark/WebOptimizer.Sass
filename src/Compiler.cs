using DartSassHost;
using JavaScriptEngineSwitcher.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using WebOptimizer.Utils;

namespace WebOptimizer.Sass;

/// <summary>
/// Compiles Sass files
/// </summary>
/// <seealso cref="IProcessor"/>
/// <remarks>Gets the custom key that should be used when calculating the memory cache key.</remarks>
public partial class Compiler(IAsset asset, WebOptimizerScssOptions? options = null) : IProcessor
{
    private static readonly Regex ImportRegex = CreateImportRegex();

    private readonly List<string> _addedImports = [];
    private readonly WebOptimizerScssOptions options = options ?? new WebOptimizerScssOptions();

    private FileVersionProvider? _fileVersionProvider;

    /// <summary>
    /// Gets the custom key that should be used when calculating the memory cache key.
    /// </summary>
    public string CacheKey(HttpContext context, IAssetContext config)
    {
        return GenerateCacheKey(context, config);
    }

    /// <summary>
    /// Executes the processor on the specified configuration.
    /// </summary>
    public async Task ExecuteAsync(IAssetContext context)
    {
        await Task.Run(
            () =>
            {
                Dictionary<string, byte[]> content = [];
                IServiceProvider serviceProvider = context.HttpContext.RequestServices;
                IWebHostEnvironment env = (IWebHostEnvironment)serviceProvider.GetRequiredService(typeof(IWebHostEnvironment));
                IJsEngineSwitcher engineSwitcher = (IJsEngineSwitcher)serviceProvider.GetRequiredService(typeof(IJsEngineSwitcher));

                IFileProvider fileProvider = context.Asset.GetFileProvider(env);

                CompilationOptions settings = new();
                if (options is not null)
                {
                    settings.OutputStyle = options.OutputStyle;
                    settings.IncludePaths = [.. settings.IncludePaths, .. options.IncludePaths];
                    settings.SourceMap = options.GenerateSourceMap;
                    settings.IndentType = options.Indent.Contains('\t') ? IndentType.Tab : IndentType.Space;
                    settings.IndentWidth = options.Indent.Length;
                    settings.LineFeedType = options.Linefeed switch
                    {
                        "\n" => LineFeedType.Lf,
                        "\r" => LineFeedType.Cr,
                        "\r\n" => LineFeedType.CrLf,
                        "\n\r" => LineFeedType.LfCr,
                        _ => throw new NotSupportedException()
                    };
                    settings.OmitSourceMapUrl = options.OmitSourceMapUrl;
                    settings.SourceMapIncludeContents = options.SourceMapContents;
                    settings.InlineSourceMap = options.SourceMapEmbed;
                    settings.SourceMapRootPath = options.SourceMapRoot;
                }

                IFileManager fileManager = new FileProviderFileManager(fileProvider);

                using SassCompiler sassCompiler = new(engineSwitcher.CreateDefaultEngine, fileManager);
                foreach (string route in context.Content.Keys)
                {
                    CompilationResult result = sassCompiler.Compile(
                        context.Content[route].AsString(),
                        UrlPathUtils.MakeAbsolute("/", route),
                        null,
                        null,
                        settings);
                    content[route] = result.CompiledContent.AsByteArray();
                }

                context.Content = content;
            });
    }

    [GeneratedRegex("^@(?:import|use) ['\"]([^\"']+)['\"];$")]
    private static partial Regex CreateImportRegex();

    private void AppendImportedSassFiles(IFileProvider fileProvider, StringBuilder cacheKey, string basePath, string route)
    {
        // Add extension if missing
        if (!Path.HasExtension(route))
        {
            route = $"{route}.scss";
        }

        string filePath = UrlPathUtils.MakeAbsolute(basePath, route);
        IFileInfo file = fileProvider.GetFileInfo(filePath);

        // Add underscore at the start if missing
        if (!file.Exists)
        {
            string[] pathParts = route.Split('/');

            pathParts[^1] = $"_{pathParts[^1]}";

            string finalRoute = string.Join("/", pathParts);

            filePath = UrlPathUtils.MakeAbsolute(basePath, finalRoute);
            file = fileProvider.GetFileInfo(filePath);
            if (!file.Exists)
            {
                return;
            }
        }

        // Don't add same file twice
        if (_addedImports.Contains(filePath))
        {
            return;
        }

        // Add file in cache key
        _addedImports.Add(filePath);

        // Add sub files
        using Stream stream = file.CreateReadStream();
        using StreamReader reader = new(stream);
        for (string? line = reader.ReadLine(); line != null; line = reader.ReadLine())
        {
            Match match = ImportRegex.Match(line.Trim());
            if (match.Success)
            {
                for (int i = 1; i < match.Groups.Count; i++)
                {
                    string subRoute = match.Groups[i].Value;
                    if (!string.IsNullOrEmpty(subRoute) && !Uri.TryCreate(subRoute, UriKind.Absolute, out _))
                    {
                        AppendImportedSassFiles(fileProvider, cacheKey, UrlPathUtils.GetDirectory(filePath), subRoute);
                    }
                }
            }
        }
    }

    private string GenerateCacheKey(HttpContext context, IAssetContext config)
    {
        StringBuilder cacheKey = new();
        IWebHostEnvironment? env = (IWebHostEnvironment?)context.RequestServices.GetService(typeof(IWebHostEnvironment));
        IFileProvider fileProvider = asset.GetFileProvider(env);
        if (_fileVersionProvider is null)
        {
            IMemoryCache? cache = (IMemoryCache?)context.RequestServices.GetService(typeof(IMemoryCache));

            _fileVersionProvider = new FileVersionProvider(
                fileProvider,
                cache,
                context.Request.PathBase);
        }

        IEnumerable<string> routes = config.Asset.SourceFiles.Where(f => f.EndsWith(".scss"));
        if (config.Asset.Items?.ContainsKey("PhysicalFiles") ?? false)
        {
            routes = (IEnumerable<string>)config.Asset.Items["PhysicalFiles"];
        }

        foreach (string? route in routes.Select(x => UrlPathUtils.MakeAbsolute("/", x)))
        {
            IFileInfo file = fileProvider.GetFileInfo(route);
            string basePath = UrlPathUtils.GetDirectory(route);
            using Stream stream = file.CreateReadStream();
            using StreamReader reader = new(stream);
            for (string? line = reader.ReadLine(); line != null; line = reader.ReadLine())
            {
                Match match = ImportRegex.Match(line.Trim());
                if (match.Success)
                {
                    for (int i = 1; i < match.Groups.Count; i++)
                    {
                        string subRoute = match.Groups[i].Value;
                        if (!string.IsNullOrEmpty(subRoute) && !Uri.TryCreate(subRoute, UriKind.Absolute, out _))
                        {
                            AppendImportedSassFiles(fileProvider, cacheKey, basePath, subRoute);
                        }
                    }
                }
            }
        }

        for (int i = 0; i < _addedImports.Count; i++)
        {
            _ = cacheKey.Append(_fileVersionProvider.AddFileVersionToPath(_addedImports[i]));
        }

        byte[] buffer = Encoding.UTF8.GetBytes(cacheKey.ToString());
        byte[] hash = SHA256.HashData(buffer);
        return WebEncoders.Base64UrlEncode(hash);
    }
}
