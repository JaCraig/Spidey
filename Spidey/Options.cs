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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace Spidey
{
    /// <summary>
    /// Configures crawling behavior, request options, and link filtering for a crawl run.
    /// </summary>
    /// <remarks>
    /// Instances are mutable and are intended to be configured before crawling begins. Internal
    /// compiled regex caches are populated on first use by <see cref="Setup"/>.
    /// </remarks>
    public class Options
    {
        /// <summary>
        /// Gets a new options instance with default values.
        /// </summary>
        public static Options Default => new();

        /// <summary>
        /// Gets or sets the allowed link patterns.
        /// </summary>
        /// <remarks>A link must match at least one allowed pattern to be parsed or followed.</remarks>
        public List<string> Allow { get; set; } = [];

        /// <summary>
        /// Gets or sets the network credentials used for requests.
        /// </summary>
        /// <remarks>
        /// When set, these credentials are used explicitly rather than the default credentials.
        /// </remarks>
        public NetworkCredential? Credentials { get; set; }

        /// <summary>
        /// Gets or sets the additional patterns that are eligible to be followed.
        /// </summary>
        /// <remarks>
        /// These patterns expand the follow set without affecting parsing unless the link also
        /// matches <see cref="Allow"/>.
        /// </remarks>
        public List<string> FollowOnly { get; set; } = [];

        /// <summary>
        /// Gets or sets the patterns that are excluded from crawling.
        /// </summary>
        /// <remarks>Ignore rules always take precedence over allow and follow rules.</remarks>
        public List<string> Ignore { get; set; } = [];

        /// <summary>
        /// Gets or sets the callback invoked when a result file is discovered.
        /// </summary>
        /// <remarks>
        /// The callback is initialized to a no-op and is guaranteed to be non-null after <see
        /// cref="Setup"/> completes.
        /// </remarks>
        public Action<ResultFile> ItemFound { get; set; } = _ => { };

        /// <summary>
        /// Gets or sets the maximum crawl delay in milliseconds.
        /// </summary>
        /// <remarks>Negative values are normalized to zero in <see cref="Setup"/>.</remarks>
        public int MaxDelay { get; set; }

        /// <summary>
        /// Gets or sets the minimum crawl delay in milliseconds.
        /// </summary>
        /// <remarks>
        /// Negative values are normalized to zero in <see cref="Setup"/>. If this value exceeds
        /// <see cref="MaxDelay"/>, the two values are swapped.
        /// </remarks>
        public int MinDelay { get; set; }

        /// <summary>
        /// Gets or sets the number of worker threads used by the crawler.
        /// </summary>
        /// <remarks>Values less than or equal to zero are normalized to one in <see cref="Setup"/>.</remarks>
        public int NumberWorkers { get; set; } = Environment.ProcessorCount;

        /// <summary>
        /// Gets or sets the proxy used for requests.
        /// </summary>
        public IWebProxy? Proxy { get; set; }

        /// <summary>
        /// Gets or sets the HTTP request headers applied to outgoing requests.
        /// </summary>
        /// <remarks>The key is the header name and the value is the header value.</remarks>
        public Dictionary<string, string> RequestHeaders { get; set; } = [];

        /// <summary>
        /// Gets or sets the initial URLs used to start crawling.
        /// </summary>
        public List<string> StartLocations { get; set; } = [];

        /// <summary>
        /// Gets or sets replacements applied to URL text before matching or following.
        /// </summary>
        /// <remarks>Keys are regular expressions; values are replacement strings.</remarks>
        public Dictionary<string, string> UrlReplacements { get; set; } = [];

        /// <summary>
        /// Gets or sets a value indicating whether default credentials should be used for requests.
        /// </summary>
        public bool UseDefaultCredentials { get; set; }

        /// <summary>
        /// Gets the compiled allow patterns used for matching.
        /// </summary>
        internal List<Regex> AllowCompiled { get; private set; } = [];

        /// <summary>
        /// Gets the compiled follow-only patterns used for matching.
        /// </summary>
        internal List<Regex> FollowOnlyCompiled { get; private set; } = [];

        /// <summary>
        /// Gets the compiled ignore patterns used for matching.
        /// </summary>
        internal List<Regex> IgnoreCompiled { get; private set; } = [];

        /// <summary>
        /// Gets the compiled URL replacement expressions.
        /// </summary>
        internal Dictionary<Regex, string> UrlReplacementsCompiled { get; } = [];

        /// <summary>
        /// Synchronizes one-time compilation and normalization of option values.
        /// </summary>
        private static object LockObject { get; } = new object();

        /// <summary>
        /// Determines whether the specified link can be crawled.
        /// </summary>
        /// <param name="link">The link to evaluate.</param>
        /// <returns>
        /// <see langword="true"/> if the link can be parsed or followed; otherwise, <see langword="false"/>.
        /// </returns>
        internal bool CanCrawl(string link)
        {
            return CanParse(link) || CanFollow(link);
        }

        /// <summary>
        /// Determines whether the specified link can be followed.
        /// </summary>
        /// <param name="link">The link to evaluate.</param>
        /// <returns>
        /// <see langword="true"/> if the link matches an allow or follow-only rule and does not
        /// match any ignore rule; otherwise, <see langword="false"/>.
        /// </returns>
        internal bool CanFollow(string link)
        {
            if (AllowCompiled is null)
                Setup();
            return (AllowCompiled.Any(x => x.IsMatch(link))
                    || FollowOnlyCompiled.Any(x => x.IsMatch(link)))
                    && !IgnoreCompiled.Any(x => x.IsMatch(link));
        }

        /// <summary>
        /// Determines whether the specified link can be parsed.
        /// </summary>
        /// <param name="link">The link to evaluate.</param>
        /// <returns>
        /// <see langword="true"/> if the link matches an allow rule and does not match any ignore
        /// rule; otherwise, <see langword="false"/>.
        /// </returns>
        internal bool CanParse(string link)
        {
            if (AllowCompiled is null)
                Setup();
            return AllowCompiled.Any(x => x.IsMatch(link))
                && !IgnoreCompiled.Any(x => x.IsMatch(link));
        }

        /// <summary>
        /// Compiles regex-based options and normalizes numeric settings.
        /// </summary>
        /// <returns>The current <see cref="Options"/> instance.</returns>
        /// <remarks>
        /// This method is idempotent after the first successful initialization. It compiles regular
        /// expressions once, assigns a no-op item callback when needed, and clamps invalid delay
        /// and worker values to safe defaults.
        /// </remarks>
        internal Options Setup()
        {
            if (IgnoreCompiled.Count > 0 || FollowOnlyCompiled.Count > 0 || AllowCompiled.Count > 0 || UrlReplacementsCompiled.Count > 0)
                return this;
            lock (LockObject)
            {
                if (IgnoreCompiled.Count > 0 || FollowOnlyCompiled.Count > 0 || AllowCompiled.Count > 0 || UrlReplacementsCompiled.Count > 0)
                    return this;
                IgnoreCompiled = Ignore.ConvertAll(x => new Regex(x, RegexOptions.Compiled | RegexOptions.IgnoreCase));
                FollowOnlyCompiled = FollowOnly.ConvertAll(x => new Regex(x, RegexOptions.Compiled | RegexOptions.IgnoreCase));
                AllowCompiled = Allow.ConvertAll(x => new Regex(x, RegexOptions.Compiled | RegexOptions.IgnoreCase));
                foreach (var Key in UrlReplacements.Keys)
                {
                    UrlReplacementsCompiled.Add(new Regex(Key), UrlReplacements[Key]);
                }
                ItemFound ??= DefaultItemFound;
                if (MinDelay < 0)
                    MinDelay = 0;
                if (MaxDelay < 0)
                    MaxDelay = 0;
                if (NumberWorkers <= 0)
                    NumberWorkers = 1;
                if (MinDelay > MaxDelay)
                {
                    (MaxDelay, MinDelay) = (MinDelay, MaxDelay);
                }
            }
            return this;
        }

        /// <summary>
        /// Default no-op callback used when no item handler is supplied.
        /// </summary>
        /// <param name="obj">The discovered result file.</param>
        private void DefaultItemFound(ResultFile obj)
        {
        }
    }
}