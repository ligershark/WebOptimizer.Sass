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

        /// <summary>
        /// Executes the processor on the specified configuration.
        /// </summary>
        public Task ExecuteAsync(IAssetContext context)
        {
            var pipeline = (IAssetPipeline)context.HttpContext.RequestServices.GetService(typeof(IAssetPipeline));
            var content = new Dictionary<string, string>();

            foreach (string route in context.Content.Keys)
            {
                IFileInfo file = pipeline.FileProvider.GetFileInfo(route);
                var options = new ScssOptions { InputFile = file.PhysicalPath };

                ScssResult result = Scss.ConvertToCss(context.Content[route], options);

                content[route] = result.Css;
            }

            context.Content = content;

            return Task.CompletedTask;
        }
    }
}
