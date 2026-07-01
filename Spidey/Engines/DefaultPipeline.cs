using Microsoft.Extensions.Logging;
using Spidey.Engines.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Spidey.Engines
{
    /// <summary>
    /// Default implementation of <see cref="IPipeline"/> that coordinates crawling, parsing,
    /// processing, and link discovery.
    /// </summary>
    /// <remarks>
    /// The pipeline uses a shared <see cref="BlockingCollection{T}"/> to stage discovered URLs and
    /// repeatedly drains that queue until no additional URLs are produced. Work is started eagerly
    /// and awaited in batches; no cancellation support is implemented here.
    /// </remarks>
    /// <seealso cref="IPipeline"/>
    /// <param name="schedulers">Available crawlers used to fetch URL content.</param>
    /// <param name="processors">Available document processors used after parsing.</param>
    /// <param name="parsers">Available content parsers used to convert crawler output into documents.</param>
    /// <param name="linkDiscoverers">
    /// Available link discoverers used to extract follow-up URLs from crawled content.
    /// </param>
    /// <param name="options">
    /// Pipeline configuration; <see cref="Options.Default"/> is used when null.
    /// </param>
    /// <param name="logger">Optional logger used for crawl lifecycle and error reporting.</param>
    public class DefaultPipeline(IEnumerable<IScheduler> schedulers, IEnumerable<IProcessor> processors, IEnumerable<IContentParser> parsers, IEnumerable<ILinkDiscoverer> linkDiscoverers, Options? options, ILogger<DefaultPipeline>? logger = null) : IPipeline
    {
        /// <summary>
        /// Gets the preferred link discoverer.
        /// </summary>
        /// <remarks>
        /// A non-default implementation is preferred when available; otherwise the default
        /// implementation is used. The value can be null when no discoverers are registered.
        /// </remarks>
        private ILinkDiscoverer? LinkDiscoverer { get; } = linkDiscoverers.FirstOrDefault(x => x is not DefaultLinkDiscoverer) ?? linkDiscoverers.FirstOrDefault(x => x is DefaultLinkDiscoverer);

        /// <summary>
        /// Gets the logger used for informational and error messages.
        /// </summary>
        private ILogger<DefaultPipeline>? Logger { get; } = logger;

        /// <summary>
        /// Gets the normalized pipeline options.
        /// </summary>
        /// <remarks>
        /// The options instance is initialized once and passed through <see cref="Options.Setup"/>
        /// before use.
        /// </remarks>
        private Options Options { get; } = (options ?? Options.Default).Setup();

        /// <summary>
        /// Gets the preferred content parser.
        /// </summary>
        /// <remarks>
        /// A non-default implementation is preferred when available; otherwise the default
        /// implementation is used. The value can be null when no parsers are registered.
        /// </remarks>
        private IContentParser? Parser { get; } = parsers.FirstOrDefault(x => x is not DefaultContentParser) ?? parsers.FirstOrDefault(x => x is DefaultContentParser);

        /// <summary>
        /// Gets the preferred processor.
        /// </summary>
        /// <remarks>
        /// A non-default implementation is preferred when available; otherwise the default
        /// implementation is used. The value can be null when no processors are registered.
        /// </remarks>
        private IProcessor? Processor { get; } = processors.FirstOrDefault(x => x is not DefaultProcessor) ?? processors.FirstOrDefault(x => x is DefaultProcessor);

        /// <summary>
        /// Gets the crawl results accumulated during the run.
        /// </summary>
        private Results? Results { get; set; }

        /// <summary>
        /// Gets the preferred scheduler.
        /// </summary>
        /// <remarks>
        /// A non-default implementation is preferred when available; otherwise the default
        /// implementation is used. The value can be null when no schedulers are registered.
        /// </remarks>
        private IScheduler? Scheduler { get; set; } = schedulers.FirstOrDefault(x => x is not DefaultScheduler) ?? schedulers.FirstOrDefault(x => x is DefaultScheduler);

        /// <summary>
        /// Gets the shared queue of URLs pending crawl.
        /// </summary>
        /// <remarks>
        /// This collection is used as the work queue for discovered URLs. It is not bounded.
        /// </remarks>
        private BlockingCollection<string> Urls { get; } = new BlockingCollection<string>(new ConcurrentQueue<string>());

        /// <summary>
        /// Releases the selected scheduler and suppresses finalization.
        /// </summary>
        public void Dispose()
        {
            Scheduler?.Dispose();
            Scheduler = null;
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Starts a crawl operation and returns the collected results.
        /// </summary>
        /// <returns>
        /// A <see cref="Task{TResult}"/> that completes with the crawl <see cref="Results"/>, or an
        /// empty results object when no start locations are configured.
        /// </returns>
        /// <remarks>
        /// Start locations are queued before crawl execution begins. The crawl continues until the
        /// work queue is empty and all scheduled crawl tasks from the current batch have completed.
        /// </remarks>
        public async Task<Results?> StartCrawlAsync()
        {
            Logger?.LogInformation("Beginning crawl");
            Results = new Results();
            if (Options.StartLocations.Count == 0)
                return Results;
            for (var I = 0; I < Options.StartLocations.Count; I++)
            {
                Urls.Add(Options.StartLocations[I]);
            }
            var Tasks = new List<Task>();
            do
            {
                CrawlUrls(Tasks);
                await Task.WhenAll(Tasks).ConfigureAwait(false);
            }
            while (Urls.Count != 0);
            await Task.WhenAll(Tasks).ConfigureAwait(false);
            return Results;
        }

        /// <summary>
        /// Crawls a single URL, parses the response, processes the document, and enqueues
        /// discovered links.
        /// </summary>
        /// <param name="url">The URL to crawl.</param>
        /// <remarks>
        /// URLs are skipped when the scheduler is unavailable, the URL has already completed, or
        /// the current options disallow crawling it. Exceptions are logged and added to <see cref="Results"/>.
        /// </remarks>
        private async Task CrawlAsync(string url)
        {
            try
            {
                if (Scheduler is null || (Results?.CompletedURLs.Contains(url) ?? false) || !Options.CanCrawl(url))
                    return;
                Results?.CompletedURLs.Add(url);

                var Result = await Scheduler.CrawlAsync(url).ConfigureAwait(false);
                if (Result is null)
                    return;

                if (Options.MinDelay > 0 || Options.MaxDelay > 0)
                    Thread.Sleep(new Random()?.Next(Options.MinDelay, Options.MaxDelay) ?? 0);

                ProcessDocument(Parser?.Parse(Result));
                FindUrls(url, Result.Content, Result.ContentType);
            }
            catch (Exception Ex) { HandleError(Ex, url); }
        }

        /// <summary>
        /// Drains queued URLs into the current batch of crawl tasks.
        /// </summary>
        /// <param name="tasks">The active crawl task collection for the current batch.</param>
        /// <remarks>
        /// URLs are removed from the queue before crawl tasks are started. A short timeout is used
        /// so the loop does not spin indefinitely when the queue is temporarily empty.
        /// </remarks>
        private void CrawlUrls(List<Task> tasks)
        {
            while (Urls.TryTake(out var Url, 1000))
            {
                tasks.Add(CrawlAsync(Url));
            }
        }

        /// <summary>
        /// Discovers links from crawled content and enqueues them for later processing.
        /// </summary>
        /// <param name="url">The source URL that produced the content.</param>
        /// <param name="content">The response body used for link discovery.</param>
        /// <param name="contentType">The reported response content type.</param>
        /// <remarks>
        /// Newly discovered URLs are always enqueued. Links that are allowed by the pipeline
        /// options are also recorded in <see cref="Results.WhereFound"/> with their source URL.
        /// </remarks>
        private void FindUrls(string url, byte[] content, string contentType)
        {
            if (!Options.CanFollow(url) || LinkDiscoverer is null)
                return;
            var CurrentDomain = LinkDiscoverer.GetDomain(url);
            foreach (var Link in LinkDiscoverer.DiscoverUrls(CurrentDomain, url, content, contentType))
            {
                Urls.Add(Link);
                if (Options.CanCrawl(Link))
                    Results?.WhereFound.Add(Link, url);
            }
        }

        /// <summary>
        /// Records crawl failures and adds them to the accumulated results.
        /// </summary>
        /// <param name="exception">The exception thrown during crawling or processing.</param>
        /// <param name="url">The URL associated with the failure.</param>
        /// <remarks>
        /// If the exception is a <see cref="WebException"/> with an HTTP response, the status code
        /// is captured; otherwise the status code is recorded as 0.
        /// </remarks>
        private void HandleError(Exception exception, string url)
        {
            var TempException = exception as WebException;
            var TempResponse = TempException?.Response as HttpWebResponse;
            Logger?.LogError(exception, "An error has occurred");
            Results?.ErrorURLs.Add(new ErrorItem(exception, url, ((int?)TempResponse?.StatusCode) ?? 0));
        }

        /// <summary>
        /// Parses a crawl result and passes it to the configured processor.
        /// </summary>
        /// <param name="resultFile">The parsed result document.</param>
        /// <remarks>No processing occurs when either the parsed result or processor is unavailable.</remarks>
        private void ProcessDocument(ResultFile? resultFile)
        {
            if (resultFile is null || Processor is null)
                return;
            Processor.Process(resultFile);
        }
    }
}