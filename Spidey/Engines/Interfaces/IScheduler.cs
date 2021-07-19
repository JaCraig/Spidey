using System;
using System.Threading.Tasks;

namespace Spidey.Engines.Interfaces
{
    /// <summary>
    /// Schedules the individual URLs to the workers.
    /// </summary>
    /// <seealso cref="IDisposable"/>
    public interface IScheduler : IDisposable
    {
        /// <summary>
        /// Schedules the specified URL for crawling.
        /// </summary>
        /// <param name="url">The URL.</param>
        Task<UrlData?> CrawlAsync(string url);
    }
}