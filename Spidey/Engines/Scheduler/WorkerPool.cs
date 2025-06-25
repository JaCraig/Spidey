using Spidey.Engines.Interfaces;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Spidey.Engines.Scheduler
{
    /// <summary>
    /// Worker pool
    /// </summary>
    /// <seealso cref="IDisposable"/>
    public class WorkerPool : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkerPool"/> class.
        /// </summary>
        /// <param name="workerCount">The worker count.</param>
        /// <param name="engine">The engine.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public WorkerPool(int workerCount, IEngine? engine, CancellationToken cancellationToken)
        {
            Workers = new Worker[workerCount];
            for (int X = 0; X < workerCount; ++X)
            {
                Workers[X] = new Worker(engine);
            }
            CancellationToken = cancellationToken;
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="WorkerPool"/> is done.
        /// </summary>
        /// <value><c>true</c> if done; otherwise, <c>false</c>.</value>
        public bool Done => Workers?.All(x => x?.Done ?? true) ?? true;

        /// <summary>
        /// Gets a value indicating whether this instance is canceled.
        /// </summary>
        /// <value><c>true</c> if this instance is canceled; otherwise, <c>false</c>.</value>
        public bool IsCanceled => CancellationToken.IsCancellationRequested;

        /// <summary>
        /// Gets the cancellation token.
        /// </summary>
        /// <value>The cancellation token.</value>
        private CancellationToken CancellationToken { get; }

        /// <summary>
        /// Gets or sets the workers.
        /// </summary>
        /// <value>The workers.</value>
        private Worker[]? Workers { get; set; }

        /// <summary>
        /// Processes the request.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns>Task.</returns>
        public Task<UrlData?> CrawlAsync(string url)
        {
            if (IsCanceled || Workers is null)
                return Task.FromResult<UrlData?>(null);
            var ConnectionToUse = Task.WaitAny(Workers.Select(x => x.CurrentTask).ToArray());
            return Workers[ConnectionToUse].CrawlAsync(url);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting
        /// unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (Workers is null)
                return;
            foreach (var Worker in Workers)
            {
                Worker.Dispose();
            }
            Workers = null;

            GC.SuppressFinalize(this);
        }
    }
}