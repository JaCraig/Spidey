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

using System.Threading.Tasks;

namespace Spidey.Engines.Interfaces
{
    /// <summary>
    /// Engine interface
    /// </summary>
    public interface IEngine
    {
        /// <summary>
        /// Crawls the specified URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="options">The options.</param>
        /// <returns>The data from the url.</returns>
        Task<UrlData> CrawlAsync(string url, Options options);
    }
}