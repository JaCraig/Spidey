/*
Copyright 2017 James Craig

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using BigBook;
using Serilog;
using Spidey.Engines;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Spidey
{
    /// <summary>
    /// Crawler class
    /// </summary>
    /// <seealso cref="IDisposable"/>
    public class Crawler : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Crawler"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="logger">The logger.</param>
        /// <exception cref="ArgumentNullException">logger</exception>
        public Crawler(Options options, ILogger logger)
        {
            Options = options ?? Options.Default;
            Options.Engine ??= new DefaultEngine();
            Options.ItemFound ??= DefaultItemFound;
            Logger = logger ?? Log.Logger ?? new LoggerConfiguration().CreateLogger() ?? throw new ArgumentNullException(nameof(logger));
            Options.Setup();
            WhereFound = new ListMapping<string, string>();
            URLs = new TaskQueue<string>(Environment.ProcessorCount,
                x => Crawl(x).GetAwaiter().GetResult(),
                100,
                (x, y) =>
                {
                    var TempException = x as WebException;
                    var TempResponse = TempException?.Response as HttpWebResponse;
                    Logger.Error(x, "An error has occurred");
                    ErrorURLs.Add(new ErrorItem(x, y, ((int?)TempResponse?.StatusCode) ?? 0));
                });
            CompletedURLs = new ConcurrentBag<string>();
            ErrorURLs = new ConcurrentBag<ErrorItem>();
            Options.LinkDiscoverer ??= new DefaultLinkDiscoverer();
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="Crawler"/> is done.
        /// </summary>
        /// <value><c>true</c> if done; otherwise, <c>false</c>.</value>
        public bool Done => URLs?.IsComplete == true;

        /// <summary>
        /// Gets or sets the error ur ls.
        /// </summary>
        /// <value>The error ur ls.</value>
        public ConcurrentBag<ErrorItem> ErrorURLs { get; }

        /// <summary>
        /// Gets the options.
        /// </summary>
        /// <value>The options.</value>
        public Options Options { get; }

        /// <summary>
        /// Gets or sets the completed urls.
        /// </summary>
        /// <value>The completed urls.</value>
        private ConcurrentBag<string> CompletedURLs { get; }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        /// <value>The logger.</value>
        private ILogger Logger { get; }

        /// <summary>
        /// Gets or sets the urls.
        /// </summary>
        /// <value>The urls.</value>
        private TaskQueue<string>? URLs { get; set; }

        /// <summary>
        /// Gets or sets the where found.
        /// </summary>
        /// <value>The where found.</value>
        private ListMapping<string, string> WhereFound { get; }

        /// <summary>
        /// Crawls the specified URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns>The task.</returns>
        public async Task<bool> Crawl(string url)
        {
            if (CompletedURLs.Contains(url) || !Options.CanCrawl(url))
                return true;
            Logger.Debug("Crawling " + url);
            CompletedURLs.Add(url);

            var Result = await Options.Engine.CrawlAsync(url, Options).ConfigureAwait(false);

            Thread.Sleep(new Random()?.Next(Options.MinDelay, Options.MaxDelay) ?? 0);

            AddDocument(Options.Parser.Parse(Options, Result));
            AddUrls(url, Result.Content, Result.ContentType);
            return true;
        }

        /// <summary>
        /// Disposes of the internal objects
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Starts crawling.
        /// </summary>
        /// <returns>The listing of each URL and where it was found.</returns>
        public ListMapping<string, string> StartCrawl()
        {
            if (Options.StartLocations.Count == 0)
                return WhereFound;
            for (var i = 0; i < Options.StartLocations.Count; i++)
            {
                URLs?.Enqueue(Options.StartLocations[i]);
            }
            while (!Done) Thread.Sleep(100);
            return WhereFound;
        }

        /// <summary>
        /// Disposes the internal objects
        /// </summary>
        /// <param name="Value"></param>
        protected virtual void Dispose(bool Value)
        {
            URLs?.Dispose();
            URLs = null;
        }

        /// <summary>
        /// Defaults the item found.
        /// </summary>
        /// <param name="_">The .</param>
        private static void DefaultItemFound(ResultFile _) { }

        /// <summary>
        /// Adds the document.
        /// </summary>
        /// <param name="file">The file.</param>
        private void AddDocument(ResultFile? file)
        {
            if (file == null)
                return;
            Options.ItemFound(file);
        }

        /// <summary>
        /// Adds the urls.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="content">The content.</param>
        /// <param name="contentType">Type of the content.</param>
        private void AddUrls(string url, byte[] content, string contentType)
        {
            if (!Options.CanFollow(url))
                return;
            var CurrentDomain = Options.LinkDiscoverer.GetDomain(url);
            foreach (var Link in Options.LinkDiscoverer.DiscoverUrls(CurrentDomain, url, content, contentType, Options))
            {
                URLs?.Enqueue(Link);
                if (Options.CanCrawl(Link))
                    WhereFound.Add(Link, url);
            }
        }
    }
}