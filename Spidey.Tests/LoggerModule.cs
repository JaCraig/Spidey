using Canister.Interfaces;

namespace Spidey.Tests
{
    public class LoggerModule : IModule
    {
        /// <summary>
        /// Order to run it in
        /// </summary>
        public int Order
        {
            get { return 1000; }
        }

        /// <summary>
        /// Loads the module
        /// </summary>
        /// <param name="bootstrapper">The bootstrapper.</param>
        public void Load(IBootstrapper bootstrapper)
        {
            bootstrapper
                ?.Register(new Options
                {
                    Allow = { "https://ajax.googleapis.com/ajax/libs/jquery/3.4.1/jquery.min.js" },
                    StartLocations = { "https://ajax.googleapis.com/ajax/libs/jquery/3.4.1/jquery.min.js" },
                });
        }
    }
}