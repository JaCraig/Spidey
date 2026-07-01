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

using BigBook.ExtensionMethods;
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
    /// Default engine implementation for crawling absolute URLs with <see cref="HttpClient"/>.
    /// </summary>
    /// <remarks>
    /// The instance owns the underlying <see cref="HttpClient"/> and must be disposed when no
    /// longer needed. Request headers, proxy settings, credentials, and default credentials
    /// behavior are configured from <see cref="Options"/>.
    /// </remarks>
    /// <seealso cref="IEngine"/>
    public partial class DefaultEngine : IEngine
    {
        /// <summary>
        /// Matches and extracts a file name value from a quoted or unquoted Content-Disposition
        /// file name token.
        /// </summary>
        private static readonly Regex _FileNameRegex = GenerateFileNameRegex();

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultEngine"/> class.
        /// </summary>
        /// <param name="options">
        /// Engine configuration. When <see langword="null"/>, <see cref="Options.Default"/> is used.
        /// </param>
        /// <param name="logger">Optional logger used for crawl diagnostics and failures.</param>
        public DefaultEngine(Options? options, ILogger<DefaultEngine>? logger = null)
        {
            Options = (options ?? Options.Default).Setup();
            Logger = logger;
            var Handler = new HttpClientHandler()
            {
                Proxy = Options.Proxy,
                AllowAutoRedirect = true,
                AutomaticDecompression = DecompressionMethods.All
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
            foreach (var Header in Options.RequestHeaders)
            {
                if (string.IsNullOrEmpty(Header.Key))
                    continue;
                Client.DefaultRequestHeaders.Add(Header.Key, Header.Value ?? "");
            }
        }

        /// <summary>
        /// Gets the client used for outbound HTTP requests.
        /// </summary>
        /// <value>The client.</value>
        private HttpClient? Client { get; set; }

        /// <summary>
        /// Gets the logger associated with this engine instance.
        /// </summary>
        /// <value>The logger.</value>
        private ILogger<DefaultEngine>? Logger { get; }

        /// <summary>
        /// Gets the configured engine options after normalization via <see cref="Options.Setup"/>.
        /// </summary>
        /// <value>The options.</value>
        private Options Options { get; }

        /// <summary>
        /// Crawls the specified absolute URL and returns the response payload and metadata.
        /// </summary>
        /// <param name="url">Absolute URL to request.</param>
        /// <returns>
        /// A populated <see cref="UrlData"/> instance when the request can be issued; otherwise
        /// <see langword="null"/> when the URL is invalid or the client has been disposed.
        /// </returns>
        /// <remarks>
        /// Redirects are followed by the underlying <see cref="HttpClientHandler"/>. On successful
        /// responses, the returned file name is derived from the response headers or final URL. On
        /// <see cref="HttpRequestException"/>, the method logs the failure and returns an
        /// error-shaped <see cref="UrlData"/> with an empty payload and a 503 status code.
        /// </remarks>
        public async Task<UrlData?> CrawlAsync(string url)
        {
            if (Client is null || string.IsNullOrEmpty(url) || !Uri.TryCreate(url, UriKind.Absolute, out Uri? TempUrl))
                return null;
            Logger?.LogDebug("Crawling {url}", TempUrl);

            try
            {
                HttpResponseMessage? Response = await Client.GetAsync(TempUrl).ConfigureAwait(false);
                if (Response is null)
                    return null;
                var FileName = GetFileName(Response);
                return new UrlData(
                    await Response.Content.ReadAsByteArrayAsync().ConfigureAwait(false) ?? [],
                    Response.Content.Headers.ContentType?.ToString() ?? "",
                    FileName,
                    Response.Headers.Location?.ToString() ?? Response.RequestMessage?.RequestUri?.ToString() ?? "",
                    (int)Response.StatusCode,
                    url
                );
            }
            catch (HttpRequestException E)
            {
                Logger?.LogError(E, "Error crawling {url}", TempUrl);
                var FileName = url;
                return new UrlData(
                    [],
                    "",
                    FileName,
                    url,
                    (int)HttpStatusCode.ServiceUnavailable,
                    url
                );
            }
        }

        /// <summary>
        /// Releases the underlying <see cref="HttpClient"/> instance and clears the engine's client reference.
        /// </summary>
        public void Dispose()
        {
            Client?.Dispose();
            Client = null;
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Builds the regular expression used to extract a file name from a Content-Disposition
        /// header value.
        /// </summary>
        /// <returns>A compiled regular expression for file name extraction.</returns>
        [GeneratedRegex(@"filename=[\""']?(?<FileName>[^\""\n\r']*)['\""\n\r]?$", RegexOptions.Compiled)]
        private static partial Regex GenerateFileNameRegex();

        /// <summary>
        /// Resolves the best available file name for a response using Content-Disposition first,
        /// then the response URL.
        /// </summary>
        /// <param name="response">HTTP response to inspect.</param>
        /// <returns>The extracted file name, or an empty string when no usable name can be determined.</returns>
        /// <remarks>
        /// If a Content-Disposition file name is present, surrounding quotes are removed.
        /// Otherwise, the file name is inferred from the last path segment of the response location
        /// or request URI and any query string is stripped.
        /// </remarks>
        private static string GetFileName(HttpResponseMessage? response)
        {
            if (response is null)
                return "";
            var FileName = "";
            if (!string.IsNullOrEmpty(response.Content.Headers.ContentDisposition?.FileName))
            {
                FileName = response.Content.Headers.ContentDisposition?.FileName ?? "";
            }
            if (!string.IsNullOrEmpty(FileName))
            {
                FileName = _FileNameRegex.Match(FileName).Groups["FileName"].Value;
            }
            if (string.IsNullOrEmpty(FileName))
            {
                var ResultURI = response.Headers.Location?.ToString() ?? response.RequestMessage?.RequestUri?.ToString() ?? "";
                FileName = ResultURI.Right(ResultURI.Length - ResultURI.LastIndexOf('/') - 1);
                if (FileName.Contains('?'))
                    FileName = FileName.Left(FileName.IndexOf('?'));
            }

            return FileName;
        }
    }
}