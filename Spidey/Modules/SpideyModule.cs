using Canister.Interfaces;

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
        public int Order
        {
            get { return 0; }
        }

        /// <summary>
        /// Loads the module
        /// </summary>
        /// <param name="bootstrapper">The bootstrapper.</param>
        public void Load(IBootstrapper bootstrapper)
        {
            bootstrapper?.Register<Crawler>()
                .Register(Options.Default);
        }
    }
}