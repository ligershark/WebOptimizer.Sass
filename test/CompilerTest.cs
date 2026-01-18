using JavaScriptEngineSwitcher.Core;
using JavaScriptEngineSwitcher.Jint;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebOptimizer.Sass.Test.Mocks;
using WebOptimizer.Utils;
using Xunit;

namespace WebOptimizer.Sass.Test
{
    public class CompilerTest
    {
        static CompilerTest()
        {
            IJsEngineSwitcher engineSwitcher = JsEngineSwitcher.Current;
            engineSwitcher.EngineFactories
                .AddJint()
                ;

            engineSwitcher.DefaultEngineName = JintJsEngine.EngineName;
        }


        [Theory]
        [InlineData("$bg: blue; div {background: $bg}", "div {\n  background: blue;\n}")]
        [InlineData("@import 'test';\ndiv {background: $bg}", "div {\n  background: red;\n}")]
        [InlineData("@import '../test';\ndiv {background: $bg}", "div {\n  background: green;\n}")]
        [InlineData("@import '/test';\ndiv {background: $bg}", "div {\n  background: green;\n}")]
        [InlineData("@import 'sub/test';\ndiv {background: $bg}", "div {\n  background: white;\n}")]
        [InlineData("@import '/css/sub/test';\ndiv {background: $bg}", "div {\n  background: white;\n}")]
        [InlineData("@import 'variables';\ndiv {background: $bg}", "div {\n  background: black;\n}")]
        public async Task Compile_Success(string scssContent, string compiled)
        {
            var context = new Mock<IAssetContext>().SetupAllProperties();
            var asset = new Mock<IAsset>().SetupAllProperties();
            var logger = new Mock<ILogger<Compiler>>();
            var processor = new Compiler(asset.Object);
            var env = new Mock<IWebHostEnvironment>();
            var fileProvider = new Mock<IFileProvider>();

            fileProvider.Setup(f => f.GetFileInfo(It.IsAny<string>()))
                .Returns((string path) => new NotFoundFileInfo(UrlPathUtils.GetFileName(path)));

            fileProvider.Setup(f => f.GetFileInfo(It.Is<string>(value => value.Equals("/css/file.scss", StringComparison.InvariantCultureIgnoreCase))))
                .Returns(new MockFileInfo("file.scss", new DateTime(2017, 1, 1), scssContent.AsByteArray()));

            fileProvider.Setup(f => f.GetFileInfo(It.Is<string>(value => value.Equals("/css/test.scss", StringComparison.InvariantCultureIgnoreCase))))
                .Returns(new MockFileInfo("test.scss", new DateTime(2017, 1, 1), "$bg: red;".AsByteArray()));

            fileProvider.Setup(f => f.GetFileInfo(It.Is<string>(value => value.Equals("/test.scss", StringComparison.InvariantCultureIgnoreCase))))
                .Returns(new MockFileInfo("test.scss", new DateTime(2017, 1, 1), "$bg: green;".AsByteArray()));

            fileProvider.Setup(f => f.GetFileInfo(It.Is<string>(value => value.Equals("/css/sub/test.scss", StringComparison.InvariantCultureIgnoreCase))))
                .Returns(new MockFileInfo("test.scss", new DateTime(2017, 1, 1), "$bg: white;".AsByteArray()));

            fileProvider.Setup(f => f.GetFileInfo(It.Is<string>(value => value.Equals("/css/_variables.scss", StringComparison.InvariantCultureIgnoreCase))))
                .Returns(new MockFileInfo("_variables.scss", new DateTime(2017, 1, 1), "$bg: black;".AsByteArray()));

            context.Object.Content = new Dictionary<string, byte[]> {
                { "css/file.scss", scssContent.AsByteArray() },
            };

            context.Setup(s => s.HttpContext.RequestServices.GetService(typeof(IWebHostEnvironment)))
                   .Returns(env.Object);

            context.Setup(s => s.HttpContext.RequestServices.GetService(typeof(IJsEngineSwitcher)))
                   .Returns(JsEngineSwitcher.Current);

            context.Setup(s => s.HttpContext.RequestServices.GetService(typeof(ILogger<Compiler>)))
                   .Returns(logger.Object);

            context.SetupGet(s => s.Asset)
                          .Returns(asset.Object);

            env.SetupGet(e => e.WebRootFileProvider)
                 .Returns(fileProvider.Object);

            await processor.ExecuteAsync(context.Object);
            var result = context.Object.Content.First().Value;
            Assert.Equal(compiled, result.AsString());
        }

        [Fact]
        public void CacheKey_Success()
        {
            var context = new Mock<IAssetContext>().SetupAllProperties();
            var asset = new Mock<IAsset>();
            asset.Setup(a => a.SourceFiles)
                .Returns(
                [
                    "css/test1.scss"
                ]);
            var env = new Mock<IWebHostEnvironment>();

            context.SetupGet(s => s.Asset)
                .Returns(asset.Object);

            // Setup files
            var fileProvider = new Mock<IFileProvider>();

            fileProvider.Setup(f => f.GetFileInfo(It.IsAny<string>()))
                .Returns((string path) => new NotFoundFileInfo(UrlPathUtils.GetFileName(path)));

            fileProvider.Setup(f => f.GetFileInfo(It.Is<string>(value => value.Equals("/css/test1.scss", StringComparison.InvariantCultureIgnoreCase))))
                .Returns(new MockFileInfo("test1.scss", new DateTime(2017, 1, 1), "@import 'test2'; \r\n @import \"test3\";".AsByteArray()));

            fileProvider.Setup(f => f.GetFileInfo(It.Is<string>(value => value.Equals("/css/test2.scss", StringComparison.InvariantCultureIgnoreCase))))
                .Returns(new MockFileInfo("test2.scss", new DateTime(2017, 1, 1), "@import '../test4';".AsByteArray()));

            fileProvider.Setup(f => f.GetFileInfo(It.Is<string>(value => value.Equals("/css/test3.scss", StringComparison.InvariantCultureIgnoreCase))))
                .Returns(new MockFileInfo("test3.scss", new DateTime(2017, 1, 1), "@import 'http://localhost/'; \r\n @use 'test5';".AsByteArray()));

            fileProvider.Setup(f => f.GetFileInfo(It.Is<string>(value => value.Equals("/css/test4.scss", StringComparison.InvariantCultureIgnoreCase))))
                .Returns(new MockFileInfo("test4.scss", new DateTime(2017, 1, 1), "@import 'http://localhost/';".AsByteArray()));

            fileProvider.Setup(f => f.GetFileInfo(It.Is<string>(value => value.Equals("/css/test5.scss", StringComparison.InvariantCultureIgnoreCase))))
                .Returns(new MockFileInfo("test5.scss", new DateTime(2017, 1, 1), "@use '../test6';".AsByteArray()));

            fileProvider.Setup(f => f.GetFileInfo(It.Is<string>(value => value.Equals("/css/test6.scss", StringComparison.InvariantCultureIgnoreCase))))
                .Returns(new MockFileInfo("test6.scss", new DateTime(2017, 1, 1), "@use 'http://localhost/';".AsByteArray()));

            env.Setup(e => e.WebRootFileProvider)
                .Returns(fileProvider.Object);

            var changeToken = new Mock<IChangeToken>();
            fileProvider.Setup(p => p.Watch(It.IsAny<string>()))
                .Returns(changeToken.Object);

            // Setup Cache
            var cache = new Mock<IMemoryCache>();
            object test;
            cache.Setup(c => c.TryGetValue(It.IsAny<object>(), out test))
                 .Returns(false);
            var cacheEntry = new Mock<ICacheEntry>().SetupAllProperties();
            cacheEntry.Setup(e => e.ExpirationTokens)
                .Returns([]);
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
            var cacheKey = processor.CacheKey(httpContext, context.Object);
            Assert.NotEmpty(cacheKey);
        }
    }
}
