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

using Spidey.Engines;
using Spidey.Engines.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
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
        /// <param name="pipelines">The pipelines.</param>
        /// <param name="options">The options.</param>
        public Crawler(IEnumerable<IPipeline> pipelines, Options? options = null)
        {
            Pipeline = pipelines.FirstOrDefault(x => x is not DefaultPipeline) ?? pipelines.FirstOrDefault(x => x is DefaultPipeline);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Crawler"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        public Crawler(Options? options = null)
            : this([new DefaultPipeline(
                [new DefaultScheduler(options, [new DefaultEngine(options)])],
                [new DefaultProcessor(options)],
                [new DefaultContentParser(options, [new DefaultLinkDiscoverer(options)], new Microsoft.IO.RecyclableMemoryStreamManager())],
                [new DefaultLinkDiscoverer(options)],
                options) ])
        {
        }

        /// <summary>
        /// Gets the pipeline.
        /// </summary>
        /// <value>The pipeline.</value>
        private IPipeline? Pipeline { get; set; }

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
        public Task<Results?> StartCrawlAsync()
        {
            return Pipeline?.StartCrawlAsync() ?? Task.FromResult<Results?>(null);
        }

        /// <summary>
        /// Disposes the internal objects
        /// </summary>
        /// <param name="Value"></param>
        protected virtual void Dispose(bool Value)
        {
            Pipeline?.Dispose();
            Pipeline = null;
        }
    }
}