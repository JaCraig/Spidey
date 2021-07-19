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
    /// Default pipeline
    /// </summary>
    /// <seealso cref="IPipeline"/>
    public class DefaultPipeline : IPipeline
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultPipeline"/> class.
        /// </summary>
        /// <param name="schedulers">The schedulers.</param>
        /// <param name="processors">The processors.</param>
        /// <param name="parsers">The parsers.</param>
        /// <param name="linkDiscoverers">The link discoverers.</param>
        /// <param name="options">The options.</param>
        /// <param name="logger">The logger.</param>
        public DefaultPipeline(IEnumerable<IScheduler> schedulers, IEnumerable<IProcessor> processors, IEnumerable<IContentParser> parsers, IEnumerable<ILinkDiscoverer> linkDiscoverers, Options? options, ILogger<DefaultPipeline>? logger = null)
        {
            Scheduler = schedulers.FirstOrDefault(x => !(x is DefaultScheduler)) ?? schedulers.FirstOrDefault(x => x is DefaultScheduler);
            Processor = processors.FirstOrDefault(x => !(x is DefaultProcessor)) ?? processors.FirstOrDefault(x => x is DefaultProcessor);
            Parser = parsers.FirstOrDefault(x => !(x is DefaultContentParser)) ?? parsers.FirstOrDefault(x => x is DefaultContentParser);
            LinkDiscoverer = linkDiscoverers.FirstOrDefault(x => !(x is DefaultLinkDiscoverer)) ?? linkDiscoverers.FirstOrDefault(x => x is DefaultLinkDiscoverer);
            Logger = logger;
            Options = (options ?? Options.Default).Setup();
        }

        /// <summary>
        /// Gets or sets the link discoverer.
        /// </summary>
        /// <value>The link discoverer.</value>
        private ILinkDiscoverer LinkDiscoverer { get; }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        /// <value>The logger.</value>
        private ILogger<DefaultPipeline>? Logger { get; }

        /// <summary>
        /// Gets the options.
        /// </summary>
        /// <value>The options.</value>
        private Options Options { get; }

        /// <summary>
        /// Gets or sets the parser.
        /// </summary>
        /// <value>The parser.</value>
        private IContentParser Parser { get; }

        /// <summary>
        /// Gets the processor.
        /// </summary>
        /// <value>The processor.</value>
        private IProcessor Processor { get; }

        /// <summary>
        /// Gets or sets the results.
        /// </summary>
        /// <value>The results.</value>
        private Results? Results { get; set; }

        /// <summary>
        /// Gets the scheduler.
        /// </summary>
        /// <value>The scheduler.</value>
        private IScheduler? Scheduler { get; set; }

        /// <summary>
        /// Gets the urls.
        /// </summary>
        /// <value>The urls.</value>
        private BlockingCollection<string> Urls { get; } = new BlockingCollection<string>(new ConcurrentQueue<string>());

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting
        /// unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Scheduler?.Dispose();
            Scheduler = null;
        }

        /// <summary>
        /// Starts the crawl asynchronous.
        /// </summary>
        /// <returns>The results from the crawl.</returns>
        public async Task<Results?> StartCrawlAsync()
        {
            Logger?.LogInformation("Beginning crawl");
            Results = new Results();
            if (Options.StartLocations.Count == 0)
                return Results;
            for (var i = 0; i < Options.StartLocations.Count; i++)
            {
                Urls.Add(Options.StartLocations[i]);
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
        /// Crawls the url asynchronous.
        /// </summary>
        /// <param name="url">The URL.</param>
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

                ProcessDocument(Parser.Parse(Result));
                FindUrls(url, Result.Content, Result.ContentType);
            }
            catch (Exception ex) { HandleError(ex, url); }
        }

        /// <summary>
        /// Crawls the urls.
        /// </summary>
        /// <param name="tasks">The tasks.</param>
        private void CrawlUrls(List<Task> tasks)
        {
            while (Urls.TryTake(out var url, 1000))
            {
                tasks.Add(CrawlAsync(url));
            }
        }

        /// <summary>
        /// Finds the urls.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="content">The content.</param>
        /// <param name="contentType">Type of the content.</param>
        private void FindUrls(string url, byte[] content, string contentType)
        {
            if (!Options.CanFollow(url))
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
        /// Handles the error.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="url">The URL.</param>
        private void HandleError(Exception exception, string url)
        {
            var TempException = exception as WebException;
            var TempResponse = TempException?.Response as HttpWebResponse;
            Logger?.LogError(exception, "An error has occurred");
            Results?.ErrorURLs.Add(new ErrorItem(exception, url, ((int?)TempResponse?.StatusCode) ?? 0));
        }

        /// <summary>
        /// Adds the document.
        /// </summary>
        /// <param name="resultFile">The result file.</param>
        private void ProcessDocument(ResultFile? resultFile)
        {
            if (resultFile is null)
                return;
            Processor.Process(resultFile);
        }
    }
}