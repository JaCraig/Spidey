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
    /// Basic options class
    /// </summary>
    public class Options
    {
        /// <summary>
        /// Gets the default.
        /// </summary>
        /// <value>The default.</value>
        public static Options Default => new Options();

        /// <summary>
        /// Gets or sets the allowed items.
        /// </summary>
        /// <value>The allowed items.</value>
        public List<string> Allow { get; set; } = new List<string>();

        /// <summary>
        /// Gets the credentials.
        /// </summary>
        /// <value>The credentials.</value>
        public NetworkCredential? Credentials { get; set; }

        /// <summary>
        /// Gets or sets the follow only list.
        /// </summary>
        /// <value>The follow only list.</value>
        public List<string> FollowOnly { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the ignore list.
        /// </summary>
        /// <value>The ignore list.</value>
        public List<string> Ignore { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the item found.
        /// </summary>
        /// <value>The item found.</value>
        public Action<ResultFile> ItemFound { get; set; } = _ => { };

        /// <summary>
        /// Gets or sets the maximum delay.
        /// </summary>
        /// <value>The maximum delay.</value>
        public int MaxDelay { get; set; }

        /// <summary>
        /// Gets or sets the minimum delay.
        /// </summary>
        /// <value>The minimum delay.</value>
        public int MinDelay { get; set; }

        /// <summary>
        /// Gets or sets the number workers.
        /// </summary>
        /// <value>The number workers.</value>
        public int NumberWorkers { get; set; } = Environment.ProcessorCount;

        /// <summary>
        /// Gets the proxy.
        /// </summary>
        /// <value>The proxy.</value>
        public IWebProxy? Proxy { get; set; }

        /// <summary>
        /// Gets or sets the start locations.
        /// </summary>
        /// <value>The start locations.</value>
        public List<string> StartLocations { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets a list of replacements for URL parts. Key is the url part that you may
        /// find, value is the replacement for it.
        /// </summary>
        /// <value>The domain replacements.</value>
        public Dictionary<string, string> UrlReplacements { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets a value indicating whether [use default credentials].
        /// </summary>
        /// <value><c>true</c> if [use default credentials]; otherwise, <c>false</c>.</value>
        public bool UseDefaultCredentials { get; set; }

        /// <summary>
        /// Gets the allow compiled.
        /// </summary>
        /// <value>The allow compiled.</value>
        internal List<Regex> AllowCompiled { get; private set; } = new List<Regex>();

        /// <summary>
        /// Gets the follow only compiled.
        /// </summary>
        /// <value>The follow only compiled.</value>
        internal List<Regex> FollowOnlyCompiled { get; private set; } = new List<Regex>();

        /// <summary>
        /// Gets the ignore compiled.
        /// </summary>
        /// <value>The ignore compiled.</value>
        internal List<Regex> IgnoreCompiled { get; private set; } = new List<Regex>();

        /// <summary>
        /// Gets or sets the URL replacements compiled.
        /// </summary>
        /// <value>The URL replacements compiled.</value>
        internal Dictionary<Regex, string> UrlReplacementsCompiled { get; } = new Dictionary<Regex, string>();

        /// <summary>
        /// Gets the lock object.
        /// </summary>
        /// <value>The lock object.</value>
        private static object LockObject { get; } = new object();

        /// <summary>
        /// Determines whether this instance can crawl the specified link.
        /// </summary>
        /// <param name="link">The link.</param>
        /// <returns><c>true</c> if this instance can crawl the specified link; otherwise, <c>false</c>.</returns>
        internal bool CanCrawl(string link)
        {
            return CanParse(link) || CanFollow(link);
        }

        /// <summary>
        /// Determines whether this instance can follow the specified link.
        /// </summary>
        /// <param name="link">The link.</param>
        /// <returns><c>true</c> if this instance can follow the specified link; otherwise, <c>false</c>.</returns>
        internal bool CanFollow(string link)
        {
            if (AllowCompiled == null)
                Setup();
            return (AllowCompiled.Any(x => x.IsMatch(link))
                || FollowOnlyCompiled.Any(x => x.IsMatch(link)))
                && !IgnoreCompiled.Any(x => x.IsMatch(link));
        }

        /// <summary>
        /// Determines whether this instance can parse the specified temporary link.
        /// </summary>
        /// <param name="link">The temporary link.</param>
        /// <returns>
        /// <c>true</c> if this instance can parse the specified temporary link; otherwise, <c>false</c>.
        /// </returns>
        internal bool CanParse(string link)
        {
            if (AllowCompiled == null)
                Setup();
            return AllowCompiled.Any(x => x.IsMatch(link))
                && !IgnoreCompiled.Any(x => x.IsMatch(link));
        }

        /// <summary>
        /// Setups this instance.
        /// </summary>
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
                    var Holder = MinDelay;
                    MinDelay = MaxDelay;
                    MaxDelay = Holder;
                }
            }
            return this;
        }

        /// <summary>
        /// Defaults the item found.
        /// </summary>
        /// <param name="obj">The object.</param>
        private void DefaultItemFound(ResultFile obj)
        {
        }
    }
}