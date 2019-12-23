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

namespace Spidey
{
    /// <summary>
    /// Error item data holder.
    /// </summary>
    public class ErrorItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorItem"/> class.
        /// </summary>
        /// <param name="error">The error.</param>
        /// <param name="url">The URL.</param>
        /// <param name="statusCode">The status code.</param>
        public ErrorItem(Exception error, string url, int statusCode)
        {
            Error = error;
            Url = url;
            StatusCode = statusCode;
        }

        /// <summary>
        /// Gets or sets the error.
        /// </summary>
        /// <value>The error.</value>
        public Exception Error { get; set; }

        /// <summary>
        /// Gets or sets the status code.
        /// </summary>
        /// <value>The status code.</value>
        public int StatusCode { get; set; }

        /// <summary>
        /// Gets or sets the URL.
        /// </summary>
        /// <value>The URL.</value>
        public string Url { get; set; }
    }
}