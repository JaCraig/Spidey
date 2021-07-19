using Spidey.Engines;
using Spidey.Tests.BaseClasses;

namespace Spidey.Tests.Engines
{
    public class DefaultProcessorTests : TestBaseClass<DefaultProcessor>
    {
        public DefaultProcessorTests()
        {
            TestObject = new DefaultProcessor(Options.Default);
        }
    }
}