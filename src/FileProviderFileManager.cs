using DartSassHost;
using JavaScriptEngineSwitcher.Core.Resources;
using Microsoft.Extensions.FileProviders;
using System.Text;
using WebOptimizer.Utils;

namespace WebOptimizer.Sass;

/// <summary>
/// The file provider file manager uses an <see cref="IFileProvider"/> to manage files.
/// Implements the <see cref="IFileManager" />
/// </summary>
/// <seealso cref="IFileManager" />
public class FileProviderFileManager(IFileProvider fileProvider) : IFileManager
{
    /// <inheritdoc/>
    public bool SupportsVirtualPaths => false;

    /// <inheritdoc/>
    public bool FileExists(string path)
    {
        return GetFileInfo(path).Exists;
    }

    /// <inheritdoc/>
    public string GetCurrentDirectory()
    {
        return string.Empty;
    }

    /// <inheritdoc/>
    public bool IsAppRelativeVirtualPath(string path)
    {
        throw new NotSupportedException();
    }

    /// <inheritdoc/>
    public string ReadFile(string path)
    {
        IFileInfo file = GetFileInfo(path);
        using Stream stream = file.CreateReadStream();
        using StreamReader reader = new(stream, Encoding.UTF8);
        return reader.ReadToEnd();
    }

    /// <inheritdoc/>
    public string ToAbsoluteVirtualPath(string path)
    {
        throw new NotSupportedException();
    }

    private IFileInfo GetFileInfo(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentNullException(
                nameof(path),
                string.Format(Strings.Common_ArgumentIsNull, nameof(path))
            );
        }

        string adjustedPath = UrlPathUtils.Normalize(path.Replace(Path.DirectorySeparatorChar, '/'));
        return fileProvider.GetFileInfo(adjustedPath);
    }
}