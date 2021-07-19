using Microsoft.Extensions.Logging;
using Spidey.Engines.Interfaces;
using Spidey.Engines.Scheduler;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Spidey.Engines
{
    /// <summary>
    /// Default scheduler
    /// </summary>
    /// <seealso cref="IScheduler"/>
    public class DefaultScheduler : IScheduler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultScheduler"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="engines">The engines.</param>
        /// <param name="logger">The logger.</param>
        public DefaultScheduler(Options? options, IEnumerable<IEngine> engines, ILogger<DefaultScheduler>? logger = null)
        {
            options = (options ?? Options.Default).Setup();
            Logger = logger;
            var Engine = engines.FirstOrDefault(x => !(x is DefaultEngine)) ?? engines.FirstOrDefault(x => x is DefaultEngine);
            WorkerPool = new WorkerPool(options.NumberWorkers, Engine, new CancellationToken());
        }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        /// <value>The logger.</value>
        private ILogger<DefaultScheduler>? Logger { get; }

        /// <summary>
        /// Gets the worker pool.
        /// </summary>
        /// <value>The worker pool.</value>
        private WorkerPool? WorkerPool { get; set; }

        /// <summary>
        /// Schedules the specified URL for crawling.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns>This.</returns>
        public Task<UrlData?> CrawlAsync(string url)
        {
            if (string.IsNullOrEmpty(url))
                return Task.FromResult<UrlData?>(null);
            Logger?.LogDebug($"Scheduling {url}");
            return WorkerPool?.CrawlAsync(url) ?? Task.FromResult<UrlData?>(null);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting
        /// unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            WorkerPool?.Dispose();
            WorkerPool = null;
        }
    }
}