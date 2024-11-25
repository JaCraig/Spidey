using Canister.Interfaces;
using Microsoft.Extensions.DependencyInjection;

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
        public void Load(IServiceCollection? bootstrapper) => bootstrapper?.RegisterSpidey();
    }
}