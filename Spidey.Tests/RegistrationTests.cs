using Canister.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace Spidey.Tests
{
    public class RegistrationTests
    {
        [Fact]
        public void RegisterSpidey_WithICanisterConfiguration_DoesNotThrow()
        {
            var Config = Substitute.For<ICanisterConfiguration>();
            var Result = RegistrationExtension.RegisterSpidey(Config);
            Assert.NotNull(Result);
        }

        [Fact]
        public void RegisterSpidey_WithIServiceCollection_DoesNotThrow()
        {
            var Services = Substitute.For<IServiceCollection>();
            var Result = RegistrationExtension.RegisterSpidey(Services);
            Assert.NotNull(Result);
        }
    }
}