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

using FileCurator;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using Spidey.Engines.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Spidey.Engines
{
    /// <summary>
    /// Parses fetched content into a <see cref="ResultFile"/> using the configured parser options
    /// and the first available non-default link discoverer.
    /// </summary>
    /// <remarks>
    /// The parser is designed for single-call use per request. It does not mutate shared state, but
    /// its behavior depends on the injected options, discoverers, and content stream manager.
    /// </remarks>
    /// <seealso cref="IContentParser"/>
    /// <param name="options">
    /// Parser configuration. When <see langword="null"/>, <see cref="Options.Default"/> is used.
    /// </param>
    /// <param name="linkDiscoverers">
    /// Available link discoverers. The parser prefers the first non- <see
    /// cref="DefaultLinkDiscoverer"/>, falling back to the first default discoverer when no
    /// specialized implementation is present.
    /// </param>
    /// <param name="recyclableMemoryStreamManager">
    /// Stream factory used to create a recyclable in-memory stream for parsing the response content.
    /// </param>
    /// <param name="logger">Optional logger used to trace parse attempts.</param>
    public class DefaultContentParser(Options? options, IEnumerable<ILinkDiscoverer> linkDiscoverers, RecyclableMemoryStreamManager recyclableMemoryStreamManager, ILogger<DefaultContentParser>? logger = null) : IContentParser
    {
        /// <summary>
        /// Gets the link discoverer selected during construction.
        /// </summary>
        /// <value>
        /// The first non-default discoverer from <paramref name="linkDiscoverers"/>, or the first
        /// <see cref="DefaultLinkDiscoverer"/> if no specialized discoverer is available.
        /// </value>
        private ILinkDiscoverer? LinkDiscoverer { get; } = linkDiscoverers.FirstOrDefault(x => x is not DefaultLinkDiscoverer) ?? linkDiscoverers.FirstOrDefault(x => x is DefaultLinkDiscoverer);

        /// <summary>
        /// Gets the logger used for parse diagnostics.
        /// </summary>
        /// <value>The configured logger instance, or <see langword="null"/> when logging is disabled.</value>
        private ILogger<DefaultContentParser>? Logger { get; } = logger;

        /// <summary>
        /// Gets the parser options after applying defaults and setup.
        /// </summary>
        /// <value>The effective parser options.</value>
        private Options Options { get; } = (options ?? Options.Default).Setup();

        /// <summary>
        /// Gets the recyclable memory stream manager used to materialize content for parsing.
        /// </summary>
        /// <value>The configured recyclable memory stream manager.</value>
        private RecyclableMemoryStreamManager RecyclableMemoryStreamManager { get; } = recyclableMemoryStreamManager;

        /// <summary>
        /// Parses the supplied URL data into a result file.
        /// </summary>
        /// <param name="data">The URL payload to parse.</param>
        /// <returns>
        /// A populated <see cref="ResultFile"/> when the input can be parsed; otherwise <see langword="null"/>.
        /// </returns>
        /// <remarks>
        /// Parsing is skipped when the input is <see langword="null"/>, the URL is excluded by the
        /// configured options, or no suitable link discoverer was injected.
        /// </remarks>
        public ResultFile? Parse(UrlData? data)
        {
            if (data is null || !Options.CanParse(data.URL) || LinkDiscoverer is null)
                return null;
            Logger?.LogDebug("Parsing {dataUrl}", data.URL);
            var CurrentDomain = LinkDiscoverer.GetDomain(data.URL);
            using var Stream = RecyclableMemoryStreamManager.GetStream(data.Content);
            return new ResultFile(
                data.ContentType,
                data,
                Stream.Parse(data.ContentType),
                data.FileName,
                LinkDiscoverer.FixUrl(CurrentDomain, data.FinalLocation, data.URL, Options.UrlReplacementsCompiled ?? []),
                data.URL,
                data.StatusCode);
        }
    }
}