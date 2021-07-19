using Spidey.Tests.BaseClasses;
using System.Threading.Tasks;
using Xunit;

namespace Spidey.Tests
{
    public class Creation : TestBaseClass<Crawler>
    {
        public Creation()
        {
            TestObject = new Crawler();
        }

        [Fact]
        public async Task Crawl()
        {
            ResultFile Result = null;
            var Options = new Options
            {
                Allow = { "https://ajax.googleapis.com/ajax/libs/jquery/3.4.1/jquery.min.js" },
                StartLocations = { "https://ajax.googleapis.com/ajax/libs/jquery/3.4.1/jquery.min.js" },
                ItemFound = x => Result = x
            };
            var Crawler = new Crawler(Options);
            var Results = await Crawler.StartCrawlAsync().ConfigureAwait(false);
            Assert.Equal(88145, Result.Data.Content.Length);
            Assert.Equal("text/javascript; charset=UTF-8", Result.ContentType);
            Assert.Equal("jquery.min.js", Result.FileName);
            Assert.Equal("https://ajax.googleapis.com/ajax/libs/jquery/3.4.1/jquery.min.js", Result.FinalLocation);
            Assert.Equal(200, Result.StatusCode);
            Assert.Equal("https://ajax.googleapis.com/ajax/libs/jquery/3.4.1/jquery.min.js", Result.Data.URL);
        }

        [Fact]
        public async Task CrawlWithResolve()
        {
            ResultFile Result = null;
            var Options = Canister.Builder.Bootstrapper.Resolve<Options>();
            Options.ItemFound = x => Result = x;
            var Crawler = Canister.Builder.Bootstrapper.Resolve<Crawler>();
            var Results = await Crawler.StartCrawlAsync().ConfigureAwait(false);
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