using NSubstitute;
using Spidey;
using Spidey.Engines;
using Spidey.Engines.Scheduler;
using System.Threading;
using Xunit;

namespace Spidey.Tests
{
    public class DisposeAndEdgeCaseTests
    {
        [Fact]
        public void WorkerPool_Dispose_CanBeCalledMultipleTimes()
        {
            var Engine = Substitute.For<Spidey.Engines.Interfaces.IEngine>();
            var Pool = new WorkerPool(2, Engine, CancellationToken.None);
            Pool.Dispose();
            Pool.Dispose(); // Should not throw
        }

        [Fact]
        public void WorkerPool_Done_WhenWorkersNull_ReturnsTrue()
        {
            var Engine = Substitute.For<Spidey.Engines.Interfaces.IEngine>();
            var Pool = new WorkerPool(1, Engine, CancellationToken.None);
            Pool.Dispose();
            Assert.True(Pool.Done);
        }
    }
}