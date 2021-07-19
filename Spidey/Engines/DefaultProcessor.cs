using Microsoft.Extensions.Logging;
using Spidey.Engines.Interfaces;

namespace Spidey.Engines
{
    /// <summary>
    /// Default processor
    /// </summary>
    /// <seealso cref="IProcessor"/>
    public class DefaultProcessor : IProcessor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultProcessor"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="logger">The logger.</param>
        public DefaultProcessor(Options? options, ILogger<DefaultProcessor>? logger = null)
        {
            Options = (options ?? Options.Default).Setup();
            Logger = logger;
        }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        /// <value>The logger.</value>
        private ILogger<DefaultProcessor>? Logger { get; }

        /// <summary>
        /// Gets the options.
        /// </summary>
        /// <value>The options.</value>
        private Options Options { get; }

        /// <summary>
        /// Processes the item found.
        /// </summary>
        /// <param name="resultFile">The result file.</param>
        public void Process(ResultFile resultFile)
        {
            if (resultFile is null)
                return;
            Logger?.LogDebug($"Processing {resultFile.Location}");
            Options.ItemFound(resultFile);
        }
    }
}