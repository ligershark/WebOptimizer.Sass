using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;
using Moq;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace WebOptimizer.Sass.Test
{
    public class CompilerTest
    {
        [Fact]
        public async Task Compile_Success()
        {
            var context = new Mock<IAssetContext>().SetupAllProperties();
            var asset = new Mock<IAsset>().SetupAllProperties();
            var processor = new Compiler(asset.Object);
            var env = new Mock<IWebHostEnvironment>();
            var fileProvider = new Mock<IFileProvider>();

            context.Object.Content = new Dictionary<string, byte[]> {
                { "/file.css", "$bg: blue; div {background: $bg}".AsByteArray() },
            };

            context.Setup(s => s.HttpContext.RequestServices.GetService(typeof(IWebHostEnvironment)))
                   .Returns(env.Object);
            
            var inputFile = new PhysicalFileInfo(new FileInfo("site.css"));

            context.SetupGet(s => s.Asset)
                          .Returns(asset.Object);

            env.SetupGet(e => e.WebRootFileProvider)
                 .Returns(fileProvider.Object);

            fileProvider.Setup(f => f.GetFileInfo(It.IsAny<string>()))
                   .Returns(inputFile);

            await processor.ExecuteAsync(context.Object);
            var result = context.Object.Content.First().Value;
            Assert.Equal("div {\n  background: blue; }\n", result.AsString());
        }

        [Fact]
        public void CacheKey_Success()
        {
            var asset = new Mock<IAsset>();
            asset.Setup(a => a.SourceFiles)
                .Returns(new List<string>()
                {
                    "css/test.scss"
                });
            var env = new Mock<IWebHostEnvironment>();

            // Setup files
            var fileProvider = new Mock<IFileProvider>();
            env.Setup(e => e.WebRootFileProvider)
                .Returns(fileProvider.Object);
            var fileInfo = new Mock<IFileInfo>();
            fileInfo.Setup(f => f.Exists).Returns(true);
            fileInfo.Setup(f => f.PhysicalPath).Returns("css/test1.scss");
            fileInfo.SetupSequence(f => f.CreateReadStream())
                .Returns(new MemoryStream(Encoding.Default.GetBytes("@import 'test1'; \r\n @import 'test2';")))
                .Returns(new MemoryStream(Encoding.Default.GetBytes("@import 'test3';")))
                .Returns(new MemoryStream(Encoding.Default.GetBytes("@import 'test3';")))
                .Returns(new MemoryStream(Encoding.Default.GetBytes("body { padding: 0; margin: 0; }")))
                .Returns(new MemoryStream(Encoding.Default.GetBytes("body { padding: 0; margin: 0; }")))
                .Returns(new MemoryStream(Encoding.Default.GetBytes("body { padding: 1; margin: 1; }")))
                .Returns(new MemoryStream(Encoding.Default.GetBytes("body { padding: 1; margin: 1; }")));

            var changeToken = new Mock<IChangeToken>();
            fileProvider.Setup(p => p.Watch(It.IsAny<string>()))
                .Returns(changeToken.Object);
            fileProvider.Setup(p => p.GetFileInfo(It.IsAny<string>()))
                .Returns(fileInfo.Object);
            
            // Setup Cache
            var cache = new Mock<IMemoryCache>();
            object test;
            cache.Setup(c => c.TryGetValue(It.IsAny<object>(), out test))
                 .Returns(false);
            var cacheEntry = new Mock<ICacheEntry>().SetupAllProperties();
            cacheEntry.Setup(e => e.ExpirationTokens)
                .Returns(new List<IChangeToken>());
            cache.Setup(c => c.CreateEntry(It.IsAny<object>()))
                 .Returns(cacheEntry.Object);
            
            // Setup Service provider
            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(p => p.GetService(typeof(IWebHostEnvironment)))
                .Returns(env.Object);
            serviceProvider.Setup(p => p.GetService(typeof(IMemoryCache)))
                .Returns(cache.Object);
            
            // Setup HttpContext
            var httpContext = new DefaultHttpContext()
            {
                RequestServices = serviceProvider.Object,
                Request = { PathBase = "/wwwroot" }
            };
            
            var processor = new Compiler(asset.Object);
            var cacheKey = processor.CacheKey(httpContext);
            Assert.NotEmpty(cacheKey);
        }
    }
}
