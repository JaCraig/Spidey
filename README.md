# <img src="https://jacraig.github.io/Spidey/images/icon.png" style="height:25px" alt="Spidey Icon" /> Spidey

[![.NET Publish](https://github.com/JaCraig/Spidey/actions/workflows/dotnet-publish.yml/badge.svg)](https://github.com/JaCraig/Spidey/actions/workflows/dotnet-publish.yml) [![NuGet](https://img.shields.io/nuget/v/Spidey.svg)](https://www.nuget.org/packages/Spidey/)

Spidey is a flexible and extensible .NET library for crawling web content. It is designed for .NET Core applications and provides a modular architecture, allowing you to customize or extend any part of the crawling pipeline.

## Features

- Simple API for crawling websites
- Highly configurable via the `Options` class
- Dependency injection support (IoC/DI)
- Easily replaceable subsystems (engine, parser, scheduler, etc.)
- Callback-based result handling
- NuGet package available

## Quick Start

Install the NuGet package:

```powershell
dotnet add package Spidey
```

## Setting up the Library

Register Spidey in your app's service collection using the `RegisterSpidey` extension method:

```csharp
using Microsoft.Extensions.DependencyInjection;
using Spidey;

var services = new ServiceCollection();
services.RegisterSpidey();

// Optionally, register your Options configuration
services.AddSingleton(new Options
{
    ItemFound = result => Console.WriteLine($"Found: {result.Url}"),
    Allow = new List<string> { "http://mywebsite", "http://mywebsite2" },
    FollowOnly = new List<string> { /* regex patterns */ },
    Ignore = new List<string> { /* regex patterns */ },
    StartLocations = new List<string> { "http://mywebsite", "http://mywebsite2" },
    UrlReplacements = new Dictionary<string, string> { /* { "old", "new" } */ },
    // Other options as needed
});

var provider = services.BuildServiceProvider();
var crawler = provider.GetRequiredService<Crawler>();
```

Alternatively, you can instantiate `Crawler` and `Options` directly without DI:

```csharp
var options = new Options
{
    ItemFound = result => Console.WriteLine($"Found: {result.Url}"),
    // ...other options
};
var crawler = new Crawler(options);
```

## Options Configuration

The `Options` class configures the crawler's behavior. Key properties include:

- `ItemFound` (`Action<ResultFile>`): Callback invoked when a new page is discovered.
- `Allow` (`List<string>`): Regex patterns for URLs allowed to be crawled.
- `FollowOnly` (`List<string>`): Regex patterns for pages whose links should be followed.
- `Ignore` (`List<string>`): Regex patterns for URLs to ignore.
- `StartLocations` (`List<string>`): Initial URLs to start crawling from.
- `UrlReplacements` (`Dictionary<string, string>`): URL replacements during crawling.
- `NetworkCredentials` (`NetworkCredential`): Optional credentials for authentication.
- `UseDefaultCredentials` (`bool`): Use default system credentials.
- `Proxy` (`IWebProxy`): Optional proxy settings.

Example callback method:

```csharp
void OnItemFound(ResultFile result)
{
    Console.WriteLine($"Discovered: {result.Url} (Status: {result.StatusCode})");
    // Additional processing...
}
```

## Basic Usage

Once configured, start the crawl process:

```csharp
crawler.StartCrawl();
```

The library will handle link discovery, content downloading, and result parsing. Your callback will be invoked for each discovered item.

## Customization

Spidey is built with extensibility in mind. The system is divided into the following subsystems, each replaceable via DI:

1. **Content Parser (`IContentParser`)** – Parses downloaded data into `ResultFile` objects.
2. **Engine (`IEngine`)** – Handles HTTP requests and content downloading.
3. **Link Discoverer (`ILinkDiscoverer`)** – Extracts links from content.
4. **Processor (`IProcessor`)** – Processes parsed content (default: invokes your callback).
5. **Scheduler (`IScheduler`)** – Manages work distribution.
6. **Pipeline (`IPipeline`)** – Orchestrates the crawling process.

To customize, implement the relevant interface from `Spidey.Engines.Interfaces` and register your implementation in the service provider. Note that if you call RegisterSpidey(), the registration is handled for you automatically. If you instantiate `Crawler` directly, you must compose the pipeline manually.

## FAQ

**Q: Can I run the crawler on multiple nodes?**

A: The default scheduler is single-node only. For distributed crawling, implement a custom scheduler (e.g., using a database or message queue) to coordinate work between instances.

## Build Process

Requirements:

- Visual Studio 2022

Clone the project and open the solution (`Spidey.sln`) in Visual Studio to build.

## License

See [LICENSE](LICENSE) for details.
