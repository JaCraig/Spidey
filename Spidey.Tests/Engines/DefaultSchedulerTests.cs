using NSubstitute;
using Spidey.Engines;
using Spidey.Engines.Interfaces;
using Spidey.Tests.BaseClasses;

namespace Spidey.Tests.Engines
{
    public class DefaultSchedulerTests : TestBaseClass<DefaultScheduler>
    {
        public DefaultSchedulerTests()
        {
            TestObject = new DefaultScheduler(Options.Default, new[] { Substitute.For<IEngine>() });
        }
    }
}