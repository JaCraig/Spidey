using Spidey;
using System;
using Xunit;

namespace Spidey.Tests
{
    public class ErrorItemTests
    {
        [Fact]
        public void Constructor_SetsPropertiesCorrectly()
        {
            var Ex = new InvalidOperationException("fail");
            var Url = "http://test";
            var Status = 404;
            var Item = new ErrorItem(Ex, Url, Status);
            Assert.Equal(Ex, Item.Error);
            Assert.Equal(Url, Item.Url);
            Assert.Equal(Status, Item.StatusCode);
        }
    }
}