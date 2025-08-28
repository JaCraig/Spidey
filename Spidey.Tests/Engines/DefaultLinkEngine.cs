using BigBook.ExtensionMethods;
using Spidey.Engines;
using Spidey.Tests.BaseClasses;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Xunit;

namespace Spidey.Tests.Engines
{
    public class DefaultLinkEngineTests : TestBaseClass<DefaultLinkDiscoverer>
    {
        public DefaultLinkEngineTests()
        {
            TestObject = new DefaultLinkDiscoverer(Options.Default);
        }

        [Fact]
        public void DiscoverUrls()
        {
            var Result = new DefaultLinkDiscoverer(Options.Default).DiscoverUrls("http://google.com", "http://google.com", "<a href=\"/Temp.html\">ASDF</a><a href=\"/Temp2.html\"></a>".ToByteArray(), "TEXT/HTML");
            Assert.Equal(2, Result.Length);
            Assert.Equal("http://google.com/Temp.html", Result[0]);
            Assert.Equal("http://google.com/Temp2.html", Result[1]);
        }

        [Fact]
        public void FixUrl()
        {
            var Result = new DefaultLinkDiscoverer(Options.Default).FixUrl("http://google.com", "/something-something/dark-side/", "http://google.com", new Dictionary<Regex, string>
            {
                [new Regex("something")] = "blah"
            });
            Assert.Equal("http://google.com/blah-blah/dark-side", Result);
        }

        [Fact]
        public void GetDomain()
        {
            var Result = new DefaultLinkDiscoverer(Options.Default).GetDomain("http://google.com/blah-blah/dark-side");
            Assert.Equal("http://google.com", Result);
        }
    }
}