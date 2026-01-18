using DartSassHost;

namespace WebOptimizer.Sass;

/// <summary>
/// Options for configuring the SCSS processing in WebOptimizer.
/// </summary>
public class WebOptimizerScssOptions
{
    /// <summary>
    /// Gets or sets the maximum number of digits after the decimal. Default is 5.
    /// </summary>
    [Obsolete]
    public int Precision { get; set; } = 5;

    /// <summary>
    /// Gets or sets the output style. Default is <see cref="OutputStyle.Expanded"/>
    /// </summary>
    public OutputStyle OutputStyle { get; set; } = OutputStyle.Expanded;

    /// <summary>
    /// Gets or sets a value indicating whether to generate source map. Default is <c>false</c>
    /// </summary>
    public bool GenerateSourceMap { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether to enable additional debugging information in the output file as CSS comments. Default is <c>false</c>
    /// </summary>
    [Obsolete]
    public bool SourceComments { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether to embed the source map as a data URI. Default is <c>false</c>
    /// </summary>
    public bool SourceMapEmbed { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether to include the contents in the source map information. Default is <c>false</c>
    /// </summary>
    public bool SourceMapContents { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether to enable or disable the inclusion of source map information in the output file. Default is <c>false</c>
    /// </summary>
    public bool OmitSourceMapUrl { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether the scss content to transform is using indented syntax. Default is <c>false</c>
    /// </summary>
    public bool IsIndentedSyntaxSource { get; set; } = false;

    /// <summary>
    /// Gets or sets the indent string. Default is 2 spaces
    /// </summary>
    [Obsolete]
    public string Indent { get; set; } = "  ";

    /// <summary>
    /// Gets or sets the linefeed. Default is LF (\n)
    /// </summary>
    [Obsolete]
    public string Linefeed { get; set; } = "\n";

    /// <summary>
    /// Gets or sets the value that will be emitted as <c>sourceRoot</c> in the source map information. Default is empty string
    /// </summary>
    public string SourceMapRoot { get; set; } = string.Empty;

    /// <summary>
    /// Gets the include paths that will be used to search for @import directives in scss content. Default is empty list
    /// </summary>
    public List<string> IncludePaths { get; set; } = [];

    /// <summary>
    /// Gets or sets the warning level. Default is <see cref="WarningLevel.Default"/>
    /// </summary>
    /// <remarks>
    /// In a production environment, it is recommended to use a <see cref="WarningLevel.Quiet"/> level.
    /// </remarks>
    public WarningLevel WarningLevel { get; set; } = WarningLevel.Default;

    /// <summary>
    /// Gets or sets a value indicating whether to silence compiler warnings from stylesheets loaded by using
    /// the <see cref="IncludePaths"/> property. Default is <c>false</c>
    /// </summary>
    public bool QuietDependencies { get; set; } = false;

    /// <summary>
    /// Gets or sets the list of deprecations to treat as fatal. Default is empty list
    /// </summary>
    /// <remarks>
    /// <para>If a deprecation warning of any provided ID is encountered during compilation, the compiler will
    /// error instead.</para>
    /// <para>If a version is provided, then all deprecations that were active in that compiler version will be
    /// treated as fatal.</para>
    /// </remarks>
    public List<string> FatalDeprecations { get; set; } = [];

    /// <summary>
    /// Gets or sets the list of future deprecations to opt into early. Default is empty list
    /// </summary>
    /// <remarks>
    /// Future deprecations, whose IDs have been passed here, will be treated as active by the compiler,
    /// emitting warnings as necessary.
    /// </remarks>
    public List<string> FutureDeprecations { get; set; } = [];

    /// <summary>
    /// Gets or sets the list of active deprecations to ignore. Default is empty list
    /// </summary>
    /// <remarks>
    /// If a deprecation warning of any provided ID is encountered during compilation, the compiler will
    /// ignore it instead.
    /// </remarks>
    public List<string> SilenceDeprecations { get; set; } = [];

    /// <summary>
    /// Gets or sets the option to minify the resulting css. Default is <c>true</c>
    /// </summary>
    public bool MinifyCss { get; set; } = true;
}
