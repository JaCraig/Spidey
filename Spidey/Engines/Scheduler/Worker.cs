using Spidey.Engines.Interfaces;
using System;
using System.Threading.Tasks;

namespace Spidey.Engines.Scheduler
{
    /// <summary>
    /// Worker class
    /// </summary>
    /// <seealso cref="IDisposable"/>
    /// <remarks>
    /// Initializes a new instance of the <see cref="Worker"/> class.
    /// </remarks>
    /// <param name="engine">The engine.</param>
    public class Worker(IEngine? engine) : IDisposable
    {

        /// <summary>
        /// Gets or sets the current task.
        /// </summary>
        /// <value>The current task.</value>
        public Task CurrentTask { get; private set; } = Task.CompletedTask;

        /// <summary>
        /// Gets a value indicating whether this <see cref="Worker"/> is done.
        /// </summary>
        /// <value><c>true</c> if done; otherwise, <c>false</c>.</value>
        public bool Done => CurrentTask?.IsCompleted ?? true;

        /// <summary>
        /// Gets the engine.
        /// </summary>
        /// <value>The engine.</value>
        private IEngine? Engine { get; set; } = engine;

        /// <summary>
        /// Crawls the url asynchronously.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns>The url data.</returns>
        public Task<UrlData?> CrawlAsync(string url)
        {
            var ReturnValue = Engine?.CrawlAsync(url) ?? Task.FromResult<UrlData?>(null);
            CurrentTask = ReturnValue;
            return ReturnValue;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting
        /// unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Engine?.Dispose();
            Engine = null;

            GC.SuppressFinalize(this);
        }
    }
}