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
    public string Indent { get; set; } = "  ";

    /// <summary>
    /// Gets or sets the linefeed. Default is LF (\n)
    /// </summary>
    public string Linefeed { get; set; } = "\n";

    /// <summary>
    /// Gets or sets the value that will be emitted as <c>sourceRoot</c> in the source map information. Default is empty string
    /// </summary>
    public string SourceMapRoot { get; set; } = string.Empty;

    /// <summary>
    /// Gets the include paths that will be used to search for @import directives in scss content. Default is empty list
    /// </summary>
    public List<string> IncludePaths { get; } = [];

    /// <summary>
    /// Gets or sets the option to minify the resulting css. Default is <c>true</c>
    /// </summary>
    public bool MinifyCss { get; set; } = true;
}
