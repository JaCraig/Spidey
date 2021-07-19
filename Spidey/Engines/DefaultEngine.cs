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
using Microsoft.Extensions.Logging;
using Spidey.Engines.Interfaces;
using System;
using System.Net;
using System.Net.Http;
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
        /// Initializes a new instance of the <see cref="DefaultEngine"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="logger">The logger.</param>
        public DefaultEngine(Options? options, ILogger<DefaultEngine>? logger = null)
        {
            Options = (options ?? Options.Default).Setup();
            Logger = logger;
            var Handler = new HttpClientHandler()
            {
                Proxy = Options.Proxy,
                AllowAutoRedirect = true
            };
            if (!string.IsNullOrEmpty(Options.Credentials?.UserName) && !string.IsNullOrEmpty(Options.Credentials?.Password))
            {
                if (!string.IsNullOrEmpty(Options.Credentials?.Domain))
                    Handler.Credentials = new NetworkCredential(Options.Credentials?.UserName, Options.Credentials?.Password, Options.Credentials?.Domain);
                else
                    Handler.Credentials = new NetworkCredential(Options.Credentials?.UserName, Options.Credentials?.Password);
            }
            else
            {
                Handler.UseDefaultCredentials = Options.UseDefaultCredentials;
            }
            Client = new HttpClient(Handler);
        }

        /// <summary>
        /// Gets the client.
        /// </summary>
        /// <value>The client.</value>
        private HttpClient? Client { get; set; }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        /// <value>The logger.</value>
        private ILogger<DefaultEngine>? Logger { get; }

        /// <summary>
        /// Gets the options.
        /// </summary>
        /// <value>The options.</value>
        private Options Options { get; }

        /// <summary>
        /// The file name regex
        /// </summary>
        private static readonly Regex FileNameRegex = new Regex(@"filename=[\""']?(?<FileName>[^\""\n\r']*)['\""\n\r]?$", RegexOptions.Compiled);

        /// <summary>
        /// Crawls the url.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns>The data from the URL.</returns>
        public async Task<UrlData?> CrawlAsync(string url)
        {
            if (Client is null || string.IsNullOrEmpty(url) || !Uri.TryCreate(url, UriKind.Absolute, out var TempUrl))
                return null;
            Logger?.LogDebug($"Crawling {TempUrl}");

            try
            {
                var Response = await Client.GetAsync(TempUrl).ConfigureAwait(false);
                if (Response is null)
                    return null;
                string FileName = GetFileName(Response);
                return new UrlData(
                    await Response.Content.ReadAsByteArrayAsync().ConfigureAwait(false) ?? Array.Empty<byte>(),
                    Response.Content.Headers.ContentType.ToString(),
                    FileName,
                    Response.Headers.Location?.ToString() ?? Response.RequestMessage.RequestUri?.ToString() ?? "",
                    (int)Response.StatusCode,
                    url
                );
            }
            catch (HttpRequestException e)
            {
                Logger?.LogError(e, $"Error crawling {TempUrl}");
                string FileName = url;
                return new UrlData(
                    Array.Empty<byte>(),
                    "",
                    FileName,
                    url,
                    (int)HttpStatusCode.ServiceUnavailable,
                    url
                );
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting
        /// unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Client?.Dispose();
            Client = null;
        }

        /// <summary>
        /// Gets the name of the file.
        /// </summary>
        /// <param name="Response">The response.</param>
        /// <returns>The file name.</returns>
        private static string GetFileName(HttpResponseMessage? Response)
        {
            if (Response is null)
                return "";
            var FileName = "";
            if (!string.IsNullOrEmpty(Response.Content.Headers.ContentDisposition?.FileName))
            {
                FileName = Response.Content.Headers.ContentDisposition?.FileName ?? "";
            }
            if (!string.IsNullOrEmpty(FileName))
            {
                FileName = FileNameRegex.Match(FileName).Groups["FileName"].Value;
            }
            if (string.IsNullOrEmpty(FileName))
            {
                var ResultURI = Response.Headers.Location?.ToString() ?? Response.RequestMessage.RequestUri?.ToString() ?? "";
                FileName = ResultURI.Right(ResultURI.Length - ResultURI.LastIndexOf("/", StringComparison.Ordinal) - 1);
                if (FileName.Contains("?"))
                    FileName = FileName.Left(FileName.IndexOf("?", StringComparison.Ordinal));
            }

            return FileName;
        }
    }
}