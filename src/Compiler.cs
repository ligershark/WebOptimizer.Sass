using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Memory;
using DartSassHost;
using JavaScriptEngineSwitcher.V8;

namespace WebOptimizer.Sass
{
    /// <summary>
    /// Compiles Sass files
    /// </summary>
    /// <seealso cref="WebOptimizer.IProcessor" />
    public class Compiler : IProcessor
    {
        private static Regex ImportRegex = new Regex("^@import ['\"]([^\"']+)['\"];$");

        /// <summary>
        /// Gets the custom key that should be used when calculating the memory cache key.
        /// </summary>
        public string CacheKey(HttpContext context, IAssetContext config) => GenerateCacheKey(context, config);

        private WebOptimazerScssOptions options;

        private IAsset _asset;

        private List<string> _addedImports;

        private FileVersionProvider _fileVersionProvider;

        /// <summary>
        /// Gets the custom key that should be used when calculating the memory cache key.
        /// </summary>
        public Compiler(IAsset asset, WebOptimazerScssOptions options = null)
        {
            _addedImports = new List<string>();
            _asset = asset;
            this.options = options;
        }


        /// <summary>
        /// Executes the processor on the specified configuration.
        /// </summary>
        public Task ExecuteAsync(IAssetContext context)
        {
            var content = new Dictionary<string, byte[]>();
            var env = (IWebHostEnvironment)context.HttpContext.RequestServices.GetService(typeof(IWebHostEnvironment));
            IFileProvider fileProvider = context.Asset.GetFileProvider(env);

            var settings = new CompilationOptions();
            if (options != null)
            {
                settings.OutputStyle = options.OutputStyle;
                settings.IncludePaths = settings.IncludePaths.Concat(options.IncludePaths).ToList();
                settings.SourceMap = options.GenerateSourceMap;
                settings.IndentType = options.Indent.Contains('\t') ? IndentType.Tab : IndentType.Space;
                settings.IndentWidth = options.Indent.Length;
                settings.LineFeedType = options.Linefeed switch { "\n" => LineFeedType.Lf, "\r" => LineFeedType.Cr, "\r\n" => LineFeedType.CrLf, "\n\r" => LineFeedType.LfCr, _ => throw new NotImplementedException() };
                settings.OmitSourceMapUrl = options.OmitSourceMapUrl;
                //settings.SourceComments = options.SourceComments;
                settings.SourceMapIncludeContents = options.SourceMapContents;
                settings.InlineSourceMap = options.SourceMapEmbed;
                settings.SourceMapRootPath = options.SourceMapRoot;
                //settings.TryImport = options.TryImport;
            }

            using (var sassCompiler = new SassCompiler(new V8JsEngineFactory(), settings))
            {
                foreach (string route in context.Content.Keys)
                {
                    IFileInfo file = fileProvider.GetFileInfo(route);
                    CompilationResult result;

                    if (file.Exists)
                    {
                        result = sassCompiler.CompileFile(file.PhysicalPath);
                    }
                    else
                    {
                        result = sassCompiler.Compile(context.Content[route].AsString(), options?.IsIndentedSyntaxSource ?? false);
                    }

                    content[route] = result.CompiledContent.AsByteArray();
                }
            }

            context.Content = content;

            return Task.CompletedTask;
        }

        private string GenerateCacheKey(HttpContext context, IAssetContext config)
        {
            var cacheKey = new StringBuilder();
            var env = (IWebHostEnvironment)context.RequestServices.GetService(typeof(IWebHostEnvironment));
            IFileProvider fileProvider = _asset.GetFileProvider(env);
            if (_fileVersionProvider == null)
            {
                var cache = (IMemoryCache)context.RequestServices.GetService(typeof(IMemoryCache));

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

            foreach (var route in routes.Select(f => f.TrimStart('/')))
            {
                IFileInfo file = fileProvider.GetFileInfo(route);
                var basePath = GetBasePath(route);
                using var stream = file.CreateReadStream();
                using var reader = new StreamReader(stream);
                for (var line = reader.ReadLine(); line != null; line = reader.ReadLine())
                {
                    var match = ImportRegex.Match(line.Trim());
                    if (match.Success)
                    {
                        for (int i = 1; i < match.Groups.Count; i++)
                        {
                            var subRoute = match.Groups[i].Value;
                            if (!string.IsNullOrEmpty(subRoute) && !Uri.TryCreate(subRoute, UriKind.Absolute, out _))
                            {
                                AppendImportedSassFiles(fileProvider, cacheKey, basePath, subRoute);
                            }
                        }
                    }
                }
            }
            
            for (int i = 0; i < _addedImports.Count; i++) {
                cacheKey.Append(_fileVersionProvider.AddFileVersionToPath(_addedImports[i]));
            }
            
            using var algo = SHA1.Create();
            byte[] buffer = Encoding.UTF8.GetBytes(cacheKey.ToString());
            byte[] hash = algo.ComputeHash(buffer);
            return WebEncoders.Base64UrlEncode(hash);
        }

        private void AppendImportedSassFiles(IFileProvider fileProvider, StringBuilder cacheKey, string basePath, string route)
        {
            // Add extension if missing
            if (!Path.HasExtension(route))
            {
                route = $"{route}.scss";
            }

            var filePath = PathCombine(basePath, route);
            IFileInfo file = fileProvider.GetFileInfo(filePath);

            // Add underscore at the start if missing
            if (!file.Exists)
            {
                var pathParts = route.Split('/');

                pathParts[pathParts.Length - 1] = $"_{pathParts[pathParts.Length - 1]}";

                var finalRoute = string.Join("/", pathParts);
                
                filePath = PathCombine(basePath, finalRoute);
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
            using var stream = file.CreateReadStream();
            using var reader = new StreamReader(stream);
            for (var line = reader.ReadLine(); line != null; line = reader.ReadLine())
            {
                var match = ImportRegex.Match(line.Trim());
                if (match.Success)
                {
                    for (int i = 1; i < match.Groups.Count; i++)
                    {
                        var subRoute = match.Groups[i].Value;
                        if (!string.IsNullOrEmpty(subRoute) && !Uri.TryCreate(subRoute, UriKind.Absolute, out _))
                        {
                            AppendImportedSassFiles(fileProvider, cacheKey, basePath, subRoute);
                        }
                    }
                }
            }
        }

        private static string PathCombine(params string[] args)
        {
            return Path.GetFullPath(Path.Combine(args))
                .Replace($"{Environment.CurrentDirectory}{Path.DirectorySeparatorChar}", string.Empty)
                .Replace("\\", "/");
        }

        private static string GetBasePath(string path)
        {
            return Path.GetDirectoryName(path)?.Replace("\\", "/") ?? string.Empty;
        }
    }
}
