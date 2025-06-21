using JavaScriptEngineSwitcher.Core;
using Microsoft.Extensions.DependencyInjection;

namespace WebOptimizer.Sass;

/// <summary>
/// Extensions methods for registrating the Sass compiler on the Asset Pipeline.
/// </summary>
public static class PipelineExtensions
{
    /// <summary>
    /// Compile Sass or Scss files on the asset pipeline.
    /// </summary>
    /// <param name="pipeline">The asset pipeline.</param>
    /// <param name="route">The route where the compiled .css file will be available from.</param>
    /// <param name="options"></param>
    /// <param name="sourceFiles">The path to the .sass or .scss source files to compile.</param>
    public static IAsset AddScssBundle(this IAssetPipeline pipeline, WebOptimizerScssOptions? options, string? route, params string[] sourceFiles)
    {
        CheckJsEngineRegistration();

        return pipeline.AddBundle(route, "text/css; charset=UTF-8", sourceFiles)
                       .CompileScss(options)
                       .AdjustRelativePaths()
                       .Concatenate()
                       .FingerprintUrls()
                       .AddResponseHeader("X-Content-Type-Options", "nosniff")
                       .MinifyCssWithOptions(options);
    }

    /// <summary>
    /// Adds the SCSS bundle.
    /// </summary>
    /// <param name="pipeline">The pipeline.</param>
    /// <param name="route">The route.</param>
    /// <param name="sourceFiles">The source files.</param>
    /// <returns>IAsset.</returns>
    public static IAsset AddScssBundle(this IAssetPipeline pipeline, string route, params string[] sourceFiles)
    {
        return pipeline.AddScssBundle(null, route, sourceFiles);
    }

    /// <summary>
    /// Compile Sass or Scss files on the asset pipeline.
    /// </summary>
    public static IAsset CompileScss(this IAsset asset, WebOptimizerScssOptions? options = null)
    {
        CheckJsEngineRegistration();

        asset.Processors.Add(new Compiler(asset, options));
        return asset;
    }

    /// <summary>
    /// Compile Sass or Scss files on the asset pipeline.
    /// </summary>
    public static IEnumerable<IAsset> CompileScss(this IEnumerable<IAsset> assets, WebOptimizerScssOptions? options = null)
    {
        CheckJsEngineRegistration();

        List<IAsset> list = [];

        foreach (IAsset asset in assets)
        {
            list.Add(asset.CompileScss(options));
        }

        return list;
    }

    /// <summary>
    /// Compiles .scss files into CSS and makes them servable in the browser.
    /// </summary>
    /// <param name="pipeline">The asset pipeline.</param>
    /// <param name="options"></param>
    public static IEnumerable<IAsset> CompileScssFiles(this IAssetPipeline pipeline, WebOptimizerScssOptions? options = null)
    {
        CheckJsEngineRegistration();

        return pipeline.AddFiles("text/css; charset=UTF-8", "**/*.scss")
                       .CompileScss(options)
                       .FingerprintUrls()
                       .AddResponseHeader("X-Content-Type-Options", "nosniff")
                       .MinifyCssWithOptions(options);
    }

    /// <summary>
    /// Compiles the specified .scss files into CSS and makes them servable in the browser.
    /// </summary>
    /// <param name="pipeline">The pipeline object.</param>
    /// <param name="options"></param>
    /// <param name="sourceFiles">A list of relative file names of the sources to compile.</param>
    public static IEnumerable<IAsset> CompileScssFiles(this IAssetPipeline pipeline, WebOptimizerScssOptions? options = null, params string[] sourceFiles)
    {
        CheckJsEngineRegistration();

        return pipeline.AddFiles("text/css; charset=UTF-8", sourceFiles)
                       .CompileScss(options)
                       .FingerprintUrls()
                       .AddResponseHeader("X-Content-Type-Options", "nosniff")
                       .MinifyCssWithOptions(options);
    }

    private static void CheckJsEngineRegistration()
    {
        if (!JsEngineSwitcher.AllowCurrentProperty)
        {
            return;
        }

        IJsEngineSwitcher engineSwitcher = JsEngineSwitcher.Current;
        if (engineSwitcher is null || !engineSwitcher.EngineFactories.Any(e => e.EngineName == engineSwitcher.DefaultEngineName))
        {
            throw new InvalidOperationException("JS engine is not registered.");
        }
    }

    private static IEnumerable<IAsset> MinifyCssWithOptions(this IEnumerable<IAsset> assets, WebOptimizerScssOptions? options)
    {
        return options?.MinifyCss ?? true ? assets.MinifyCss() : assets;
    }

    private static IAsset MinifyCssWithOptions(this IAsset asset, WebOptimizerScssOptions? options)
    {
        return options?.MinifyCss ?? true ? asset.MinifyCss() : asset;
    }
}