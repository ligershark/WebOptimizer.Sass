using DartSassHost;
using JavaScriptEngineSwitcher.Core.Resources;
using Microsoft.Extensions.FileProviders;
using System;
using System.IO;
using System.Text;
using WebOptimizer.Utils;

namespace WebOptimizer.Sass
{
    public class FileProviderFileManager : IFileManager
    {
        private readonly IFileProvider _fileProvider;

        public FileProviderFileManager(IFileProvider fileProvider)
        {
            _fileProvider = fileProvider;
        }

        public bool SupportsVirtualPaths => false;

        public bool FileExists(string path)
        {
            return GetFileInfo(path).Exists;
        }

        public string GetCurrentDirectory() => string.Empty;

        public bool IsAppRelativeVirtualPath(string path) => throw new NotSupportedException();

        public string ReadFile(string path)
        {
            IFileInfo file = GetFileInfo(path);
            using Stream stream = file.CreateReadStream();
            using StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            return reader.ReadToEnd();
        }

        public string ToAbsoluteVirtualPath(string path) => throw new NotSupportedException();

        private IFileInfo GetFileInfo(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(
                    nameof(path),
                    string.Format(Strings.Common_ArgumentIsNull, nameof(path))
                );
            }

            var adjustedPath = UrlPathUtils.Normalize(path.Replace(Path.DirectorySeparatorChar, '/'));
            return _fileProvider.GetFileInfo(adjustedPath);
        }
    }
}
