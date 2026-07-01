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

namespace Spidey.Engines
{
    /// <summary>
    /// Represents the result of downloading or resolving a URL.
    /// </summary>
    /// <remarks>
    /// This type is a simple data container populated by an engine and returned to callers.
    /// Property values are initialized from the constructor arguments and remain publicly settable
    /// so consumers can adjust them after creation if needed.
    /// </remarks>
    /// <param name="content">The retrieved content bytes.</param>
    /// <param name="contentType">The response content type reported by the source.</param>
    /// <param name="fileName">The suggested or resolved file name.</param>
    /// <param name="finalLocation">The final location after redirects or resolution.</param>
    /// <param name="statusCode">The HTTP or engine-specific status code.</param>
    /// <param name="uRL">The original or effective URL associated with the result.</param>
    public class UrlData(byte[] content, string contentType, string fileName, string finalLocation, int statusCode, string uRL)
    {
        /// <summary>
        /// Gets or sets the retrieved content bytes.
        /// </summary>
        /// <value>
        /// The raw payload returned by the engine. The array reference may be reassigned by consumers.
        /// </value>
        public byte[] Content { get; set; } = content;

        /// <summary>
        /// Gets or sets the MIME type or content type of the retrieved payload.
        /// </summary>
        /// <value>The response content type.</value>
        public string ContentType { get; set; } = contentType;

        /// <summary>
        /// Gets or sets the file name associated with the result.
        /// </summary>
        /// <value>The file name suggested by the source or derived by the engine.</value>
        public string FileName { get; set; } = fileName;

        /// <summary>
        /// Gets or sets the final resolved location.
        /// </summary>
        /// <value>The final URL or path after any redirects or resolution steps.</value>
        public string FinalLocation { get; set; } = finalLocation;

        /// <summary>
        /// Gets or sets the status code associated with the result.
        /// </summary>
        /// <value>The HTTP status code or equivalent engine-specific status value.</value>
        public int StatusCode { get; set; } = statusCode;

        /// <summary>
        /// Gets or sets the original or effective URL for the result.
        /// </summary>
        /// <value>The URL used to obtain the content.</value>
        public string URL { get; set; } = uRL;
    }
}