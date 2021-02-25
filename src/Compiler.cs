using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using SharpScss;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebOptimizer.Sass
{
    /// <summary>
    /// Compiles Sass files
    /// </summary>
    /// <seealso cref="WebOptimizer.IProcessor" />
    public class Compiler : IProcessor
    {
        /// <summary>
        /// Gets the custom key that should be used when calculating the memory cache key.
        /// </summary>
        public string CacheKey(HttpContext context) => string.Empty;

        private WebOptimazerScssOptions options;

        /// <summary>
        /// Gets the custom key that should be used when calculating the memory cache key.
        /// </summary>
        public Compiler()
        {
            this.options = null;
        }

        /// <summary>
        /// Gets the custom key that should be used when calculating the memory cache key.
        /// </summary>
        public Compiler(WebOptimazerScssOptions options)
        {
           
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

            foreach (string route in context.Content.Keys)
            {
                IFileInfo file = fileProvider.GetFileInfo(route);
                var settings = new ScssOptions { InputFile = file.PhysicalPath };
                if(options!=null)
                {
                    settings.IncludePaths.AddRange(options.IncludePaths);
                    settings.GenerateSourceMap = options.GenerateSourceMap;
                    settings.Indent = options.Indent;
                    settings.IsIndentedSyntaxSource = options.IsIndentedSyntaxSource;
                    settings.Linefeed = options.Linefeed;
                    settings.OmitSourceMapUrl = options.OmitSourceMapUrl;
                    settings.SourceComments = options.SourceComments;
                    settings.SourceMapContents = options.SourceMapContents;
                    settings.SourceMapEmbed = options.SourceMapEmbed;
                    settings.SourceMapRoot = options.SourceMapRoot;
                    settings.TryImport = options.TryImport;
                }

                ScssResult result = Scss.ConvertToCss(context.Content[route].AsString(), settings);

                content[route] = result.Css.AsByteArray();
            }

            context.Content = content;

            return Task.CompletedTask;
        }
    }
}
