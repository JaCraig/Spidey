using Canister.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IO;
using Spidey.Engines.Interfaces;

namespace Spidey.Modules
{
    /// <summary>
    /// Spidey module
    /// </summary>
    /// <seealso cref="IModule"/>
    public class SpideyModule : IModule
    {
        /// <summary>
        /// Order to run it in
        /// </summary>
        public int Order { get; } = int.MinValue;

        /// <summary>
        /// Loads the module
        /// </summary>
        /// <param name="bootstrapper">The bootstrapper.</param>
        public void Load(IBootstrapper? bootstrapper)
        {
            bootstrapper?.Register<Crawler>()
                .RegisterAll<ILinkDiscoverer>()
                .RegisterAll<IEngine>()
                .RegisterAll<IContentParser>()
                .RegisterAll<IProcessor>()
                .RegisterAll<IPipeline>()
                .RegisterAll<IScheduler>()
                .Register(Options.Default)
                .Register<RecyclableMemoryStreamManager>(ServiceLifetime.Singleton);
        }
    }
}