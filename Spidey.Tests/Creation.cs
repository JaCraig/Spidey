using Microsoft.Extensions.DependencyInjection;
using Spidey.Tests.BaseClasses;
using System;
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

        private IServiceProvider ServiceProvider
        {
            get
            {
                if (_Services is not null)
                    return _Services;
                lock (LockObj)
                {
                    if (_Services is not null)
                        return _Services;
                    _Services = new ServiceCollection().AddCanisterModules()?.BuildServiceProvider();
                }
                return _Services;
            }
        }

        private IServiceProvider _Services;
        private object LockObj = new();

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
            var Services = new ServiceCollection().AddCanisterModules().AddTransient(_ =>
                new Options
                {
                    Allow = { "https://ajax.googleapis.com/ajax/libs/jquery/3.4.1/jquery.min.js" },
                    StartLocations = { "https://ajax.googleapis.com/ajax/libs/jquery/3.4.1/jquery.min.js" },
                    ItemFound = x => Result = x
                }).BuildServiceProvider();
            var Crawler = Services.GetService<Crawler>();
            var Results = (await Crawler.StartCrawlAsync().ConfigureAwait(false));
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
            var Result = ServiceProvider.GetService<Crawler>();
            Assert.NotNull(Result);
        }
    }
}