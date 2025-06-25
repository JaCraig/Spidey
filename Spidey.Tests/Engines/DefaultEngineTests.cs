using Spidey.Engines;
using Spidey.Tests.BaseClasses;
using System.Threading.Tasks;
using Xunit;

namespace Spidey.Tests.Engines
{
    public class DefaultEngineTests : TestBaseClass<DefaultEngine>
    {
        public DefaultEngineTests()
        {
            TestObject = new DefaultEngine(Options.Default);
        }

        [Fact]
        public async Task Crawl()
        {
            var TestObject = new DefaultEngine(
            new Options
            {
                Allow = { "https://ajax.googleapis.com/ajax/libs/jquery/3.4.1/jquery.min.js" },
            });
            var Result = await TestObject.CrawlAsync("https://ajax.googleapis.com/ajax/libs/jquery/3.4.1/jquery.min.js");
            Assert.NotNull(Result);
            Assert.Equal(88145, Result.Content.Length);
            Assert.Equal("text/javascript; charset=UTF-8", Result.ContentType);
            Assert.Equal("jquery.min.js", Result.FileName);
            Assert.Equal("https://ajax.googleapis.com/ajax/libs/jquery/3.4.1/jquery.min.js", Result.FinalLocation);
            Assert.Equal(200, Result.StatusCode);
            Assert.Equal("https://ajax.googleapis.com/ajax/libs/jquery/3.4.1/jquery.min.js", Result.URL);
        }

        [Fact]
        public async Task CrawlWithQueryString()
        {
            var TestObject = new DefaultEngine(
            new Options
            {
                Allow = { "https://ajax.googleapis.com/ajax/libs/jquery/3.4.1/jquery.min.js" },
            });
            var Result = await TestObject.CrawlAsync("https://ajax.googleapis.com/ajax/libs/jquery/3.4.1/jquery.min.js?test");
            Assert.NotNull(Result);
            Assert.Equal(88145, Result.Content.Length);
            Assert.Equal("text/javascript; charset=UTF-8", Result.ContentType);
            Assert.Equal("jquery.min.js", Result.FileName);
            Assert.Equal("https://ajax.googleapis.com/ajax/libs/jquery/3.4.1/jquery.min.js?test", Result.FinalLocation);
            Assert.Equal(200, Result.StatusCode);
            Assert.Equal("https://ajax.googleapis.com/ajax/libs/jquery/3.4.1/jquery.min.js?test", Result.URL);
        }
    }
}