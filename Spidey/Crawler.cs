﻿/*
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
using FileCurator;
using Serilog;
using Spidey.Engines;
using Spidey.Engines.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
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
        /// <param name="itemFound">The item found.</param>
        /// <param name="options">The options.</param>
        /// <param name="engine">The engine.</param>
        /// <param name="logger">The logger.</param>
        /// <exception cref="ArgumentNullException">logger</exception>
        public Crawler(Action<ResultFile> itemFound, Options options, IEngine engine, ILogger logger)
        {
            Engine = engine ?? new DefaultEngine();
            ItemFound = itemFound;
            Logger = logger ?? Log.Logger ?? new LoggerConfiguration().CreateLogger() ?? throw new ArgumentNullException(nameof(logger));
            Options = options ?? Options.Default;
            WhereFound = new ListMapping<string, string>();
            URLs = new TaskQueue<string>(Environment.ProcessorCount,
                x => Crawl(x).GetAwaiter().GetResult(),
                100,
                (x, y) =>
                {
                    var TempException = x as WebException;
                    var TempResponse = TempException?.Response as HttpWebResponse;
                    Logger.Error(x, "An error has occurred");
                    ErrorURLs.Add(new ErrorItem { Error = x, Url = y, StatusCode = ((int?)TempResponse?.StatusCode) ?? 0 });
                });
            CompletedURLs = new ConcurrentBag<string>();
            ErrorURLs = new ConcurrentBag<ErrorItem>();
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="Crawler"/> is done.
        /// </summary>
        /// <value><c>true</c> if done; otherwise, <c>false</c>.</value>
        public bool Done => URLs.IsComplete;

        /// <summary>
        /// Gets the engine.
        /// </summary>
        /// <value>The engine.</value>
        public IEngine Engine { get; }

        /// <summary>
        /// Gets or sets the error ur ls.
        /// </summary>
        /// <value>The error ur ls.</value>
        public ConcurrentBag<ErrorItem> ErrorURLs { get; }

        /// <summary>
        /// Gets the item found.
        /// </summary>
        /// <value>The item found.</value>
        public Action<ResultFile> ItemFound { get; }

        /// <summary>
        /// Gets the options.
        /// </summary>
        /// <value>The options.</value>
        public Options Options { get; }

        /// <summary>
        /// Gets the scheme regex.
        /// </summary>
        /// <value>The scheme regex.</value>
        private static Regex SchemeRegex { get; } = new Regex("^(?<scheme>[^:]*://)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

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
        private TaskQueue<string> URLs { get; set; }

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
            if (CompletedURLs.Contains(url) || !CanCrawl(url))
                return true;
            Logger.Debug("Crawling " + url);
            CompletedURLs.Add(url);

            var Result = await Engine.CrawlAsync(url, Options).ConfigureAwait(false);

            AddDocument(Parse(Result));
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
            Options.StartLocations.ForEach(x => URLs.Enqueue(x));
            while (!Done) Thread.Sleep(100);
            return WhereFound;
        }

        /// <summary>
        /// Disposes the internal objects
        /// </summary>
        /// <param name="Value"></param>
        protected virtual void Dispose(bool Value)
        {
            if (true)
            {
                if (URLs != null)
                {
                    URLs.Dispose();
                    URLs = null;
                }
            }
        }

        /// <summary>
        /// Fixes the URL.
        /// </summary>
        /// <param name="currentDomain">The current domain.</param>
        /// <param name="link">The link.</param>
        /// <param name="replacements">The replacements.</param>
        /// <returns>The resulting URL</returns>
        private static string FixUrl(string currentDomain, string link, Dictionary<string, string> replacements)
        {
            link = link.Replace("\\", "/").Trim();
            if (link.StartsWith("/", StringComparison.OrdinalIgnoreCase))
                link = currentDomain + link;
            else if (!SchemeRegex.IsMatch(link))
                link = "http://" + link;
            link = link.Split('#')[0];
            if (link.EndsWith("/", StringComparison.OrdinalIgnoreCase))
                link = link.Remove(link.LastIndexOf('/'), 1);
            link = Uri.UnescapeDataString(link).Trim();
            foreach (var Key in replacements.Keys)
            {
                link = new Regex(Key).Replace(link, replacements[Key]);
            }
            return Uri.EscapeUriString(link);
        }

        /// <summary>
        /// Gets the domain.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns>The domain associated with the URL</returns>
        private static string GetDomain(string url)
        {
            var TempUri = new Uri(url);
            return TempUri.Scheme + "://" + TempUri.Host + (TempUri.Port == 80 ? "" : (":" + TempUri.Port));
        }

        /// <summary>
        /// Adds the document.
        /// </summary>
        /// <param name="file">The file.</param>
        private void AddDocument(ResultFile file)
        {
            if (file == null)
                return;
            ItemFound(file);
        }

        /// <summary>
        /// Adds the urls.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="content">The content.</param>
        /// <param name="contentType">Type of the content.</param>
        private void AddUrls(string url, byte[] content, string contentType)
        {
            if (contentType.IndexOf("TEXT/HTML", StringComparison.InvariantCultureIgnoreCase) >= 0)
            {
                if (!CanFollow(url))
                    return;
                string CurrentDomain = GetDomain(url);

                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(Encoding.UTF8.GetString(content));
                var Nodes = doc.DocumentNode.SelectNodes("//a[@href]");
                if (Nodes == null)
                    return;

                foreach (var Link in Nodes.SelectMany(x => x.Attributes).Select(x => x.Value))
                {
                    var TempLink = Link;
                    if (string.IsNullOrEmpty(TempLink))
                        continue;
                    TempLink = FixUrl(CurrentDomain, TempLink, Options.UrlReplacements);
                    URLs.Enqueue(TempLink);
                    if (CanCrawl(TempLink))
                        WhereFound.Add(TempLink, url);
                }
            }
        }

        /// <summary>
        /// Determines whether this instance can crawl the specified link.
        /// </summary>
        /// <param name="link">The link.</param>
        /// <returns><c>true</c> if this instance can crawl the specified link; otherwise, <c>false</c>.</returns>
        private bool CanCrawl(string link)
        {
            return CanParse(link) || CanFollow(link);
        }

        /// <summary>
        /// Determines whether this instance can follow the specified temporary link.
        /// </summary>
        /// <param name="link">The temporary link.</param>
        /// <returns>
        /// <c>true</c> if this instance can follow the specified temporary link; otherwise, <c>false</c>.
        /// </returns>
        private bool CanFollow(string link)
        {
            return (Options.Allow.Any(x => new Regex(x, RegexOptions.IgnoreCase).IsMatch(link))
                || Options.FollowOnly.Any(x => new Regex(x, RegexOptions.IgnoreCase).IsMatch(link)))
                && !Options.Ignore.Any(x => new Regex(x, RegexOptions.IgnoreCase).IsMatch(link));
        }

        /// <summary>
        /// Determines whether this instance can parse the specified temporary link.
        /// </summary>
        /// <param name="link">The temporary link.</param>
        /// <returns>
        /// <c>true</c> if this instance can parse the specified temporary link; otherwise, <c>false</c>.
        /// </returns>
        private bool CanParse(string link)
        {
            return Options.Allow.Any(x => new Regex(x, RegexOptions.IgnoreCase).IsMatch(link))
                && !Options.Ignore.Any(x => new Regex(x, RegexOptions.IgnoreCase).IsMatch(link));
        }

        /// <summary>
        /// Parses the specified URL.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <returns>The resulting file.</returns>
        private ResultFile Parse(UrlData result)
        {
            if (!CanParse(result.URL))
                return null;
            string CurrentDomain = GetDomain(result.URL);
            using (System.IO.MemoryStream Stream = new System.IO.MemoryStream(result.Content))
            {
                return new ResultFile
                {
                    FileContent = Stream.Parse(result.ContentType),
                    Location = result.URL,
                    ContentType = result.ContentType,
                    FileName = result.FileName,
                    FinalLocation = FixUrl(CurrentDomain, result.FinalLocation, Options.UrlReplacements),
                    StatusCode = result.StatusCode,
                    Data = result
                };
            }
        }
    }
}