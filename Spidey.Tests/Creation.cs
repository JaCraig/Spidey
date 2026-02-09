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

        private IServiceProvider? _Services;

        private object LockObj = new();

        private IServiceProvider? ServiceProvider
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

        [Fact]
        public async Task Crawl()
        {
            ResultFile? Result = null;
            var Options = new Options
            {
                Allow = { "https://code.jquery.com/jquery-3.7.1.min.js" },
                StartLocations = { "https://code.jquery.com/jquery-3.7.1.min.js" },
                ItemFound = x => Result = x,
            };
            var Crawler = new Crawler(Options);
            var Results = await Crawler.StartCrawlAsync();
            Assert.NotNull(Results);
            Assert.NotNull(Result);
            Assert.Equal(87533, Result.Data.Content.Length);
            Assert.Equal("application/javascript; charset=utf-8", Result.ContentType);
            Assert.Equal("jquery-3.7.1.min.js", Result.FileName);
            Assert.Equal("https://code.jquery.com/jquery-3.7.1.min.js", Result.FinalLocation);
            Assert.Equal(200, Result.StatusCode);
            Assert.Equal("https://code.jquery.com/jquery-3.7.1.min.js", Result.Data.URL);
        }

        [Fact]
        public async Task CrawlWithResolve()
        {
            ResultFile? Result = null;
            var Services = new ServiceCollection().AddCanisterModules()?.AddTransient(_ =>
                new Options
                {
                    Allow = { "https://code.jquery.com/jquery-3.7.1.min.js" },
                    StartLocations = { "https://code.jquery.com/jquery-3.7.1.min.js" },
                    ItemFound = x => Result = x
                }).BuildServiceProvider();
            var Crawler = Services?.GetService<Crawler>();
            Assert.NotNull(Crawler);
            var Results = await Crawler.StartCrawlAsync();
            Assert.NotNull(Results);
            Assert.NotNull(Result);
            Assert.Equal(87533, Result.Data.Content.Length);
            Assert.Equal("application/javascript; charset=utf-8", Result.ContentType);
            Assert.Equal("jquery-3.7.1.min.js", Result.FileName);
            Assert.Equal("https://code.jquery.com/jquery-3.7.1.min.js", Result.FinalLocation);
            Assert.Equal(200, Result.StatusCode);
            Assert.Equal("https://code.jquery.com/jquery-3.7.1.min.js", Result.Data.URL);
        }

        [Fact]
        public void Registration()
        {
            var Result = ServiceProvider?.GetService<Crawler>();
            Assert.NotNull(Result);
        }
    }
}