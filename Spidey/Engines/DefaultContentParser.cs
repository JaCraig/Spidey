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
using System.Text.RegularExpressions;

namespace Spidey.Engines
{
    /// <summary>
    /// Default content parser
    /// </summary>
    /// <seealso cref="IContentParser"/>
    public class DefaultContentParser : IContentParser
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultContentParser"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="linkDiscoverers">The link discoverers.</param>
        /// <param name="recyclableMemoryStreamManager">The recyclable memory stream manager.</param>
        /// <param name="logger">The logger.</param>
        public DefaultContentParser(Options? options, IEnumerable<ILinkDiscoverer> linkDiscoverers, RecyclableMemoryStreamManager recyclableMemoryStreamManager, ILogger<DefaultContentParser>? logger = null)
        {
            Options = (options ?? Options.Default).Setup();
            LinkDiscoverer = linkDiscoverers.FirstOrDefault(x => !(x is DefaultLinkDiscoverer)) ?? linkDiscoverers.FirstOrDefault(x => x is DefaultLinkDiscoverer);
            Logger = logger;
            RecyclableMemoryStreamManager = recyclableMemoryStreamManager;
        }

        /// <summary>
        /// Gets the link discoverer.
        /// </summary>
        /// <value>The link discoverer.</value>
        private ILinkDiscoverer LinkDiscoverer { get; }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        /// <value>The logger.</value>
        private ILogger<DefaultContentParser>? Logger { get; }

        /// <summary>
        /// Gets the options.
        /// </summary>
        /// <value>The options.</value>
        private Options Options { get; }

        /// <summary>
        /// Gets the recyclable memory stream manager.
        /// </summary>
        /// <value>The recyclable memory stream manager.</value>
        private RecyclableMemoryStreamManager RecyclableMemoryStreamManager { get; }

        /// <summary>
        /// Parses the specified options.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>Result file</returns>
        public ResultFile? Parse(UrlData? data)
        {
            if (data is null || !Options.CanParse(data.URL))
                return null;
            Logger?.LogDebug($"Parsing {data.URL}");
            var CurrentDomain = LinkDiscoverer.GetDomain(data.URL);
            using var Stream = RecyclableMemoryStreamManager.GetStream(data.Content);
            return new ResultFile(
                data.ContentType,
                data,
                Stream.Parse(data.ContentType),
                data.FileName,
                LinkDiscoverer.FixUrl(CurrentDomain, data.FinalLocation, Options.UrlReplacementsCompiled ?? new Dictionary<Regex, string>()),
                data.URL,
                data.StatusCode);
        }
    }
}