using Microsoft.Extensions.Logging;
using Spidey.Engines.Interfaces;

namespace Spidey.Engines
{
    /// <summary>
    /// Default <see cref="IProcessor"/> implementation used to forward discovered results to the
    /// configured <see cref="Options.ItemFound"/> callback.
    /// </summary>
    /// <remarks>
    /// The processor is intentionally lightweight and stateless aside from the supplied options and
    /// logger. A <paramref name="resultFile"/> value of <see langword="null"/> is ignored.
    /// </remarks>
    /// <seealso cref="IProcessor"/>
    public class DefaultProcessor(Options? options, ILogger<DefaultProcessor>? logger = null) : IProcessor
    {
        /// <summary>
        /// Gets the logger used for diagnostic output.
        /// </summary>
        private ILogger<DefaultProcessor>? Logger { get; } = logger;

        /// <summary>
        /// Gets the configured options, falling back to <see cref="Options.Default"/> when no
        /// options instance is supplied.
        /// </summary>
        private Options Options { get; } = (options ?? Options.Default).Setup();

        /// <summary>
        /// Processes a discovered result by logging it and invoking the configured callback.
        /// </summary>
        /// <param name="resultFile">The discovered result to process. Null values are ignored.</param>
        /// <remarks>
        /// This method does not throw for <see langword="null"/> input. Any exception behavior from
        /// <see cref="Options.ItemFound"/> is delegated to the configured callback.
        /// </remarks>
        public void Process(ResultFile resultFile)
        {
            if (resultFile is null)
                return;

            Logger?.LogDebug("Processing {location}", resultFile.Location);
            Options.ItemFound(resultFile);
        }
    }
}