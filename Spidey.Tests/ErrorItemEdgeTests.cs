using Spidey;
using Xunit;

namespace Spidey.Tests
{
    public class ErrorItemEdgeTests
    {
        [Fact]
        public void ErrorItem_AllowsNulls()
        {
            var Item = new ErrorItem(null!, null!, 0);
            Assert.Null(Item.Error);
            Assert.Null(Item.Url);
            Assert.Equal(0, Item.StatusCode);
        }
    }
}