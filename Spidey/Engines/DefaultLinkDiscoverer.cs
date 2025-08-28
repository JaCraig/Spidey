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

using Spidey.Engines.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Spidey.Engines
{
    /// <summary>
    /// Default link engine
    /// </summary>
    /// <seealso cref="ILinkDiscoverer"/>
    public class DefaultLinkDiscoverer : ILinkDiscoverer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultLinkDiscoverer"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        public DefaultLinkDiscoverer(Options? options)
        {
            Options = (options ?? Options.Default).Setup();
        }

        /// <summary>
        /// Gets the scheme regex.
        /// </summary>
        /// <value>The scheme regex.</value>
        private static Regex SchemeRegex { get; } = new Regex("^(?<scheme>[^:]*://)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// Gets the options.
        /// </summary>
        /// <value>The options.</value>
        private Options Options { get; }

        /// <summary>
        /// Discovers the urls.
        /// </summary>
        /// <param name="currentDomain">The current domain.</param>
        /// <param name="url">The URL.</param>
        /// <param name="content">The content.</param>
        /// <param name="contentType">Type of the content.</param>
        /// <returns>The links within the document.</returns>
        public string[] DiscoverUrls(string currentDomain, string url, byte[] content, string contentType)
        {
            if (string.IsNullOrEmpty(contentType) || contentType.IndexOf("TEXT/HTML", StringComparison.InvariantCultureIgnoreCase) < 0)
                return Array.Empty<string>();
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(Encoding.UTF8.GetString(content));
            return doc
                .DocumentNode
                .SelectNodes("//a[@href]")
                ?.SelectMany(x => (x?.Attributes))
                .Where(x => string.Equals(x?.Name, "href", StringComparison.OrdinalIgnoreCase) || (x?.Value.StartsWith("http") ?? false))
                .Select(x => (x?.Value))
                .Where(x => !string.IsNullOrEmpty(x))
                .Select(x => FixUrl(currentDomain, x, url, Options.UrlReplacementsCompiled ?? new Dictionary<Regex, string>()))
                .ToArray()
                ?? Array.Empty<string>();
        }

        /// <summary>
        /// Fixes the URL.
        /// </summary>
        /// <param name="currentDomain">The current domain.</param>
        /// <param name="link">The link.</param>
        /// <param name="url">The URL.</param>
        /// <param name="replacements">The replacements.</param>
        /// <returns>The fixed URL</returns>
        public string FixUrl(string currentDomain, string? link, string url, Dictionary<Regex, string> replacements)
        {
            if (string.IsNullOrEmpty(link))
                return "";
            replacements ??= new Dictionary<Regex, string>();
            link = link.Replace("\\", "/").Trim();
            if (link.StartsWith("#") || link.StartsWith("?"))
                link = url.Split('#')[0].Split('?')[0] + link;
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
                link = Key.Replace(link, replacements[Key]);
            }
            return Uri.EscapeUriString(link);
        }

        /// <summary>
        /// Gets the domain.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns>The domain of the url.</returns>
        public string GetDomain(string url)
        {
            if (string.IsNullOrEmpty(url) || !Uri.TryCreate(url, UriKind.Absolute, out var TempUri))
                return "";
            return TempUri.Scheme + "://" + TempUri.Host + (TempUri.Port == 80 ? "" : (":" + TempUri.Port));
        }
    }
}