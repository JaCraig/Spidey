using Microsoft.Extensions.Logging;
using Spidey.Engines.Interfaces;
using Spidey.Engines.Scheduler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Spidey.Engines
{
    /// <summary>
    /// Default <see cref="IScheduler"/> implementation that selects an engine and forwards crawl
    /// requests to a worker pool.
    /// </summary>
    /// <remarks>
    /// The scheduler prefers the first engine that is not <see cref="DefaultEngine"/> and falls
    /// back to <see cref="DefaultEngine"/> if no alternative engine is available. The selected
    /// engine is captured once during construction and the scheduler does not mutate its worker
    /// pool after initialization.
    /// </remarks>
    /// <seealso cref="IScheduler"/>
    public class DefaultScheduler : IScheduler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultScheduler"/> class.
        /// </summary>
        /// <param name="options">
        /// Scheduler options. When <see langword="null"/>, <see cref="Options.Default"/> is used.
        /// </param>
        /// <param name="engines">Available engines used to construct the worker pool.</param>
        /// <param name="logger">Optional logger used to emit debug-level scheduling messages.</param>
        /// <remarks>
        /// The worker count is derived from the configured options. Engine selection is performed
        /// eagerly and the resulting engine instance is passed to the worker pool. A fresh <see
        /// cref="CancellationToken"/> is created for the worker pool and is not exposed by this scheduler.
        /// </remarks>
        public DefaultScheduler(Options? options, IEnumerable<IEngine> engines, ILogger<DefaultScheduler>? logger = null)
        {
            options = (options ?? Options.Default).Setup();
            Logger = logger;
            var Engine = engines.FirstOrDefault(x => x is not DefaultEngine) ?? engines.FirstOrDefault(x => x is DefaultEngine);
            WorkerPool = new WorkerPool(options.NumberWorkers, Engine, new CancellationToken());
        }

        /// <summary>
        /// Gets the logger instance used for diagnostic output.
        /// </summary>
        private ILogger<DefaultScheduler>? Logger { get; }

        /// <summary>
        /// Gets or sets the worker pool that executes crawl requests.
        /// </summary>
        /// <remarks>
        /// The worker pool is created during construction and disposed when the scheduler is disposed.
        /// </remarks>
        private WorkerPool? WorkerPool { get; set; }

        /// <summary>
        /// Schedules the specified URL for crawling.
        /// </summary>
        /// <param name="url">The URL to crawl.</param>
        /// <returns>
        /// A task that completes with the crawled <see cref="UrlData"/> result, or <see
        /// langword="null"/> when the input URL is empty or the scheduler is unavailable.
        /// </returns>
        /// <remarks>
        /// Empty or <see langword="null"/> URLs are rejected without contacting the worker pool.
        /// The method is otherwise a thin pass-through to <see
        /// cref="WorkerPool.CrawlAsync(string)"/> and preserves the worker pool's asynchronous
        /// execution behavior.
        /// </remarks>
        public Task<UrlData?> CrawlAsync(string url)
        {
            if (string.IsNullOrEmpty(url))
                return Task.FromResult<UrlData?>(null);
            Logger?.LogDebug("Scheduling {url}", url);
            return WorkerPool?.CrawlAsync(url) ?? Task.FromResult<UrlData?>(null);
        }

        /// <summary>
        /// Releases the worker pool and suppresses finalization.
        /// </summary>
        /// <remarks>
        /// After disposal, subsequent crawl requests return <see langword="null"/> results because
        /// the worker pool reference is cleared. Disposal is idempotent.
        /// </remarks>
        public void Dispose()
        {
            WorkerPool?.Dispose();
            WorkerPool = null;
            GC.SuppressFinalize(this);
        }
    }
}