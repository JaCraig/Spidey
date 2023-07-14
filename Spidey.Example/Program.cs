using BigBook;
using Microsoft.Extensions.DependencyInjection;

namespace Spidey.Example
{
    /// <summary>
    /// Example program for Spidey
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        private static async Task Main(string[] args)
        {
            // We need to setup the service provider
            var Services = new ServiceCollection()
                // We need to first add the crawler and subsequent dependencies
                .AddCanisterModules()
                // And add our options
                ?.AddSingleton(new Options
                {
                    // We want to allow these locations to be crawled
                    Allow = { "https://ajax.googleapis.com/ajax/libs/jquery/3.4.1/jquery.min.js" },
                    // We want to use these credentials for authentication against the server
                    Credentials = new System.Net.NetworkCredential("username", "password"),
                    // We want to ignore these locations
                    Ignore = { "https://ajax.googleapis.com/ajax/libs/jquery/3.4.1/jquery.min.js" },
                    // When we find a new item, we want to print the filename to the console
                    ItemFound = x => System.Console.WriteLine(x.FileName),
                    // The maximum delay between requests
                    MaxDelay = 1000,
                    // The minimum delay between requests
                    MinDelay = 100,
                    // We want to start our crawl from these locations
                    StartLocations = { "https://ajax.googleapis.com/ajax/libs/jquery/3.4.1/jquery.min.js" },
                    // We want to use a proxy server at http://localhost:8888
                    Proxy = new System.Net.WebProxy("http://localhost:8888"),
                    // We want to use 4 workers
                    NumberWorkers = 4,
                    // We don't want to use the default credentials
                    UseDefaultCredentials = false
                })
                ?.BuildServiceProvider();
            if (Services is null)
                return;

            // Let's start by creating a crawler
            var Crawler = Services.GetRequiredService<Crawler>();

            // Now we can start the crawl
            var Results = await Crawler.StartCrawlAsync().ConfigureAwait(false);
            if (Results is null)
                return;
            // We can see the urls that were crawled
            Console.WriteLine("Found the following URLs:");
            Console.WriteLine(Results.CompletedURLs.ToString(x => x, "\n"));

            // We can also see where the urls were found
            Console.WriteLine("The following URLs were discovered from these locations:");
            Console.WriteLine(Results.WhereFound.ToString(x => $"{x.Key}: {x.Value.ToString(y => y)}", "\n"));

            // And the urls that had errors
            Console.WriteLine("And the following URLs had errors:");
            Console.WriteLine(Results.ErrorURLs.ToString(x => $"{x.Url} ({x.StatusCode}): {x.Error}", "\n"));
        }
    }
}