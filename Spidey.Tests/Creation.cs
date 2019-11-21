using Spidey.Tests.BaseClasses;
using Xunit;

namespace Spidey.Tests
{
    public class Creation : TestBaseClass
    {
        [Fact]
        public void Crawl()
        {
            var Crawler = new Crawler(new Options
            {
                Allow = { "https://ajax.googleapis.com/ajax/libs/jquery/3.4.1/jquery.min.js" },
                StartLocations = { "https://ajax.googleapis.com/ajax/libs/jquery/3.4.1/jquery.min.js" },
            }, null);
            ResultFile Result = null;
            Crawler.Options.ItemFound = x => Result = x;
            Crawler.StartCrawl();
            Assert.Equal(88145, Result.Data.Content.Length);
            Assert.Equal("text/javascript; charset=UTF-8", Result.ContentType);
            Assert.Equal("jquery.min.js", Result.FileName);
            Assert.Equal("https://ajax.googleapis.com/ajax/libs/jquery/3.4.1/jquery.min.js", Result.FinalLocation);
            Assert.Equal(200, Result.StatusCode);
            Assert.Equal("https://ajax.googleapis.com/ajax/libs/jquery/3.4.1/jquery.min.js", Result.Data.URL);
        }

        [Fact]
        public void CrawlWithResolve()
        {
            var Crawler = Canister.Builder.Bootstrapper.Resolve<Crawler>();
            ResultFile Result = null;
            Crawler.Options.ItemFound = x => Result = x;
            Crawler.StartCrawl();
            Assert.Equal(88145, Result.Data.Content.Length);
            Assert.Equal("text/javascript; charset=UTF-8", Result.ContentType);
            Assert.Equal("jquery.min.js", Result.FileName);
            Assert.Equal("https://ajax.googleapis.com/ajax/libs/jquery/3.4.1/jquery.min.js", Result.FinalLocation);
            Assert.Equal(200, Result.StatusCode);
            Assert.Equal("https://ajax.googleapis.com/ajax/libs/jquery/3.4.1/jquery.min.js", Result.Data.URL);
        }

        [Fact]
        public void Registration()
        {
            var Result = Canister.Builder.Bootstrapper.Resolve<Crawler>();
            Assert.NotNull(Result);
        }
    }
}