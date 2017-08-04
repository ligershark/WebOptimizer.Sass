using System.Collections.Generic;

namespace WebOptimizer.Sass
{
    /// <summary>
    /// Extensions methods for registrating the Sass compiler on the Asset Pipeline.
    /// </summary>
    public static class PipelineExtensions
    {
        /// <summary>
        /// Compile Sass or Scss files on the asset pipeline.
        /// </summary>
        public static IAsset CompileScss(this IAsset asset)
        {
            asset.Processors.Add(new Compiler());
            return asset;
        }

        /// <summary>
        /// Compile Sass or Scss files on the asset pipeline.
        /// </summary>
        public static IEnumerable<IAsset> CompileScss(this IEnumerable<IAsset> assets)
        {
            var list = new List<IAsset>();

            foreach (IAsset asset in assets)
            {
                list.Add(asset.CompileScss());
            }

            return list;
        }

        /// <summary>
        /// Compile Sass or Scss files on the asset pipeline.
        /// </summary>
        /// <param name="pipeline">The asset pipeline.</param>
        /// <param name="route">The route where the compiled .css file will be available from.</param>
        /// <param name="sourceFiles">The path to the .sass or .scss source files to compile.</param>
        public static IAsset AddScssBundle(this IAssetPipeline pipeline, string route, params string[] sourceFiles)
        {
            return pipeline.AddBundle(route, "text/css", sourceFiles)
                           .CompileScss()
                           .AdjustRelativePaths()
                           .Concatinate()
                           .FingerprintUrls()
                           .MinifyCss();
        }

        /// <summary>
        /// Compiles .scss files into CSS and makes them servable in the browser.
        /// </summary>
        /// <param name="pipeline">The asset pipeline.</param>
        public static IAsset CompileScssFiles(this IAssetPipeline pipeline)
        {
            return pipeline.AddFileExtension(".scss", "text/css")
                           .CompileScss()
                           .FingerprintUrls()
                           .MinifyCss();
        }

        /// <summary>
        /// Compiles the specified .scss files into CSS and makes them servable in the browser.
        /// </summary>
        /// <param name="pipeline">The pipeline object.</param>
        /// <param name="sourceFiles">A list of relative file names of the sources to compile.</param>
        public static IEnumerable<IAsset> CompileScssFiles(this IAssetPipeline pipeline, params string[] sourceFiles)
        {
            var list = new List<IAsset>();

            foreach (string file in sourceFiles)
            {
                IAsset asset = pipeline.AddBundle(file, "text/css", new[] { file })
                         .CompileScss()
                         .FingerprintUrls()
                         .MinifyCss();

                list.Add(asset);
            }

            return list;
        }
    }
}
