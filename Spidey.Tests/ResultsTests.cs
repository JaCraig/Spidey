using Spidey;
using Xunit;

namespace Spidey.Tests
{
    public class ResultsTests
    {
        [Fact]
        public void Properties_AreInitialized()
        {
            var Results = new Results();
            Assert.NotNull(Results.CompletedURLs);
            Assert.NotNull(Results.ErrorURLs);
            Assert.NotNull(Results.WhereFound);
        }
    }
}