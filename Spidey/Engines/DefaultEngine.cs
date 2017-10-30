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

using BigBook;
using Spidey.Engines.Interfaces;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Spidey.Engines
{
    /// <summary>
    /// Default engine
    /// </summary>
    /// <seealso cref="IEngine"/>
    public class DefaultEngine : IEngine
    {
        /// <summary>
        /// The file name regex
        /// </summary>
        private static readonly Regex FileNameRegex = new Regex(@"filename=[\""']?(?<FileName>[^\""\n\r']*)['\""\n\r]?$", RegexOptions.Compiled);

        /// <summary>
        /// Crawls the url.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="options">The options.</param>
        /// <returns>The data from the URL.</returns>
        public async Task<UrlData> CrawlAsync(string url, Options options)
        {
            byte[] Content = new byte[0];
            var Client = WebRequest.Create(url);
            if (options.Credentials == null && options.UseDefaultCredentials)
                Client.UseDefaultCredentials = true;
            else
                Client.Credentials = options.Credentials;
            Client.Proxy = options.Proxy;

            var Response = (await Client.GetResponseAsync()) as HttpWebResponse;
            string FileName = Response.Headers["content-disposition"];
            if (!string.IsNullOrEmpty(FileName))
            {
                FileName = FileNameRegex.Match(FileName).Groups["FileName"].Value;
            }

            return new UrlData
            {
                Content = Response.GetResponseStream().ReadAllBinary(),
                ContentType = Response.ContentType,
                FileName = FileName,
                FinalLocation = Response.ResponseUri.ToString(),
                StatusCode = (int)Response.StatusCode,
                URL = url
            };
        }
    }
}