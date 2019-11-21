using Microsoft.Extensions.DependencyInjection;
using Spidey.Registration;
using System.Reflection;
using Xunit;

namespace Spidey.Tests.BaseClasses
{
    /// <summary>
    /// Test base class
    /// </summary>
    /// <seealso cref="System.IDisposable"/>
    [Collection("TestCollection")]
    public class TestBaseClass
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestBaseClass"/> class.
        /// </summary>
        public TestBaseClass()
        {
            if (Canister.Builder.Bootstrapper == null)
            {
                Canister.Builder.CreateContainer(new ServiceCollection())
                    .AddAssembly(typeof(TestBaseClass).GetTypeInfo().Assembly)
                    .RegisterSpidey()
                    .Build();
            }
        }
    }
}