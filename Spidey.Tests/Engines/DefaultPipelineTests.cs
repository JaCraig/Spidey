using NSubstitute;
using Spidey.Engines;
using Spidey.Engines.Interfaces;
using Spidey.Tests.BaseClasses;

namespace Spidey.Tests.Engines
{
    public class DefaultPipelineTests : TestBaseClass<DefaultPipeline>
    {
        public DefaultPipelineTests()
        {
            TestObject = new DefaultPipeline(new[] { Substitute.For<IScheduler>() },
                new[] { Substitute.For<IProcessor>() },
                new[] { Substitute.For<IContentParser>() },
                new[] { Substitute.For<ILinkDiscoverer>() },
                Options.Default);
        }
    }
}