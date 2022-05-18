using Xunit;

namespace WebOptimizer.Sass.Test
{
    public class WebOptimizerScssOptionsTest
    {
        [Fact]
        public void MinifyScssOptionIsTrueByDefault()
        {
            var target = new WebOptimizerScssOptions();

            Assert.True(target.MinifyCss);
        }
    }
}
