using Spidey;
using Xunit;

namespace Spidey.Tests
{
    public class OptionsTests
    {
        [Fact]
        public void Default_Properties_AreNotNull()
        {
            // Arrange & Act
            var Options = Spidey.Options.Default;

            // Assert
            Assert.NotNull(Options.Allow);
            Assert.NotNull(Options.FollowOnly);
            Assert.NotNull(Options.Ignore);
            Assert.NotNull(Options.ItemFound);
            Assert.NotNull(Options.StartLocations);
            Assert.NotNull(Options.UrlReplacements);
        }

        [Fact]
        public void NumberWorkers_Default_IsGreaterThanZero()
        {
            // Arrange & Act
            var Options = Spidey.Options.Default;

            // Assert
            Assert.True(Options.NumberWorkers > 0);
        }

        [Fact]
        public void Can_Set_And_Get_Credentials()
        {
            // Arrange
            var Options = new Options();
            var Creds = new System.Net.NetworkCredential("user", "pass");

            // Act
            Options.Credentials = Creds;

            // Assert
            Assert.Equal(Creds, Options.Credentials);
        }

        [Fact]
        public void Can_Set_And_Get_Proxy()
        {
            // Arrange
            var Options = new Options();
            var Proxy = new System.Net.WebProxy();

            // Act
            Options.Proxy = Proxy;

            // Assert
            Assert.Equal(Proxy, Options.Proxy);
        }
    }
}