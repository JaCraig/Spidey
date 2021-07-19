using NSubstitute;
using Spidey.Engines.Interfaces;
using Spidey.Engines.Scheduler;
using Spidey.Tests.BaseClasses;

namespace Spidey.Tests.Engines.Scheduler
{
    public class WorkerPoolTests : TestBaseClass<WorkerPool>
    {
        public WorkerPoolTests()
        {
            TestObject = new WorkerPool(2, Substitute.For<IEngine>(), new System.Threading.CancellationToken());
        }
    }
}