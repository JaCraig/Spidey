using System;
using System.Threading.Tasks;

namespace Spidey.Engines.Interfaces
{
    /// <summary>
    /// Pipeline interface
    /// </summary>
    /// <seealso cref="IDisposable"/>
    public interface IPipeline : IDisposable
    {
        /// <summary>
        /// Starts the crawl asynchronous.
        /// </summary>
        /// <returns>The results from the crawl.</returns>
        Task<Results?> StartCrawlAsync();
    }
}