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
    /// Discovers links from HTML documents and normalizes them into absolute, escaped URLs.
    /// </summary>
    /// <remarks>
    /// Relative links are resolved against the supplied current domain or page URL, fragment-only
    /// and query-only links are expanded from the current page URL, and configured replacement rules
    /// are applied after normalization.
    /// </remarks>
    /// <seealso cref="ILinkDiscoverer"/>
    /// <param name="options">Optional link discovery options; when <see langword="null"/>, defaults are used.</param>
    public partial class DefaultLinkDiscoverer(Options? options) : ILinkDiscoverer
    {
        /// <summary>
        /// Gets the regular expression used to detect an existing URL scheme prefix.
        /// </summary>
        private static Regex SchemeRegex { get; } = UrlSchemeRegex();

        /// <summary>
        /// Gets the configured options instance used during discovery and URL normalization.
        /// </summary>
        private Options Options { get; } = (options ?? Options.Default).Setup();

        /// <summary>
        /// Extracts and normalizes links from HTML content.
        /// </summary>
        /// <param name="currentDomain">The current domain, used to resolve root-relative links.</param>
        /// <param name="url">The current page URL, used to resolve fragment-only and query-only links.</param>
        /// <param name="content">The response body to inspect.</param>
        /// <param name="contentType">
        /// The response content type. Only HTML content is processed; all other content types return no links.
        /// </param>
        /// <returns>An array of normalized URLs discovered in the document; otherwise an empty array.</returns>
        /// <remarks>
        /// The HTML is parsed using HtmlAgilityPack. Only anchor elements with an <c>href</c> attribute are considered.
        /// </remarks>
        public string[] DiscoverUrls(string currentDomain, string url, byte[] content, string contentType)
        {
            if (string.IsNullOrEmpty(contentType) || contentType.IndexOf("TEXT/HTML", StringComparison.InvariantCultureIgnoreCase) < 0)
                return [];
            var Doc = new HtmlAgilityPack.HtmlDocument();
            Doc.LoadHtml(Encoding.UTF8.GetString(content));
            return Doc
                .DocumentNode
                .SelectNodes("//a[@href]")
                ?.SelectMany(x => (x?.Attributes))
                .Where(x => string.Equals(x?.Name, "href", StringComparison.OrdinalIgnoreCase) || (x?.Value.StartsWith("http") ?? false))
                .Select(x => (x?.Value))
                .Where(x => !string.IsNullOrEmpty(x))
                .Select(x => FixUrl(currentDomain, x, url, Options.UrlReplacementsCompiled ?? []))
                .ToArray()
                ?? [];
        }

        /// <summary>
        /// Normalizes a discovered link into an absolute, escaped URL.
        /// </summary>
        /// <param name="currentDomain">The current domain, used when the link starts with <c>/</c>.</param>
        /// <param name="link">The candidate link to normalize.</param>
        /// <param name="url">The current page URL, used to expand fragment-only and query-only links.</param>
        /// <param name="replacements">Optional regex replacement rules applied after normalization.</param>
        /// <returns>The normalized URL, or an empty string when <paramref name="link"/> is null or empty.</returns>
        /// <remarks>
        /// Normalization performs the following steps in order:
        /// <list type="number">
        /// <item>Replaces backslashes with forward slashes and trims whitespace.</item>
        /// <item>Expands fragment-only and query-only links from the current page URL.</item>
        /// <item>Prefixes root-relative links with the current domain.</item>
        /// <item>Prefixes non-schemed links with <c>http://</c>.</item>
        /// <item>Removes fragments, trims trailing slashes, unescapes, applies regex replacements, and escapes the final URI string.</item>
        /// </list>
        /// </remarks>
        public string FixUrl(string currentDomain, string? link, string url, Dictionary<Regex, string> replacements)
        {
            if (string.IsNullOrEmpty(link))
                return "";
            replacements ??= [];
            link = link.Replace("\\", "/").Trim();
            if (link.StartsWith('#') || link.StartsWith('?'))
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
        /// Gets the scheme and authority portion of an absolute URL.
        /// </summary>
        /// <param name="url">The URL to inspect.</param>
        /// <returns>The scheme, host, and non-default port for the URL; otherwise an empty string.</returns>
        /// <remarks>
        /// Returns an empty string for invalid or non-absolute URLs. Port <c>80</c> is omitted to preserve the
        /// current implementation behavior.
        /// </remarks>
        public string GetDomain(string url)
        {
            if (string.IsNullOrEmpty(url) || !Uri.TryCreate(url, UriKind.Absolute, out var TempUri))
                return "";
            return TempUri.Scheme + "://" + TempUri.Host + (TempUri.Port == 80 ? "" : (":" + TempUri.Port));
        }

        /// <summary>
        /// Matches and captures the URL scheme prefix, including the trailing "://".
        /// </summary>
        /// <returns>A regular expression that matches the URL scheme prefix.</returns>
        [GeneratedRegex("^(?<scheme>[^:]*://)", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-US")]
        private static partial Regex UrlSchemeRegex();
    }
}