using Xunit;

namespace WebOptimizer.Sass.Test
{
    public class WebOptimazerScssOptionsTest
    {
        [Fact]
        public void MinifyScssOptionIsTrueByDefault()
        {
            var target = new WebOptimazerScssOptions();

            Assert.True(target.MinifyCss);
        }
    }
}
