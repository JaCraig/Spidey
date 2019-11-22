using Spidey.Engines;
using System.Threading.Tasks;
using Xunit;

namespace Spidey.Tests.Engines
{
    public class DefaultEngineTests
    {
        [Fact]
        public async Task Crawl()
        {
            var TestObject = new DefaultEngine();
            var TempOptions = new Options
            {
                Allow = { "https://ajax.googleapis.com/ajax/libs/jquery/3.4.1/jquery.min.js" },
            };
            var Result = await TestObject.CrawlAsync("https://ajax.googleapis.com/ajax/libs/jquery/3.4.1/jquery.min.js", TempOptions).ConfigureAwait(false);
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
            var TestObject = new DefaultEngine();
            var TempOptions = new Options
            {
                Allow = { "https://ajax.googleapis.com/ajax/libs/jquery/3.4.1/jquery.min.js" },
            };
            var Result = await TestObject.CrawlAsync("https://ajax.googleapis.com/ajax/libs/jquery/3.4.1/jquery.min.js?test", TempOptions).ConfigureAwait(false);
            Assert.Equal(88145, Result.Content.Length);
            Assert.Equal("text/javascript; charset=UTF-8", Result.ContentType);
            Assert.Equal("jquery.min.js", Result.FileName);
            Assert.Equal("https://ajax.googleapis.com/ajax/libs/jquery/3.4.1/jquery.min.js?test", Result.FinalLocation);
            Assert.Equal(200, Result.StatusCode);
            Assert.Equal("https://ajax.googleapis.com/ajax/libs/jquery/3.4.1/jquery.min.js?test", Result.URL);
        }
    }
}