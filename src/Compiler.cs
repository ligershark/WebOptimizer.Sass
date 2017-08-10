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
            var content = new Dictionary<string, byte[]>();

            foreach (string route in context.Content.Keys)
            {
                IFileInfo file = context.Options.FileProvider.GetFileInfo(route);
                var settings = new ScssOptions { InputFile = file.PhysicalPath };

                ScssResult result = Scss.ConvertToCss(context.Content[route].AsString(), settings);

                content[route] = result.Css.AsByteArray();
            }

            context.Content = content;

            return Task.CompletedTask;
        }
    }
}
