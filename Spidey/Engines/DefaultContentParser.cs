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

using FileCurator;
using Spidey.Engines.Interfaces;

namespace Spidey.Engines
{
    /// <summary>
    /// Default content parser
    /// </summary>
    /// <seealso cref="IContentParser"/>
    public class DefaultContentParser : IContentParser
    {
        /// <summary>
        /// Parses the specified options.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="data">The data.</param>
        /// <returns>Result file</returns>
        public ResultFile Parse(Options options, UrlData data)
        {
            if (!options.CanParse(data.URL))
                return null;
            var CurrentDomain = options.LinkDiscoverer.GetDomain(data.URL);
            using (var Stream = new System.IO.MemoryStream(data.Content))
            {
                return new ResultFile
                {
                    FileContent = Stream.Parse(data.ContentType),
                    Location = data.URL,
                    ContentType = data.ContentType,
                    FileName = data.FileName,
                    FinalLocation = options.LinkDiscoverer.FixUrl(CurrentDomain, data.FinalLocation, options.UrlReplacementsCompiled),
                    StatusCode = data.StatusCode,
                    Data = data
                };
            }
        }
    }
}