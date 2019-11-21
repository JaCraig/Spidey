using BigBook;
using Spidey.Engines;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Xunit;

namespace Spidey.Tests.Engines
{
    public class DefaultLinkEngineTests
    {
        [Fact]
        public void DiscoverUrls()
        {
            var Result = new DefaultLinkDiscoverer().DiscoverUrls("http://google.com", "http://google.com", "<a href=\"/Temp.html\">ASDF</a><a href=\"/Temp2.html\"></a>".ToByteArray(), "TEXT/HTML", Options.Default);
            Assert.Equal(2, Result.Length);
            Assert.Equal("http://google.com/Temp.html", Result[0]);
            Assert.Equal("http://google.com/Temp2.html", Result[1]);
        }

        [Fact]
        public void FixUrl()
        {
            var Result = new DefaultLinkDiscoverer().FixUrl("http://google.com", "/something-something/dark-side/", new Dictionary<Regex, string>
            {
                [new Regex("something")] = "blah"
            });
            Assert.Equal("http://google.com/blah-blah/dark-side", Result);
        }
    }
}