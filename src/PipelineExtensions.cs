using System.Collections.Generic;
using WebOptimizer;
using WebOptimizer.Sass;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extensions methods for registrating the Sass compiler on the Asset Pipeline.
    /// </summary>
    public static class PipelineExtensions
    {
        /// <summary>
        /// Compile Sass or Scss files on the asset pipeline.
        /// </summary>
        public static IAsset CompileScss(this IAsset asset, WebOptimazerScssOptions options = null)
        {
            asset.Processors.Add(new Compiler(options));
            return asset;
        }

        /// <summary>
        /// Compile Sass or Scss files on the asset pipeline.
        /// </summary>
        public static IEnumerable<IAsset> CompileScss(this IEnumerable<IAsset> assets, WebOptimazerScssOptions options = null)
        {
            var list = new List<IAsset>();

            foreach (IAsset asset in assets)
            {
                list.Add(asset.CompileScss(options));
            }

            return list;
        }

        /// <summary>
        /// Compile Sass or Scss files on the asset pipeline.
        /// </summary>
        /// <param name="pipeline">The asset pipeline.</param>
        /// <param name="route">The route where the compiled .css file will be available from.</param>
        /// <param name="options"></param>
        /// <param name="sourceFiles">The path to the .sass or .scss source files to compile.</param>
        public static IAsset AddScssBundle(this IAssetPipeline pipeline,WebOptimazerScssOptions options, string route, params string[] sourceFiles)
        {
            return pipeline.AddBundle(route, "text/css; charset=UTF-8", sourceFiles)
                           .CompileScss(options)
                           .AdjustRelativePaths()
                           .Concatenate()
                           .FingerprintUrls()
                           .AddResponseHeader("X-Content-Type-Options", "nosniff")
                           .MinifyCss();
        }

        public static IAsset AddScssBundle(this IAssetPipeline pipeline, string route, params string[] sourceFiles)
            => AddScssBundle(pipeline, null, route, sourceFiles);

        /// <summary>
        /// Compiles .scss files into CSS and makes them servable in the browser.
        /// </summary>
        /// <param name="pipeline">The asset pipeline.</param>
        public static IEnumerable<IAsset> CompileScssFiles(this IAssetPipeline pipeline, WebOptimazerScssOptions options = null)
        {
            return pipeline.AddFiles("text/css; charset=UTF-8", "**/*.scss")
                           .CompileScss(options)
                           .FingerprintUrls()
                           .AddResponseHeader("X-Content-Type-Options", "nosniff")
                           .MinifyCss();
        }

        /// <summary>
        /// Compiles the specified .scss files into CSS and makes them servable in the browser.
        /// </summary>
        /// <param name="pipeline">The pipeline object.</param>
        /// <param name="sourceFiles">A list of relative file names of the sources to compile.</param>
        public static IEnumerable<IAsset> CompileScssFiles(this IAssetPipeline pipeline, WebOptimazerScssOptions options = null, params string[] sourceFiles)
        {
            return pipeline.AddFiles("text/css; charset=UFT-8", sourceFiles)
                           .CompileScss(options)
                           .FingerprintUrls()
                           .AddResponseHeader("X-Content-Type-Options", "nosniff")
                           .MinifyCss();
        }
    }
}
