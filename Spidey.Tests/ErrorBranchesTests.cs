using NSubstitute;
using Spidey.Engines;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Spidey.Tests
{
    public class ErrorBranchesTests
    {
        [Fact]
        public async Task WorkerPool_CrawlAsync_WhenCanceled_ReturnsNull()
        {
            var Engine = Substitute.For<Spidey.Engines.Interfaces.IEngine>();
            using var Cts = new CancellationTokenSource();
            Cts.Cancel();
            var Pool = new Spidey.Engines.Scheduler.WorkerPool(1, Engine, Cts.Token);
            var Result = await Pool.CrawlAsync("http://test");
            Assert.Null(Result);
        }
    }
}