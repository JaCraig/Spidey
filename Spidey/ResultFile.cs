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

using FileCurator.Formats.Data.Interfaces;
using Spidey.Engines;

namespace Spidey
{
    /// <summary>
    /// Result file
    /// </summary>
    public class ResultFile
    {
        /// <summary>
        /// Gets or sets the type of the content.
        /// </summary>
        /// <value>The type of the content.</value>
        public string ContentType { get; set; }

        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        /// <value>The data.</value>
        public UrlData Data { get; set; }

        /// <summary>
        /// Gets or sets the file.
        /// </summary>
        /// <value>The file.</value>
        public IGenericFile FileContent { get; set; }

        /// <summary>
        /// Gets or sets the name of the file if this is something downloaded.
        /// </summary>
        /// <value>The name of the file if this is something downloaded.</value>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the final location (if page is redirected, this will be different than location).
        /// </summary>
        /// <value>The final location.</value>
        public string FinalLocation { get; set; }

        /// <summary>
        /// Gets the file location.
        /// </summary>
        /// <value>The file location.</value>
        public string Location { get; set; }

        /// <summary>
        /// Gets or sets the status code.
        /// </summary>
        /// <value>The status code.</value>
        public int StatusCode { get; set; }
    }
}