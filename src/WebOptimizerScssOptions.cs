using DartSassHost;
using System;
using System.Collections.Generic;
using System.Text;

namespace WebOptimizer.Sass
{
    public class WebOptimizerScssOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScssOptions"/> class.
        /// </summary>
        public WebOptimizerScssOptions()
        {
            //Precision = 5;
            OutputStyle = OutputStyle.Expanded;
            IncludePaths = new List<string>();
            Indent = "  ";
            Linefeed = "\n";
        }

        ///// <summary>
        ///// Gets or sets the maximum number of digits after the decimal. Default is 5. 
        ///// TODO: the C function is not working
        ///// </summary>
        //public int Precision { get; set; }

        /// <summary>
        /// Gets or sets the output style. Default is <see cref="ScssOutputStyle.Nested"/>
        /// </summary>
        public OutputStyle OutputStyle { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to generate source map (result in <see cref="ScssResult.SourceMap"/>
        /// </summary>
        /// <remarks>
        /// Note that <see cref="OutputFile"/> should be setup. <see cref="SourceMapFile"/> will then automatically
        /// map to <see cref="OutputFile"/> + ".map" unless specified.
        /// </remarks>
        public bool GenerateSourceMap { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to enable additional debugging information in the output file as CSS comments. Default is <c>false</c>
        /// </summary>
        public bool SourceComments { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to embed the source map as a data URI. Default is <c>false</c>
        /// </summary>
        public bool SourceMapEmbed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include the contents in the source map information. Default is <c>false</c>
        /// </summary>
        public bool SourceMapContents { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to enable or disable the inclusion of source map information in the output file. Default is <c>false</c>
        /// </summary>
        /// <remarks>
        /// If this is set to <c>true</c>, the <see cref="OutputFile"/> must be setup to avoid unexpected behavior.
        /// </remarks>
        public bool OmitSourceMapUrl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the scss content to transform is using indented syntax.
        /// </summary>
        public bool IsIndentedSyntaxSource { get; set; }

        /// <summary>
        /// Gets or sets the indent string. Default is 2 spaces.
        /// </summary>
        public string Indent { get; set; }

        /// <summary>
        /// Gets or sets the linefeed. Default is LF (\n)
        /// </summary>
        public string Linefeed { get; set; }

        /// <summary>
        /// Gets or sets the value that will be emitted as sourceRoot in the source map information. Default is null.
        /// </summary>
        public string SourceMapRoot { get; set; }

        /// <summary>
        /// Gets the include paths that will be used to search for @import directives in scss content.
        /// </summary>
        public List<string> IncludePaths { get; }

        /// <summary>
        /// Gets or sets a dynamic delegate used to resolve imports dynamically.
        /// </summary>
        //public TryImportDelegate TryImport { get; set; }

        /// <summary>
        /// Gets or sets the option to minify the resulting css.
        /// </summary>
        public bool MinifyCss { get; set; } = true;
    }
}
