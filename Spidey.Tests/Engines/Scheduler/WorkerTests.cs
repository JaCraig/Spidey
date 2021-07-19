using NSubstitute;
using Spidey.Engines.Interfaces;
using Spidey.Engines.Scheduler;
using Spidey.Tests.BaseClasses;

namespace Spidey.Tests.Engines.Scheduler
{
    /// <summary>
    /// Worker tests
    /// </summary>
    /// <seealso cref="TestBaseClass{Worker}"/>
    public class WorkerTests : TestBaseClass<Worker>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkerTests"/> class.
        /// </summary>
        public WorkerTests()
        {
            TestObject = new Worker(Substitute.For<IEngine>());
        }
    }
}