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
        public static IAsset AddScss(this IAssetPipeline pipeline, string route, params string[] sourceFiles)
        {
            return pipeline.Add(route, "text/css", sourceFiles)
                           .CompileScss()
                           .AdjustRelativePaths()
                           .Concatinate()
                           .CssFingerprint();
        }
    }
}
