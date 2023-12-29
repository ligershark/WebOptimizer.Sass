using DartSassHost;
using JavaScriptEngineSwitcher.Core.Resources;
using Microsoft.Extensions.FileProviders;
using System;
using System.IO;
using System.Text;

namespace WebOptimizer.Sass
{
    public class ManifestFileManager : IFileManager
    {
        private readonly IFileProvider _fileProvider;

        public ManifestFileManager(IFileProvider fileProvider)
        {
            _fileProvider = fileProvider;
        }

        public bool SupportsVirtualPaths => false;

        public bool FileExists(string path)
        {
            IFileInfo file = _fileProvider.GetFileInfo(path);
            return file.Exists;
        }

        public string GetCurrentDirectory() => string.Empty;

        public bool IsAppRelativeVirtualPath(string path) => throw new NotImplementedException();


        public string ReadFile(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(
                    nameof(path),
                    string.Format(Strings.Common_ArgumentIsNull, nameof(path))
                );
            }

            IFileInfo file = _fileProvider.GetFileInfo(path);
            using Stream stream = file.CreateReadStream();
            using StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            return reader.ReadToEnd();
        }

        public string ToAbsoluteVirtualPath(string path) => throw new NotImplementedException();
    }
}
