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
using FileCurator;
using Serilog;
using System;
using System.Collections.Concurrent;
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
    /// <seealso cref="System.IDisposable"/>
    public class Crawler : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Crawler"/> class.
        /// </summary>
        /// <param name="itemFound">The item found.</param>
        /// <param name="options">The options.</param>
        /// <param name="logger">The logger.</param>
        /// <exception cref="System.ArgumentNullException">logger</exception>
        public Crawler(Action<ResultFile> itemFound, Options options, ILogger logger)
        {
            ItemFound = itemFound;
            Logger = logger ?? Log.Logger ?? new LoggerConfiguration().CreateLogger() ?? throw new ArgumentNullException(nameof(logger));
            Options = options ?? Options.Default;
            WhereFound = new ListMapping<string, string>();
            URLs = new TaskQueue<string>(Environment.ProcessorCount,
                x => Crawl(x).GetAwaiter().GetResult(),
                100,
                (x, y) =>
                {
                    Logger.Error(x, "An error has occurred");
                    ErrorURLs.Add(new ErrorItem { Error = x, Url = y });
                });
            CompletedURLs = new ConcurrentBag<string>();
            ErrorURLs = new ConcurrentBag<ErrorItem>();
        }

        private static Regex FileNameRegex = new Regex(@"filename=[\""']?(?<FileName>[^\""\n\r']*)['\""\n\r]?$", RegexOptions.Compiled);

        /// <summary>
        /// Gets a value indicating whether this <see cref="Crawler"/> is done.
        /// </summary>
        /// <value><c>true</c> if done; otherwise, <c>false</c>.</value>
        public bool Done => URLs.IsComplete;

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
        /// Gets or sets the completed urls.
        /// </summary>
        /// <value>The completed urls.</value>
        private ConcurrentBag<string> CompletedURLs { get; set; }

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
        private ListMapping<string, string> WhereFound { get; set; }

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

            byte[] Content = new byte[0];
            var Client = WebRequest.Create(url);
            if (Options.Credentials == null && Options.UseDefaultCredentials)
                Client.UseDefaultCredentials = true;
            else
                Client.Credentials = Options.Credentials;
            Client.Proxy = Options.Proxy;

            var Response = await Client.GetResponseAsync();
            Content = Response.GetResponseStream().ReadAllBinary();
            string ContentType = Response.ContentType;
            string FinalLocation = Response.ResponseUri.ToString();
            string FileName = Response.Headers["content-disposition"];
            if (!string.IsNullOrEmpty(FileName))
            {
                FileName = FileNameRegex.Match(FileName).Groups["FileName"].Value;
            }
            AddDocument(Parse(url, Content, ContentType, FinalLocation, FileName));
            AddUrls(url, Content, ContentType);
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
            if (!Options.StartLocations.Any())
                return WhereFound;
            Options.StartLocations.ForEach(x =>
            {
                URLs.Enqueue(x);
            });
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

        private static string FixUrl(string currentDomain, string link)
        {
            link = link.Replace("\\", "/");
            if (link.StartsWith("/", StringComparison.OrdinalIgnoreCase))
                link = currentDomain + link;
            else if (!link.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
                link = "http://" + link;
            link = link.Split('#')[0];
            if (link.EndsWith("/", StringComparison.OrdinalIgnoreCase))
                link = link.Remove(link.LastIndexOf('/'), 1);
            return System.Uri.EscapeUriString(System.Uri.UnescapeDataString(link));
        }

        /// <summary>
        /// Gets the domain.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns>The domain associated with the URL</returns>
        private static string GetDomain(string url)
        {
            var TempUri = new Uri(url);
            return "http://" + TempUri.Host + (TempUri.Port == 80 ? "" : (":" + TempUri.Port));
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
            if (contentType.ToUpperInvariant().Contains("TEXT/HTML"))
            {
                if (!CanFollow(url))
                    return;
                string CurrentDomain = GetDomain(url);

                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(Encoding.UTF8.GetString(content));
                var Nodes = doc.DocumentNode.SelectNodes("//a[@href]");
                if (Nodes == null)
                    return;

                var Links = Nodes.SelectMany(x => x.Attributes).Select(x => x.Value);
                foreach (var Link in Links)
                {
                    var TempLink = Link;
                    if (string.IsNullOrEmpty(TempLink))
                        continue;
                    TempLink = FixUrl(CurrentDomain, TempLink);
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
        /// <param name="url">The URL.</param>
        /// <param name="content">The content.</param>
        /// <param name="contentType">Type of the content.</param>
        /// <param name="finalLocation">The final location.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>The resulting file.</returns>
        private ResultFile Parse(string url, byte[] content, string contentType, string finalLocation, string fileName)
        {
            if (!CanParse(url))
                return null;
            string CurrentDomain = GetDomain(url);
            using (System.IO.MemoryStream Stream = new System.IO.MemoryStream(content))
            {
                return new ResultFile
                {
                    FileContent = Stream.Parse(contentType),
                    Location = url,
                    ContentType = contentType,
                    FileName = fileName,
                    FinalLocation = FixUrl(CurrentDomain, finalLocation)
                };
            }
        }
    }
}